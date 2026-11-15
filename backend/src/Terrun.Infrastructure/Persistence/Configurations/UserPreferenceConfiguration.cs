using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Terrun.Domain.Common;
using Terrun.Domain.Identity;
using Terrun.Domain.Profiles;

namespace Terrun.Infrastructure.Persistence.Configurations;

internal sealed class UserPreferenceConfiguration : IEntityTypeConfiguration<UserPreference>
{
    public void Configure(EntityTypeBuilder<UserPreference> builder)
    {
        builder.ToTable("user_preference");
        builder.HasKey(p => p.UserId);
        builder.Property(p => p.Units).HasDefaultValue(UnitSystem.Metric);
        builder.Property(p => p.VoiceCueIntervalKm).HasPrecision(4, 2);

        // Explicit sentinel = default, so an "off" chosen by the runner is always written (EF 10 infers it; older versions did not)
        builder.Property(p => p.DefaultVisibility).HasDefaultValue(Visibility.Followers).HasSentinel(Visibility.Followers);
        builder.Property(p => p.VoiceCueIntervalKm).HasDefaultValue(1m).HasSentinel(1m);
        HasDefaultOn(builder.Property(p => p.IsAutoPauseEnabled));
        HasDefaultOn(builder.Property(p => p.IsVoiceCuesEnabled));
        HasDefaultOn(builder.Property(p => p.NotifyZoneStolen));
        HasDefaultOn(builder.Property(p => p.NotifyKudos));
        HasDefaultOn(builder.Property(p => p.NotifyComments));
        HasDefaultOn(builder.Property(p => p.NotifyFollowers));
        HasDefaultOn(builder.Property(p => p.NotifyChallenges));

        builder.Property(p => p.UpdatedAt).HasDefaultValueSql(ColumnDefaults.Now);
        builder.HasOne<AppUser>().WithOne().HasForeignKey<UserPreference>(p => p.UserId).OnDelete(DeleteBehavior.Cascade);
    }

    private static void HasDefaultOn(PropertyBuilder<bool> property) => property.HasDefaultValue(true).HasSentinel(true);
}
