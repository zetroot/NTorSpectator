using Microsoft.Extensions.Options;
using NTorSpectator.Observer.TorIntegration;
using NTorSpectator.Services;
using NTorSpectator.Services.Models;

namespace NTorSpectator.Observer.Services;

public class Spectator : BackgroundService
{
    private readonly ILogger<Spectator> _logger;
    private readonly TorControlManager _torControl;
    private readonly string _torSitesFile;
    private readonly TimeSpan _cooldown;
    private readonly ISitesCatalogue _sitesCatalogue;
    private readonly ISiteObserver _siteObserver;

    public Spectator(ILogger<Spectator> logger, TorControlManager torControl, IOptions<SpectatorSettings> opts, ISitesCatalogue sitesCatalogue, ISiteObserver siteObserver)
    {
        _logger = logger;
        _torControl = torControl;
        _sitesCatalogue = sitesCatalogue;
        _siteObserver = siteObserver;
        _torSitesFile = opts.Value.SiteList;
        _cooldown = opts.Value.CooldownInterval;
    }

    public bool IsRunning { get; private set; }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            IsRunning = true;
            await UpdateSites();
            while (!stoppingToken.IsCancellationRequested)
            {
                await Watch();
                await Task.Delay(_cooldown, stoppingToken);
            }
        }
        finally
        {
            IsRunning = false;
        }
    }

    private async Task Watch()
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

    private async Task UpdateSites()
    {
        var sites = await File.ReadAllLinesAsync(_torSitesFile);
        foreach (var site in sites)
        {
            await _sitesCatalogue.AddIfNotExists(site);
        }
    }
    private async Task<TorWatchResults> ObserveSite(string site)
    {
        var torReply = await _torControl.HsFetch(site);
        var positive = torReply.Count(x => x.Action == HsDescAction.Received);
        var negative = torReply.Count(x => x.Action == HsDescAction.Failed);
        return new(site, positive, negative);
    }

    private record QueuedSite(Site Site, int ObservationsCount);
}

public record TorWatchResults(string Site, int Positive, int Negative)
{
    public bool IsOk => Positive > 0;
}
