namespace NTorSpectator.Services.Models;

/// <summary>
/// One observation record
/// </summary>
public class Observation
{
    /// <summary>
    /// When the observation occured
    /// </summary>
    public DateTime ObservedAt { get; set; }
    
    /// <summary>
    /// Is available
    /// </summary>
    public bool IsAvailable { get; set; }
}
