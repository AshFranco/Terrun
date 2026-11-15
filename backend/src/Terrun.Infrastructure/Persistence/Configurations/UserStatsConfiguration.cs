using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Terrun.Domain.Identity;
using Terrun.Domain.Profiles;

namespace Terrun.Infrastructure.Persistence.Configurations;

internal sealed class UserStatsConfiguration : IEntityTypeConfiguration<UserStats>
{
    public void Configure(EntityTypeBuilder<UserStats> builder)
    {
        builder.ToTable("user_stats");
        builder.HasKey(s => s.UserId);
        builder.Property(s => s.TotalDistanceM).HasDefaultValue(0L);
        builder.Property(s => s.TotalTimeS).HasDefaultValue(0L);
        builder.Property(s => s.TotalActivities).HasDefaultValue(0);
        builder.Property(s => s.ZonesConquered).HasDefaultValue(0);
        builder.Property(s => s.ZonesOwned).HasDefaultValue(0);
        builder.Property(s => s.TotalSteals).HasDefaultValue(0);
        builder.Property(s => s.TotalXp).HasDefaultValue(0);
        builder.Property(s => s.Level).HasDefaultValue((short)1).HasSentinel((short)1);
        builder.Property(s => s.CurrentStreakWeeks).HasDefaultValue((short)0);
        builder.Property(s => s.LongestStreakWeeks).HasDefaultValue((short)0);
        builder.HasOne<AppUser>().WithOne().HasForeignKey<UserStats>(s => s.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}
