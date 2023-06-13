namespace NTorSpectator.Services.Models;

/// <summary>
/// site being observed in service
/// </summary>
public class Site
{
    /// <summary>
    /// Onion uri for the site 
    /// </summary>
    public required string SiteUri { get; init; }
}
