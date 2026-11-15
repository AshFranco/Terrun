namespace Terrun.Domain.Identity;

public sealed class VerificationToken
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public TokenType Type { get; set; }

    public required byte[] TokenHash { get; set; }

    public DateTimeOffset ExpiresAt { get; set; }

    public DateTimeOffset? UsedAt { get; set; }

    // Wrong codes entered; after the limit the code stops working and a new one must be requested
    public short AttemptCount { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
