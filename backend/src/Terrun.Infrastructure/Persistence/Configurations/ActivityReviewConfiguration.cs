using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Terrun.Domain.Activities;
using Terrun.Domain.Identity;
using Terrun.Domain.Moderation;

namespace Terrun.Infrastructure.Persistence.Configurations;

internal sealed class ActivityReviewConfiguration : IEntityTypeConfiguration<ActivityReview>
{
    public void Configure(EntityTypeBuilder<ActivityReview> builder)
    {
        builder.ToTable("activity_review");
        builder.Property(r => r.Id).HasDefaultValueSql(ColumnDefaults.NewUuid);
        builder.Property(r => r.ViolatedRules).HasColumnType("jsonb");
        builder.Property(r => r.Status).HasDefaultValue(ReviewStatus.Pending);
        builder.Property(r => r.CreatedAt).HasDefaultValueSql(ColumnDefaults.Now);
        builder.HasIndex(r => r.ActivityId).IsUnique().HasDatabaseName("uk_activity_review_activity_id");
        builder.HasOne<Activity>().WithMany().HasForeignKey(r => r.ActivityId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<AppUser>().WithMany().HasForeignKey(r => r.ReviewerId).OnDelete(DeleteBehavior.SetNull);
    }
}
