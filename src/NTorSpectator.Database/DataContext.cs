using Microsoft.EntityFrameworkCore;
using NTorSpectator.Database.Models;

namespace NTorSpectator.Database;

public class DataContext : DbContext
{
    public DbSet<Site> Sites => Set<Site>();
    public DbSet<Observation> Observations => Set<Observation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
