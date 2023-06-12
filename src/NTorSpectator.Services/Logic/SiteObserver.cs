using Microsoft.Extensions.Logging;
using NTorSpectator.Services.Persistent;

namespace NTorSpectator.Services.Logic;

public class SiteObserver : ISiteObserver
{
    private readonly IReporter _reporter;
    private readonly IObservesRepository _repo;
    private readonly ILogger<SiteObserver> _logger;

    public SiteObserver(IReporter reporter, IObservesRepository repo, ILogger<SiteObserver> logger)
    {
        _reporter = reporter;
        _repo = repo;
        _logger = logger;
    }

    public async Task AddNewObservation(string siteUri, bool isAvailable)
    {
        using var _ = _logger.BeginScope(new Dictionary<string, object> { { "SiteUri", siteUri }, { "IsAvailable", isAvailable } });
        _logger.LogDebug("Got new observation to save");
        var previousObservation = await _repo.GetLastObservationForSite(siteUri);
        await _repo.AddNewObservation(siteUri, isAvailable, DateTime.Now);
        _logger.LogInformation("Saved new observation");
        if (previousObservation is null)
        {
            _logger.LogDebug("This is first observation for site. Won't compare with previous");
            return;
        }

        if (previousObservation.IsAvailable == isAvailable)
        {
            _logger.LogDebug("Site availability has not changed, nothing to report");
            return;   
        }

        if (isAvailable)
        {
            await _reporter.ReportCameUp(siteUri);
            _logger.LogInformation("Reported site came up");
        }
        else
        {
            await _reporter.ReportWentDown(siteUri, previousObservation.ObservedAt);
            _logger.LogInformation("Reported site went down");
        }
        
    }
}
