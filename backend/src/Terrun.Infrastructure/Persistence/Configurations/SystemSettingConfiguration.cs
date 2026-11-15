using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Terrun.Domain.Identity;
using Terrun.Domain.Platform;
using Terrun.Infrastructure.Persistence.Seed;

namespace Terrun.Infrastructure.Persistence.Configurations;

internal sealed class SystemSettingConfiguration : IEntityTypeConfiguration<SystemSetting>
{
    public void Configure(EntityTypeBuilder<SystemSetting> builder)
    {
        builder.ToTable("system_setting");
        builder.HasKey(s => s.Key);
        builder.Property(s => s.Key).HasMaxLength(80);
        builder.Property(s => s.Value).HasColumnType("jsonb");
        builder.Property(s => s.IsPublic).HasDefaultValue(false);
        builder.Property(s => s.UpdatedAt).HasDefaultValueSql(ColumnDefaults.Now);
        builder.HasOne<AppUser>().WithMany().HasForeignKey(s => s.UpdatedBy).OnDelete(DeleteBehavior.SetNull);
        builder.HasData(SystemSettingSeed.All);
    }
}
