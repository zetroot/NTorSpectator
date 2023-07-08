using Microsoft.Extensions.DependencyInjection;
using NTorSpectator.Services.Logic;
using NTorSpectator.Services.Persistent;

namespace NTorSpectator.Services;

/// <summary>
/// Injects business logic into application
/// </summary>
public static class BizlogicInjector
{
    /// <summary>
    /// Inject bizlogic services into DI
    /// </summary>
    /// <param name="services">DI containEr</param>
    /// <returns></returns>
    public static IServiceCollection AddBizLogic(this IServiceCollection services)
    {
        services.AddTransient<ISitesCatalogue, SitesCatalogue>()
            .AddTransient<ISiteObserver, SiteObserver>();
        return services;
    }
}
