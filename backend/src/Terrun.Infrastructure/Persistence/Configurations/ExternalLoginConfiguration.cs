using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Terrun.Domain.Identity;

namespace Terrun.Infrastructure.Persistence.Configurations;

internal sealed class ExternalLoginConfiguration : IEntityTypeConfiguration<ExternalLogin>
{
    public void Configure(EntityTypeBuilder<ExternalLogin> builder)
    {
        builder.ToTable("external_login");
        builder.Property(e => e.Id).HasDefaultValueSql(ColumnDefaults.NewUuid);
        builder.Property(e => e.Email).HasColumnType("citext");
        builder.HasIndex(e => new { e.Provider, e.Subject }).IsUnique().HasDatabaseName("uk_external_login_provider_subject");
        builder.Property(e => e.CreatedAt).HasDefaultValueSql(ColumnDefaults.Now);
        builder.HasOne<AppUser>().WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}
