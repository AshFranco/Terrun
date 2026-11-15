using System.Net;

namespace Terrun.Domain.Moderation;

// LLD §2.3 (M12) and §7.3: insert-only record of security-relevant actions; a database trigger rejects changes
public sealed class AuditLog
{
    public long Id { get; set; }

    public Guid? UserId { get; set; }

    public required string Action { get; set; }

    public required string Entity { get; set; }

    public string? EntityId { get; set; }

    // Raw JSON stored in a jsonb column; never coordinates, tokens or passwords (LLD §7.3)
    public string? Data { get; set; }

    public IPAddress? Ip { get; set; }

    public DateTimeOffset OccurredAt { get; set; }
}
