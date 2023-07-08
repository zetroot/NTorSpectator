namespace NTorSpectator.Database.Models;

/// <summary>
/// Report is a collection of observations
/// </summary>
public class ObservationReport
{
    /// <summary>
    /// PK
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Observations included in this report
    /// </summary>
    public ICollection<Observation> Observations { get; set; } = null!;

    /// <summary>
    /// events occured in this report
    /// </summary>
    public ICollection<SiteAvailabilityEvent> Events { get; set; } = null!;
}
