using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NTorSpectator.Database.Models;

namespace NTorSpectator.Database.Configuration;

internal class ObservationReportEntityConfiguration : IEntityTypeConfiguration<ObservationReport>
{
    public void Configure(EntityTypeBuilder<ObservationReport> builder)
    {
        builder.ToTable("reports");
        builder.HasKey(x => x.Id);
        
        builder.HasMany(x => x.Observations).WithOne().HasForeignKey(x => x.ReportId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(x => x.Events).WithOne().HasForeignKey(x => x.ReportId).OnDelete(DeleteBehavior.Restrict);
    }
}