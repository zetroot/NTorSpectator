using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NTorSpectator.Database.Models;

namespace NTorSpectator.Database.Configuration;

internal class ObservationEventEntityConfiguration : IEntityTypeConfiguration<SiteAvailabilityEvent>
{
    public void Configure(EntityTypeBuilder<SiteAvailabilityEvent> builder)
    {
        builder.ToTable("availability_events");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.OccuredAt);
        builder.HasIndex(x => x.EventType);
        builder.HasOne(x => x.Site).WithMany().HasForeignKey(x => x.SiteId).OnDelete(DeleteBehavior.Cascade);
    }
}
