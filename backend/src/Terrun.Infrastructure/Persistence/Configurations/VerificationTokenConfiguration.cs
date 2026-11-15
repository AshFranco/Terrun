using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Terrun.Domain.Identity;

namespace Terrun.Infrastructure.Persistence.Configurations;

internal sealed class VerificationTokenConfiguration : IEntityTypeConfiguration<VerificationToken>
{
    public void Configure(EntityTypeBuilder<VerificationToken> builder)
    {
        builder.ToTable("verification_token");
        builder.Property(t => t.Id).HasDefaultValueSql(ColumnDefaults.NewUuid);
        builder.HasIndex(t => t.TokenHash).IsUnique().HasDatabaseName("uk_verification_token_token_hash");
        builder.Property(t => t.CreatedAt).HasDefaultValueSql(ColumnDefaults.Now);
        builder.Property(t => t.AttemptCount).HasDefaultValue((short)0);
        builder.HasIndex(t => new { t.UserId, t.Type });
        builder.HasOne<AppUser>().WithMany().HasForeignKey(t => t.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}
