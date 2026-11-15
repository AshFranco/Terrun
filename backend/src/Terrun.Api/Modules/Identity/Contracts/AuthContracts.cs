using Terrun.Domain.Identity;

namespace Terrun.Api.Modules.Identity.Contracts;

// LLD §3.2 (M1) and §3.3. Device and app version are optional: they help the runner recognize sessions and record how
// the legal documents were accepted

public sealed record RegisterRequest(
    string Email,
    string Password,
    string DisplayName,
    string Handle,
    IReadOnlyList<Guid> AcceptedLegalDocumentIds,
    string? AppVersion = null,
    string? Device = null);

public sealed record LoginRequest(string Email, string Password, string? Device = null);

public sealed record RefreshRequest(string RefreshToken, string? Device = null);

public sealed record LogoutRequest(string RefreshToken);

public sealed record VerifyEmailRequest(string Email, string Code);

public sealed record ResendVerificationRequest(string Email);

public sealed record AuthResponse(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAt,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAt,
    AuthUserResponse User);

public sealed record AuthUserResponse(Guid Id, string Email, string DisplayName, string Handle, bool IsEmailVerified, UserRole Role);
