namespace NTorSpectator.Mastodon;

/// <summary>
/// Settings for mastodon bot connection
/// </summary>
public class MastodonSettings
{
    /// <summary>
    /// Instance Uri - where API is accessible
    /// </summary>
    public required Uri Instance { get; init; }
    
    /// <summary>
    /// API access token to use
    /// </summary>
    public required string Token { get; init; }
}
