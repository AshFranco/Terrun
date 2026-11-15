using System.Net;

namespace Terrun.Domain.Identity;

public sealed class RefreshToken
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    // SHA-256 of the token: the plain value is never stored
    public required byte[] TokenHash { get; set; }

    public Guid FamilyId { get; set; }

    public DateTimeOffset ExpiresAt { get; set; }

    public DateTimeOffset? RevokedAt { get; set; }

    public Guid? ReplacedBy { get; set; }

    public IPAddress? Ip { get; set; }

    public string? Device { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
