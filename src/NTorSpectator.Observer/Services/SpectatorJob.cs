using NTorSpectator.Observer.TorIntegration;
using NTorSpectator.Services;
using NTorSpectator.Services.Models;
using Quartz;

namespace NTorSpectator.Observer.Services;

public class SpectatorJob : IJob
{
    private readonly ILogger<SpectatorJob> _logger;
    private readonly ISitesCatalogue _sitesCatalogue;
    private readonly TorControlManager _torControl;
    private readonly ISiteObserver _siteObserver;

    public SpectatorJob(ILogger<SpectatorJob> logger, ISitesCatalogue sitesCatalogue, TorControlManager torControl, ISiteObserver siteObserver)
    {
        _logger = logger;
        _sitesCatalogue = sitesCatalogue;
        _torControl = torControl;
        _siteObserver = siteObserver;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var sites = await _sitesCatalogue.GetAllSites();
        var siteQueue = new Queue<QueuedSite>(sites.Select(x => new QueuedSite(x, 0)));
        
        while(siteQueue.TryDequeue(out var queuedSite))
        {
            using var _ = _logger.BeginScope(new Dictionary<string, object> { { "HiddenService", queuedSite.Site.SiteUri } });
            try
            {
                var observations = await ObserveSite(queuedSite.Site.SiteUri);
                if (!observations.IsOk)
                {
                    _logger.LogDebug("Site observed as not available");
                    var siteObservationsCount = queuedSite.ObservationsCount;
                    if (siteObservationsCount < 3)
                    {
                        _logger.LogDebug("Site has been observed {Count} times, returning it to queue", siteObservationsCount);
                        siteQueue.Enqueue(queuedSite with{ObservationsCount = siteObservationsCount + 1});
                        continue;
                    }
                }
                await _siteObserver.AddNewObservation(queuedSite.Site.SiteUri, observations.IsOk);
                _logger.LogInformation("Site observed");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Observation for site failed");
            }
        }
    }
    
    private record QueuedSite(Site Site, int ObservationsCount);
    
    private async Task<TorWatchResults> ObserveSite(string site)
    {
        var torReply = await _torControl.HsFetch(site);
        var positive = torReply.Count(x => x.Action == HsDescAction.Received);
        var negative = torReply.Count(x => x.Action == HsDescAction.Failed);
        return new(site, positive, negative);
    }
}
