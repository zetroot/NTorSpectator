namespace NTorSpectator.Services.Models;

/// <summary>
/// One event for the site: came up or gone down
/// </summary>
public class AvailabilityEvent
{
    /// <summary>
    /// Site url
    /// </summary>
    public required string SiteUri { get; init; }
    
    /// <summary>
    /// Site came up or gone down
    /// </summary>
    public EventType Kind { get; set; }
    
    /// <summary>
    /// When the event has occured
    /// </summary>
    public DateTime OccuredAt { get; set; }
    
    /// <summary>
    /// Event types - down or up
    /// </summary>
    public enum EventType
    {
        /// <summary>
        /// No value
        /// </summary>
        None = 0,
        
        /// <summary>
        /// Site gone down
        /// </summary>
        Down = -1,
        
        /// <summary>
        /// Site came up
        /// </summary>
        Up = 1
    }
}
