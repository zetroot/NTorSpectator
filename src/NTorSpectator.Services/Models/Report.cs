namespace NTorSpectator.Services.Models;

/// <summary>
/// Combines observations and events happened in one run
/// </summary>
public class Report
{
    /// <summary>
    /// Observations done in this report
    /// </summary>
    public ICollection<Observation> Observations { get; set; } = new List<Observation>();
    
    /// <summary>
    /// Events occured while report generating 
    /// </summary>
    public ICollection<AvailabilityEvent> Events { get; set; } = new List<AvailabilityEvent>();
}
