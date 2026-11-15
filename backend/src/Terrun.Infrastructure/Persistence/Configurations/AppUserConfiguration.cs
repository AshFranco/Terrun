using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Terrun.Domain.Identity;

namespace Terrun.Infrastructure.Persistence.Configurations;

internal sealed class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        builder.ToTable("app_user");
        builder.Property(u => u.Id).HasDefaultValueSql(ColumnDefaults.NewUuid);
        builder.Property(u => u.Email).HasColumnType("citext");
        builder.HasIndex(u => u.Email).IsUnique().HasDatabaseName("uk_app_user_email");
        builder.Property(u => u.IsEmailVerified).HasDefaultValue(false);
        builder.Property(u => u.Role).HasDefaultValue(UserRole.Runner);
        builder.Property(u => u.Status).HasDefaultValue(UserStatus.Active);
        builder.Property(u => u.SecurityVersion).HasDefaultValue(1).HasSentinel(1);
        builder.Property(u => u.CreatedAt).HasDefaultValueSql(ColumnDefaults.Now);
        builder.Property(u => u.UpdatedAt).HasDefaultValueSql(ColumnDefaults.Now);
    }
}
