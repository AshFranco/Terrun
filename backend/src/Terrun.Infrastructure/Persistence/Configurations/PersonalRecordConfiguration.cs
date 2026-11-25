using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Terrun.Domain.Activities;
using Terrun.Domain.Identity;

namespace Terrun.Infrastructure.Persistence.Configurations;

internal sealed class PersonalRecordConfiguration : IEntityTypeConfiguration<PersonalRecord>
{
    public void Configure(EntityTypeBuilder<PersonalRecord> builder)
    {
        builder.ToTable("personal_record");
        builder.Property(r => r.Id).HasDefaultValueSql(ColumnDefaults.NewUuid);
        builder.Property(r => r.Value).HasPrecision(10, 2);
        builder.HasIndex(r => new { r.UserId, r.Type }).IsUnique().HasDatabaseName("uk_personal_record_user_id_type");
        builder.HasOne<AppUser>().WithMany().HasForeignKey(r => r.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Activity>().WithMany().HasForeignKey(r => r.ActivityId).OnDelete(DeleteBehavior.Cascade);
    }
}
