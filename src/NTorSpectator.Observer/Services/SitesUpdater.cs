using Microsoft.Extensions.Options;
using NTorSpectator.Services;

namespace NTorSpectator.Observer.Services;

public class SitesUpdater : BackgroundService
{
    private readonly ILogger<SitesUpdater> _logger;
    private readonly string _torSitesFile;
    private readonly ISitesCatalogue _sitesCatalogue;

    public SitesUpdater(ILogger<SitesUpdater> logger, IOptions<SpectatorSettings> opts, ISitesCatalogue sitesCatalogue)
    {
        _logger = logger;
        _sitesCatalogue = sitesCatalogue;
        _torSitesFile = opts.Value.SiteList;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!File.Exists(_torSitesFile))
        {
            _logger.LogWarning("Tor sites file is not found at path {TorSitesFilePath}. Exiting", _torSitesFile);
            return;
        }
        
        var sites = await File.ReadAllLinesAsync(_torSitesFile);
        foreach (var site in sites)
        {
            if (string.IsNullOrWhiteSpace(site))
                continue;
            await _sitesCatalogue.AddIfNotExists(site);
        }
        _logger.LogDebug("Finished adding sites");
    }
}
