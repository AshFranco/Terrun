using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Terrun.Domain.Activities;

namespace Terrun.Infrastructure.Persistence.Configurations;

internal sealed class SplitConfiguration : IEntityTypeConfiguration<Split>
{
    public void Configure(EntityTypeBuilder<Split> builder)
    {
        builder.ToTable("split");
        builder.HasKey(s => new { s.ActivityId, s.Number });
        builder.Property(s => s.DistanceM).HasPrecision(7, 2);
        builder.Property(s => s.ElevationM).HasPrecision(6, 1);
        builder.HasOne<Activity>().WithMany().HasForeignKey(s => s.ActivityId).OnDelete(DeleteBehavior.Cascade);
    }
}
