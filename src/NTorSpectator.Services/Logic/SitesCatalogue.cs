using Microsoft.Extensions.Logging;
using NTorSpectator.Services.Models;
using NTorSpectator.Services.Persistent;

namespace NTorSpectator.Services.Logic;

/// <summary>
/// sites catalogue implementation
/// </summary>
internal class SitesCatalogue : ISitesCatalogue
{
    private readonly ISitesRepository _repository;
    private readonly ILogger<SitesCatalogue> _logger;

    public SitesCatalogue(ILogger<SitesCatalogue> logger, ISitesRepository repository)
    {
        _logger = logger;
        _repository = repository;
    }

    /// <inheritdoc cref="ISitesCatalogue.AddIfNotExists"/>
    public async Task<bool> AddIfNotExists(string siteUri)
    {
        using var _ = _logger.BeginScope(new Dictionary<string, object> { { "SiteUri", siteUri } });
        _logger.LogDebug("Trying to add new site into catalogue");
        var sites = await _repository.GetAllSites();
        if (sites.Any(x => string.Equals(x.SiteUri, siteUri, StringComparison.OrdinalIgnoreCase)))
        {
            _logger.LogDebug("Site already exists, wont add new");
            return false;
        }
        else
        {
            _logger.LogDebug("Site does not exists, will add new record");
            await _repository.AddNewSite(siteUri);
            _logger.LogInformation("New site added!");
            return true;
        }
    }

    public Task<IReadOnlyCollection<Site>> GetAllSites() => _repository.GetAllSites();
}
