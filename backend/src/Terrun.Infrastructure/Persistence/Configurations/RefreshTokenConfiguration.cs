using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Terrun.Domain.Identity;

namespace Terrun.Infrastructure.Persistence.Configurations;

internal sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_token");
        builder.Property(t => t.Id).HasDefaultValueSql(ColumnDefaults.NewUuid);
        builder.HasIndex(t => t.TokenHash).IsUnique().HasDatabaseName("uk_refresh_token_token_hash");
        builder.HasIndex(t => new { t.UserId, t.RevokedAt });

        // LLD §7.1: reusing a rotated token revokes its whole family in one statement
        builder.HasIndex(t => t.FamilyId);
        builder.Property(t => t.CreatedAt).HasDefaultValueSql(ColumnDefaults.Now);
        builder.HasOne<AppUser>().WithMany().HasForeignKey(t => t.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}
