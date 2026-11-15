using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Terrun.Domain.Identity;
using Terrun.Infrastructure.Persistence.Seed;

namespace Terrun.Infrastructure.Persistence.Configurations;

internal sealed class LegalDocumentConfiguration : IEntityTypeConfiguration<LegalDocument>
{
    public void Configure(EntityTypeBuilder<LegalDocument> builder)
    {
        builder.ToTable("legal_document");
        builder.Property(d => d.Id).HasDefaultValueSql(ColumnDefaults.NewUuid);
        builder.HasIndex(d => new { d.Type, d.Version }).IsUnique().HasDatabaseName("uk_legal_document_type_version");
        builder.HasData(LegalDocumentSeed.All);
    }
}
