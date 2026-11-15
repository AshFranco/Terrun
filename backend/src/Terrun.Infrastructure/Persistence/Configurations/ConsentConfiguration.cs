using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Terrun.Domain.Identity;

namespace Terrun.Infrastructure.Persistence.Configurations;

internal sealed class ConsentConfiguration : IEntityTypeConfiguration<Consent>
{
    public void Configure(EntityTypeBuilder<Consent> builder)
    {
        builder.ToTable("consent");
        builder.Property(c => c.Id).HasDefaultValueSql(ColumnDefaults.NewUuid);
        builder.HasIndex(c => new { c.UserId, c.LegalDocumentId }).IsUnique().HasDatabaseName("uk_consent_user_id_legal_document_id");
        builder.Property(c => c.AcceptedAt).HasDefaultValueSql(ColumnDefaults.Now);
        builder.HasOne<AppUser>().WithMany().HasForeignKey(c => c.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<LegalDocument>().WithMany().HasForeignKey(c => c.LegalDocumentId).OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_consent_legal_document_legal_document_id");
    }
}
