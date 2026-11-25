using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Terrun.Domain.Activities;
using Terrun.Domain.Equipment;
using Terrun.Domain.Identity;

namespace Terrun.Infrastructure.Persistence.Configurations;

// D-34: no two activities of a runner overlap in time, except the rejected and failed ones. EF cannot express an EXCLUDE
// constraint, so the migration creates it with SQL (DatabaseConstraints.ActivityNoOverlap); the model never drops it
internal sealed class ActivityConfiguration : IEntityTypeConfiguration<Activity>
{
    public void Configure(EntityTypeBuilder<Activity> builder)
    {
        builder.ToTable("activity");
        builder.Property(a => a.Id).HasDefaultValueSql(ColumnDefaults.NewUuid);
        builder.Property(a => a.Title).HasMaxLength(80);
        builder.Property(a => a.Description).HasMaxLength(500);
        builder.Property(a => a.Status).HasDefaultValue(ActivityStatus.Pending);
        builder.Property(a => a.DistanceM).HasPrecision(10, 2);
        builder.Property(a => a.MaxSpeedMps).HasPrecision(5, 2);
        builder.Property(a => a.ElevationGainM).HasPrecision(7, 1);
        builder.Property(a => a.ElevationLossM).HasPrecision(7, 1);
        builder.Property(a => a.Route).HasColumnType("geography (linestring, 4326)");
        builder.Property(a => a.CreatedAt).HasDefaultValueSql(ColumnDefaults.Now);
        builder.Property(a => a.UpdatedAt).HasDefaultValueSql(ColumnDefaults.Now);

        // MT-09: the same clientId of the same runner is the same activity
        builder.HasIndex(a => new { a.UserId, a.ClientId }).IsUnique().HasDatabaseName(DatabaseConstraints.ActivityClientIdUnique);

        // History (/me/activities, newest first), the processor queue (§6) and the map queries on the route
        builder.HasIndex(a => new { a.UserId, a.StartedAt }).IsDescending(false, true);
        builder.HasIndex(a => a.CreatedAt).HasFilter("status = 'pending'").HasDatabaseName("ix_activity_pending_created_at");
        builder.HasIndex(a => a.Route).HasMethod("gist");

        builder.ToTable(table => table.HasCheckConstraint("ck_activity_ended_after_started", "ended_at > started_at"));
        builder.HasOne<AppUser>().WithMany().HasForeignKey(a => a.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Shoe>().WithMany().HasForeignKey(a => a.ShoeId).OnDelete(DeleteBehavior.SetNull);
    }
}
