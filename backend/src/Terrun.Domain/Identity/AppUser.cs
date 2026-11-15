namespace Terrun.Domain.Identity;

public sealed class AppUser
{
    public Guid Id { get; set; }

    public required string Email { get; set; }

    public bool IsEmailVerified { get; set; }

    // LLD §2.3: null only when the account signs in through an external login
    public string? PasswordHash { get; set; }

    public UserRole Role { get; set; } = UserRole.Runner;

    public UserStatus Status { get; set; } = UserStatus.Active;

    // LLD §7.1: the "sv" claim of the access token; raising it on a password or role change invalidates older tokens
    public int SecurityVersion { get; set; } = 1;

    public DateTimeOffset? LastLoginAt { get; set; }

    public DateTimeOffset? DeletedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
