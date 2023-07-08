using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NTorSpectator.Database.Repositories;
using NTorSpectator.Services.Persistent;

namespace NTorSpectator.Database;

/// <summary>
/// Injects data access services into DI
/// </summary>
public static class DatabaseInjector
{
    /// <summary>
    /// Registers Datacontext and repositories implementations in DI-container
    /// </summary>
    /// <param name="services">DI-container</param>
    /// <param name="configuration">Configuration to get database settings from</param>
    /// <returns>Same di container to chain extension calls</returns>
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContextPool<DataContext>(opts => opts.UseNpgsql(configuration.GetConnectionString("SpectatorDatabase")));
        services
            .AddTransient<ISitesRepository, SitesRepository>()
            .AddTransient<IObservesRepository, ObservesRepository>();
        
        return services;
    }
}
