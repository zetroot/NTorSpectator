using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NTorSpectator.Database.Models;

namespace NTorSpectator.Database.Configuration;

internal class ObservationEntityConfiguration : IEntityTypeConfiguration<Observation>
{
    public void Configure(EntityTypeBuilder<Observation> builder)
    {
        builder.ToTable("observations");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.ObservedAt);
        builder.HasIndex(x => x.SiteId);
        
        builder.HasOne(x => x.Site).WithMany().HasForeignKey(x => x.SiteId).OnDelete(DeleteBehavior.Restrict);
    }
}
