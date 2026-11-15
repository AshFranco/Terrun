namespace Terrun.Domain.Identity;

public sealed class LegalDocument
{
    public Guid Id { get; set; }

    public LegalDocumentType Type { get; set; }

    public required string Version { get; set; }

    public required string Url { get; set; }

    public DateTimeOffset EffectiveFrom { get; set; }
}
