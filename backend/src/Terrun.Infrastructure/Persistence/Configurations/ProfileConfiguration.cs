using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Terrun.Domain.Identity;
using Terrun.Domain.Profiles;

namespace Terrun.Infrastructure.Persistence.Configurations;

internal sealed class ProfileConfiguration : IEntityTypeConfiguration<Profile>
{
    public void Configure(EntityTypeBuilder<Profile> builder)
    {
        builder.ToTable("profile", table => table.HasCheckConstraint("ck_profile_weight_kg", "weight_kg BETWEEN 25 AND 300"));
        builder.HasKey(p => p.UserId);
        builder.Property(p => p.DisplayName).HasMaxLength(60);
        builder.Property(p => p.Handle).HasColumnType("citext");
        builder.HasIndex(p => p.Handle).IsUnique().HasDatabaseName("uk_profile_handle");
        builder.Property(p => p.Bio).HasMaxLength(300);
        builder.Property(p => p.City).HasMaxLength(80);
        builder.Property(p => p.Country).HasColumnType("char(2)");
        builder.Property(p => p.WeightKg).HasPrecision(5, 2);
        builder.Property(p => p.IsPrivate).HasDefaultValue(false);
        builder.Property(p => p.UpdatedAt).HasDefaultValueSql(ColumnDefaults.Now);
        builder.HasOne<AppUser>().WithOne().HasForeignKey<Profile>(p => p.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}
