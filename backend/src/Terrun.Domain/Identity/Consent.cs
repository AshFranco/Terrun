using System.Net;

namespace Terrun.Domain.Identity;

public sealed class Consent
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid LegalDocumentId { get; set; }

    public DateTimeOffset AcceptedAt { get; set; }

    public ConsentMethod Method { get; set; }

    // App version that showed the checkbox, e.g. "0.3.0"
    public string? AppVersion { get; set; }

    public IPAddress? Ip { get; set; }
}
