using Microsoft.Extensions.DependencyInjection;
using NTorSpectator.Services.Logic;
using NTorSpectator.Services.Persistent;

namespace NTorSpectator.Services;

public static class BizlogicInjector
{
    public static IServiceCollection AddBizLogic(this IServiceCollection services)
    {
        services.AddTransient<ISitesCatalogue, SitesCatalogue>()
            .AddTransient<ISiteObserver, SiteObserver>();
        return services;
    }
}
