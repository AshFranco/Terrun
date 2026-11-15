using System.Net;

namespace Terrun.Domain.Identity;

public sealed class Consent
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid LegalDocumentId { get; set; }

    public DateTimeOffset AcceptedAt { get; set; }

    public IPAddress? Ip { get; set; }
}
