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
    private readonly TorControlManager _torControl;
    private readonly ISiteObserver _siteObserver;
    private readonly IReporter _reporter;

    public SpectatorJob(ILogger<SpectatorJob> logger, ISitesCatalogue sitesCatalogue, TorControlManager torControl, ISiteObserver siteObserver, IReporter reporter)
    {
        _logger = logger;
        _sitesCatalogue = sitesCatalogue;
        _torControl = torControl;
        _siteObserver = siteObserver;
        _reporter = reporter;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        TotalSessionDuration.Set(0);
        var sw = Stopwatch.StartNew();
        _logger.LogDebug("Starting sites observations");
        var sites = await _sitesCatalogue.GetAllSites();
        _logger.LogDebug("Got {Count} sites to observe", sites.Count);
        
        var siteQueue = new Queue<QueuedSite>(sites.Select(x => new QueuedSite(x, 0)));
        var report = new Report();
        while(siteQueue.TryDequeue(out var queuedSite))
        {
            QueueLength.Set(siteQueue.Count);
            using var _ = _logger.BeginScope(new Dictionary<string, object> { { "HiddenService", queuedSite.Site.SiteUri } });
            _logger.LogDebug("Starting observations on the next site");
            try
            {
                var observations = await ObserveSite(queuedSite.Site.SiteUri);
                ObservationsCount.Inc();
                if (!observations.IsOk)
                {
                    _logger.LogDebug("Site observed as not available");
                    var siteObservationsCount = queuedSite.ObservationsCount;
                    if (siteObservationsCount < 3)
                    {
                        _logger.LogDebug("Site has been observed {Count} times, returning it to queue", siteObservationsCount);
                        siteQueue.Enqueue(queuedSite with{ObservationsCount = siteObservationsCount + 1});
                        RetriesCount.Inc();
                        continue;
                    }
                }

                var observationModel = new Observation { SiteUri = queuedSite.Site.SiteUri, ObservedAt = DateTime.UtcNow, IsAvailable = observations.IsOk };
                var maybeEvent = await _siteObserver.GenerateEvent(observationModel);
                
                report.Observations.Add(observationModel);
                if (maybeEvent is { } siteEvent)
                {
                    _logger.LogDebug("Should report site state change");
                    report.Events.Add(siteEvent);
                }
                
                SiteStatus.WithLabels(queuedSite.Site.SiteUri).Set(observations.IsOk ? 1 : 0);
                _logger.LogInformation("Site observed");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Observation for site failed");
            }
        }
        _logger.LogDebug("The queue is finally empty, observations finished");
        await _siteObserver.SaveReport(report);
        _logger.LogInformation("Report saved!");
        await _reporter.PublishReportData(report);
        _logger.LogInformation("Report published!");
        sw.Stop();
        TotalSessionDuration.Set(sw.ElapsedMilliseconds);
    }
    
    private record QueuedSite(Site Site, int ObservationsCount);
    
    private async Task<TorWatchResults> ObserveSite(string site)
    {
        using var _ = RequestDuration.NewTimer();
        var torReply = await _torControl.HsFetch(site);
        var positive = torReply.Count(x => x.Action == HsDescAction.Received);
        var negative = torReply.Count(x => x.Action == HsDescAction.Failed);
        return new(site, positive, negative);
    }
}
