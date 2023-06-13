using Prometheus;

namespace NTorSpectator.Observer.HealthChecks;

public static class HealthCheckExtensions
{
    public static IServiceCollection RegisterApplicationHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<TorAccessibilityCheck>();

        services.AddHealthChecks()
            .AddCheck<TorAccessibilityCheck>(name: "tor-daemon", tags: new[] { "health", "ready", "alive" })
            .ForwardToPrometheus();

        return services;
    }
}
