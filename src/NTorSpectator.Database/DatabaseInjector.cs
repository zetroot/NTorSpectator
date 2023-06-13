using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NTorSpectator.Database.Repositories;
using NTorSpectator.Services.Persistent;

namespace NTorSpectator.Database;

public static class DatabaseInjector
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContextPool<DataContext>(opts => opts.UseNpgsql(configuration.GetConnectionString("SpectatorDatabase")));
        services
            .AddTransient<ISitesRepository, SitesRepository>()
            .AddTransient<IObservesRepository, ObservesRepository>();
        
        return services;
    }
}
