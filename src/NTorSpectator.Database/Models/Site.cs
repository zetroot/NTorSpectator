namespace NTorSpectator.Database.Models;

/// <summary>
/// Site that is observed
/// </summary>
public class Site
{
    /// <summary>
    /// PK in database
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Onion site uri
    /// </summary>
    public string SiteUri { get; set; } = string.Empty;
}
