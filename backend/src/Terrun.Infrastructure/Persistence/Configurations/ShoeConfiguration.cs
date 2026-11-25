using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Terrun.Domain.Equipment;
using Terrun.Domain.Identity;

namespace Terrun.Infrastructure.Persistence.Configurations;

internal sealed class ShoeConfiguration : IEntityTypeConfiguration<Shoe>
{
    public void Configure(EntityTypeBuilder<Shoe> builder)
    {
        builder.ToTable("shoe");
        builder.Property(s => s.Id).HasDefaultValueSql(ColumnDefaults.NewUuid);
        builder.Property(s => s.Brand).HasMaxLength(40);
        builder.Property(s => s.Model).HasMaxLength(60);
        builder.Property(s => s.Nickname).HasMaxLength(40);
        builder.Property(s => s.RetirementKm).HasDefaultValue((short)700);
        builder.Property(s => s.CreatedAt).HasDefaultValueSql(ColumnDefaults.Now);

        // LLD §2.3: at most one default shoe per runner
        builder.HasIndex(s => s.UserId, "uk_shoe_user_id_default").IsUnique().HasFilter("is_default");
        builder.HasOne<AppUser>().WithMany().HasForeignKey(s => s.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}
