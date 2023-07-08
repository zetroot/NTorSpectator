using NTorSpectator.Services.Models;

namespace NTorSpectator.Services;

/// <summary>
/// Service which deals with sites observations 
/// </summary>
public interface ISiteObserver
{
    /// <summary>
    /// Saves report into database
    /// </summary>
    /// <param name="report">Report to save</param>
    Task SaveReport(Report report);

    /// <summary>
    /// Generate event for this observation
    /// </summary>
    /// <param name="observation">Observation to generate event based on it</param>
    /// <returns>Event if site availability changed. Null - otherwise</returns>
    Task<AvailabilityEvent?> GenerateEvent(Observation observation);
}
