using Microsoft.Extensions.Logging;
using NTorSpectator.Services.Models;
using NTorSpectator.Services.Persistent;

namespace NTorSpectator.Services.Logic;

internal class SiteObserver : ISiteObserver
{
    private readonly IObservesRepository _observationsRepository;
    private readonly ILogger<SiteObserver> _logger;
    private readonly IReportsRepository _reportsRepository;

    public SiteObserver(IReporter reporter, IObservesRepository observationsRepository, ILogger<SiteObserver> logger, IReportsRepository reportsRepository)
    {
        _observationsRepository = observationsRepository;
        _logger = logger;
        _reportsRepository = reportsRepository;
    }

    public async Task<AvailabilityEvent?> GenerateEvent(Observation observation)
    {
        using var _ = _logger.BeginScope(new Dictionary<string, object> { { "SiteUri", observation.SiteUri }, { "IsAvailable", observation.IsAvailable } });
        _logger.LogDebug("Got new observation to save");
        var previousObservation = await _observationsRepository.GetLastObservationForSite(observation.SiteUri);
        if (previousObservation is null)
        {
            _logger.LogDebug("This is first observation for site. Won't compare with previous");
            return null;
        }

        if (previousObservation.IsAvailable == observation.IsAvailable)
        {
            _logger.LogDebug("Site availability has not changed, won't create event");
            return null;   
        }

        _logger.LogDebug("Site availability changed! Will create new event");
        return new()
        {
            SiteUri = observation.SiteUri,
            OccuredAt = observation.ObservedAt,
            Kind = observation.IsAvailable ? AvailabilityEvent.EventType.Up : AvailabilityEvent.EventType.Down
        }; 
    }

    public Task SaveReport(Report report) => _reportsRepository.SaveReport(report);
}
