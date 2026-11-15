namespace Terrun.Domain.Identity;

public sealed class VerificationToken
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public TokenType Type { get; set; }

    public required byte[] TokenHash { get; set; }

    public DateTimeOffset ExpiresAt { get; set; }

    public DateTimeOffset? UsedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
