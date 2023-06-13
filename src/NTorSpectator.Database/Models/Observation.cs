namespace NTorSpectator.Database.Models;

/// <summary>
/// One observation record
/// </summary>
public class Observation
{
    /// <summary>
    /// PK in db
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// When the observation occured
    /// </summary>
    public DateTime ObservedAt { get; set; }
    
    /// <summary>
    /// FK to sites
    /// </summary>
    public int SiteId { get; set; }

    /// <summary>
    /// Observed site
    /// </summary>
    public Site Site { get; set; } = null!;
    
    /// <summary>
    /// Is available
    /// </summary>
    public bool IsAvailable { get; set; }
}
