using Terrun.Domain.Identity;

namespace Terrun.Api.Modules.Identity.Contracts;

// LLD §3.2 (M1), RF-12 and Law 172-13: current legal versions for P-01, P-02 and P-32, and consent acceptance from settings

public sealed record LegalDocumentResponse(
    Guid Id,
    LegalDocumentType Type,
    string Version,
    string Url,
    DateTimeOffset EffectiveFrom);

public sealed record AcceptConsentsRequest(
    IReadOnlyList<Guid>? LegalDocumentIds,
    string? AppVersion = null);

public sealed record ConsentResponse(
    Guid LegalDocumentId,
    LegalDocumentType Type,
    string Version,
    DateTimeOffset AcceptedAt,
    ConsentMethod Method,
    string? AppVersion);
