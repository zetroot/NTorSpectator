using NTorSpectator.Services.Models;

namespace NTorSpectator.Services.Persistent;

public interface IObservesRepository
{
    Task<Observation?> GetLastObservationForSite(string siteUri);
    Task AddNewObservation(string siteUri, bool isAvailable, DateTime timestamp);
}
