namespace Terrun.Domain.Identity;

public sealed class ExternalLogin
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public LoginProvider Provider { get; set; }

    // The provider "sub" claim
    public required string Subject { get; set; }

    public required string Email { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
