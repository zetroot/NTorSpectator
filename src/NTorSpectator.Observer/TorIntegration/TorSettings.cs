namespace NTorSpectator.Observer.TorIntegration;

/// <summary>
/// Settings for tor daemon interactions 
/// </summary>
public class TorSettings
{
    /// <summary>
    /// Tor daemon control socket path
    /// </summary>
    public required string TorSocket { get; init; } = "/run/tor/control";
    
    /// <summary>
    /// Tor daemon cookie for authentication in control socket
    /// </summary>
    public required string TorCookie { get; init; } = "/run/tor/control.authcookie";
    
    /// <summary>
    /// Wait for service descriptors for this period of time
    /// </summary>
    public required TimeSpan ReplyReceiveTimeout { get; init; } = TimeSpan.FromSeconds(10);
}
