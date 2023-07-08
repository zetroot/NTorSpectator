namespace NTorSpectator.Database.Models;

/// <summary>
/// Single event
/// </summary>
public class SiteAvailabilityEvent
{
    /// <summary>
    /// PK
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// FK to observation report in which this event occured
    /// </summary>
    public int ReportId { get; set; }

    /// <summary>
    /// FK to sites - what site generated this event
    /// </summary>
    public int SiteId { get; set; }

    /// <summary>
    /// Navigation property - site
    /// </summary>
    public Site Site { get; set; } = null!;
    
    /// <summary>
    /// Event occured at
    /// </summary>
    public DateTime OccuredAt { get; set; }
    
    /// <summary>
    /// event type - site up, site down
    /// </summary>
    public int EventType { get; set; }
}
