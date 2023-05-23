using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace NTorSpectator.HealthChecks;

public class TorAccessibilityCheck : IHealthCheck
{
    private readonly AppOptions _opts;

    public TorAccessibilityCheck(IOptions<AppOptions> options) => _opts = options.Value;
    
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_opts.TorSocket))
            return Task.FromResult(HealthCheckResult.Unhealthy("Tor socket not found"));
        
        if (!File.Exists(_opts.TorCookie))
            return Task.FromResult(HealthCheckResult.Unhealthy("Tor cookie not found"));

        return Task.FromResult(HealthCheckResult.Healthy());
    }
}