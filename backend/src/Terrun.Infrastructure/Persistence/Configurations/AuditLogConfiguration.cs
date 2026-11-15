using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Terrun.Domain.Identity;
using Terrun.Domain.Moderation;

namespace Terrun.Infrastructure.Persistence.Configurations;

internal sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_log");
        builder.Property(a => a.Id).UseIdentityAlwaysColumn();
        builder.Property(a => a.Action).HasMaxLength(60);
        builder.Property(a => a.Entity).HasMaxLength(40);
        builder.Property(a => a.Data).HasColumnType("jsonb");
        builder.Property(a => a.OccurredAt).HasDefaultValueSql(ColumnDefaults.Now);
        builder.HasIndex(a => new { a.UserId, a.OccurredAt });

        // Restrict: an audited user is anonymized, never deleted (LLD §4.5), and audit rows cannot change
        builder.HasOne<AppUser>().WithMany().HasForeignKey(a => a.UserId).OnDelete(DeleteBehavior.Restrict);
    }
}
