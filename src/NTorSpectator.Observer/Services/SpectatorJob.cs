using System.Collections.Concurrent;
using System.Diagnostics;
using NTorSpectator.Observer.TorIntegration;
using NTorSpectator.Services;
using NTorSpectator.Services.Models;
using Prometheus;
using Quartz;

namespace NTorSpectator.Observer.Services;

public class SpectatorJob : IJob
{
    private static readonly Gauge QueueLength = Metrics.CreateGauge("sites_queue_length", "Length of the queue left to observe");
    private static readonly Counter ObservationsCount = Metrics.CreateCounter("observations", "counts all observations");
    private static readonly Counter RetriesCount = Metrics.CreateCounter("enqueued_retries", "counts retires");
    private static readonly Histogram RequestDuration = Metrics.CreateHistogram("observation_duration", "duration of site observation",
        new HistogramConfiguration
        {
            Buckets = Histogram.LinearBuckets(0.5, 0.5, 20)
        });
    private static readonly Gauge TotalSessionDuration = Metrics.CreateGauge("observation_session_duration", "Total observation session duration, ms");
    private static readonly Gauge SiteStatus = Metrics.CreateGauge("site_aliveness", "Reports site aliveness status", "site");
    
    private readonly ILogger<SpectatorJob> _logger;
    private readonly ISitesCatalogue _sitesCatalogue;
    private readonly IServiceProvider _services;
    
    public SpectatorJob(ILogger<SpectatorJob> logger, ISitesCatalogue sitesCatalogue, IServiceProvider services)
    {
        _logger = logger;
        _sitesCatalogue = sitesCatalogue;
        _services = services;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        TotalSessionDuration.Set(0);
        var sw = Stopwatch.StartNew();
        _logger.LogDebug("Starting sites observations");
        var sites = await _sitesCatalogue.GetAllSites();
        _logger.LogDebug("Got {Count} sites to observe", sites.Count);
        var queuedSites = sites.Select(x => new QueuedSite(x, 0));
        var siteQueue = new ConcurrentQueue<QueuedSite>(queuedSites);
        while(true)
        {
            QueueLength.Set(siteQueue.Count);
            if (!siteQueue.Any())
                break;
            var chunk = TryDequeueMax(siteQueue, 4);
            var tasks = chunk.Select(x => TryCheckSite(x, s => siteQueue.Enqueue(s))).ToArray();
            await Task.WhenAll(tasks);
        }
        _logger.LogDebug("The queue is finally empty, observations finished");
        sw.Stop();
        TotalSessionDuration.Set(sw.ElapsedMilliseconds);
    }


    private QueuedSite[] TryDequeueMax(ConcurrentQueue<QueuedSite> queue, int max)
    {
        var result = new List<QueuedSite>(max);
        for (int i = 0; i < max; ++i)
        {
            if (queue.TryDequeue(out var item))
                result.Add(item);
        }
        return result.ToArray();
    }
    
    private async Task TryCheckSite(QueuedSite queuedSite, Action<QueuedSite> putBackIntoQueue)
    {
        using var _ = _logger.BeginScope(new Dictionary<string, object> { { "HiddenService", queuedSite.Site.SiteUri } });
        using var scope = _services.CreateScope();
        var torControl = scope.ServiceProvider.GetRequiredService<TorControlManager>();
        var siteObserver = scope.ServiceProvider.GetRequiredService<ISiteObserver>();
        _logger.LogDebug("Starting observations on the next site");
        try
        {
            var observations = await ObserveSite(queuedSite.Site.SiteUri, torControl);
            ObservationsCount.Inc();
            if (!observations.IsOk)
            {
                _logger.LogDebug("Site observed as not available");
                var siteObservationsCount = queuedSite.ObservationsCount;
                if (siteObservationsCount < 3)
                {
                    _logger.LogDebug("Site has been observed {Count} times, returning it to queue", siteObservationsCount);
                    putBackIntoQueue(queuedSite with { ObservationsCount = siteObservationsCount + 1 });
                    RetriesCount.Inc();
                    return;
                }
            }
            _logger.LogDebug("Site seems to be up");
            await siteObserver.AddNewObservation(queuedSite.Site.SiteUri, observations.IsOk);
            SiteStatus.WithLabels(queuedSite.Site.SiteUri).Set(observations.IsOk ? 1 : 0);
            _logger.LogInformation("Site observed");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Observation for site failed");
        }
    }

    private record QueuedSite(Site Site, int ObservationsCount);
    
    private async Task<TorWatchResults> ObserveSite(string site, TorControlManager torControl)
    {
        using var _ = RequestDuration.NewTimer();
        var torReply = await torControl.HsFetch(site);
        var positive = torReply.Count(x => x.Action == HsDescAction.Received);
        var negative = torReply.Count(x => x.Action == HsDescAction.Failed);
        return new(site, positive, negative);
    }
}
