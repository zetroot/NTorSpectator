using NTorSpectator.Services.Models;

namespace NTorSpectator.Services.Persistent;

/// <summary>
/// Repository for observations
/// </summary>
public interface IObservesRepository
{
    /// <summary>
    /// Get last observation for the site
    /// </summary>
    /// <param name="siteUri">Site uri</param>
    /// <returns>Null if there is not observations, instance if there is one</returns>
    Task<Observation?> GetLastObservationForSite(string siteUri);
}
