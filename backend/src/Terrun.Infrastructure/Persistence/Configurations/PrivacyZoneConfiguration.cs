using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Terrun.Domain.Identity;
using Terrun.Domain.Profiles;

namespace Terrun.Infrastructure.Persistence.Configurations;

internal sealed class PrivacyZoneConfiguration : IEntityTypeConfiguration<PrivacyZone>
{
    public void Configure(EntityTypeBuilder<PrivacyZone> builder)
    {
        // LLD §2.3: at most 5 zones per user is enforced by the service, not by the database
        builder.ToTable("privacy_zone", table => table.HasCheckConstraint("ck_privacy_zone_radius_m", "radius_m BETWEEN 100 AND 1000"));
        builder.Property(z => z.Id).HasDefaultValueSql(ColumnDefaults.NewUuid);
        builder.Property(z => z.Name).HasMaxLength(40);
        builder.Property(z => z.Center).HasColumnType("geography (point, 4326)");
        builder.Property(z => z.CreatedAt).HasDefaultValueSql(ColumnDefaults.Now);
        builder.HasOne<AppUser>().WithMany().HasForeignKey(z => z.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}
