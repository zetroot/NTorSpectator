namespace NTorSpectator.Observer.Services;

/// <summary>
/// Settings for spectator: where to watch for sites, how often watch for sites
/// </summary>
public class SpectatorSettings
{
    /// <summary>
    /// File to look for tor sites list
    /// </summary>
    public required string SiteList { get; init; } = "sites.txt";

    /// <summary>
    /// Time to wait between requests
    /// </summary>
    public required TimeSpan CooldownInterval { get; init; } = TimeSpan.FromMinutes(5);
}
