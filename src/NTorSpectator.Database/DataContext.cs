using Microsoft.EntityFrameworkCore;
using NTorSpectator.Database.Models;

namespace NTorSpectator.Database;

internal class DataContext : DbContext
{
    public DataContext() { }
    
    public DataContext(DbContextOptions<DataContext> options) : base(options) { }

    public DbSet<Site> Sites => Set<Site>();
    public DbSet<Observation> Observations => Set<Observation>();
    public DbSet<SiteAvailabilityEvent> Events => Set<SiteAvailabilityEvent>();
    public DbSet<ObservationReport> Reports => Set<ObservationReport>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
