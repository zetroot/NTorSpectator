namespace NTorSpectator.Observer.Mastodon;

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

    /// <summary>
    /// Maximum message length
    /// </summary>
    public required int MessageLimit { get; init; } = 500;
}
