using Terrun.Domain.Identity;

namespace Terrun.Infrastructure.Persistence.Seed;

// Version 1.0 of the legal texts written in CTZ-27 (docs/producto/legal/). Fixed ids keep the migration stable
internal static class LegalDocumentSeed
{
    private const string _legalTextsBaseUrl = "https://github.com/AshFranco/Terrun/blob/main/docs/producto/legal/";

    private static readonly DateTimeOffset _effectiveFrom = new(2026, 11, 15, 0, 0, 0, TimeSpan.Zero);

    public static readonly LegalDocument[] All =
    [
        new()
        {
            Id = new Guid("6f1c2d0e-7a4b-4c5d-9e8f-0a1b2c3d4e01"),
            Type = LegalDocumentType.Terms,
            Version = "1.0",
            Url = _legalTextsBaseUrl + "terminos-v1.0.md",
            EffectiveFrom = _effectiveFrom,
        },
        new()
        {
            Id = new Guid("6f1c2d0e-7a4b-4c5d-9e8f-0a1b2c3d4e02"),
            Type = LegalDocumentType.Privacy,
            Version = "1.0",
            Url = _legalTextsBaseUrl + "privacidad-v1.0.md",
            EffectiveFrom = _effectiveFrom,
        },
    ];
}
