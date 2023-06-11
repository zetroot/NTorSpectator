using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using NTorSpectator.TorIntegration;

namespace NTorSpectator.HealthChecks;

public class TorAccessibilityCheck : IHealthCheck
{
    private readonly TorSettings _opts;

    public TorAccessibilityCheck(IOptions<TorSettings> options) => _opts = options.Value;
    
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_opts.TorSocket))
            return Task.FromResult(HealthCheckResult.Unhealthy("Tor socket not found"));
        
        if (!File.Exists(_opts.TorCookie))
            return Task.FromResult(HealthCheckResult.Unhealthy("Tor cookie not found"));
        
        return Task.FromResult(HealthCheckResult.Healthy());
    }
}
