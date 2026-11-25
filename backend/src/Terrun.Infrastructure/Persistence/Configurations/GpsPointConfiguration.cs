using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Terrun.Domain.Activities;

namespace Terrun.Infrastructure.Persistence.Configurations;

internal sealed class GpsPointConfiguration : IEntityTypeConfiguration<GpsPoint>
{
    public void Configure(EntityTypeBuilder<GpsPoint> builder)
    {
        builder.ToTable("gps_point");
        builder.HasKey(p => new { p.ActivityId, p.Seq });
        builder.Property(p => p.IsDiscarded).HasDefaultValue(false);
        builder.HasOne<Activity>().WithMany().HasForeignKey(p => p.ActivityId).OnDelete(DeleteBehavior.Cascade);
    }
}
