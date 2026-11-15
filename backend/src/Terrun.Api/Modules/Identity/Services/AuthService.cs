using System.Net;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Terrun.Api.Modules.Identity.Contracts;
using Terrun.Api.Modules.Identity.Validators;
using Terrun.Domain.Errors;
using Terrun.Domain.Identity;
using Terrun.Domain.Profiles;
using Terrun.Infrastructure.Persistence;
using Terrun.Infrastructure.Security;

namespace Terrun.Api.Modules.Identity.Services;

public sealed class AuthService(
    TerrunDbContext db,
    IPasswordHasher passwordHasher,
    AccessTokenIssuer accessTokens,
    RefreshTokenService refreshTokens,
    EmailVerificationService emailVerification,
    LegalConsentService legalConsents,
    TimeProvider clock)
{
    private const string _emailUniqueConstraint = "uk_app_user_email";
    private const string _handleUniqueConstraint = "uk_profile_handle";

    // RF-01 and RF-12: one transaction creates the account, its profile, preferences and stats, the consent evidence
    // and the verification code; the runner is signed in right away and verifies from P-04
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, IPAddress? ip, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim();
        var handle = AuthRules.NormalizeHandle(request.Handle);
        await EnsureAvailableAsync(email, handle, cancellationToken);
        var legalDocumentIds = await legalConsents.RequireCurrentVersionsAsync(request.AcceptedLegalDocumentIds, cancellationToken);

        var user = new AppUser { Id = Guid.CreateVersion7(), Email = email, PasswordHash = passwordHasher.Hash(request.Password) };
        var profile = new Profile { UserId = user.Id, DisplayName = request.DisplayName.Trim(), Handle = handle };
        db.Users.Add(user);
        db.Profiles.Add(profile);
        db.UserPreferences.Add(new UserPreference { UserId = user.Id });
        db.UserStats.Add(new UserStats { UserId = user.Id });
        legalConsents.Record(user.Id, legalDocumentIds, ConsentMethod.Registration, request.AppVersion, ip);
        var code = emailVerification.CreateCode(user.Id);
        var response = IssueSession(user, profile, ip, request.Device);
        db.Record(AuditTrail.UserRegistered, user.Id, ip);

        await SaveRegistrationAsync(cancellationToken);
        await emailVerification.SendAsync(user.Email, profile.DisplayName, code, cancellationToken);
        return response;
    }

    // LLD §7.1 and §7.3: generic error for a wrong email or password, and every attempt goes to the audit log
    public async Task<AuthResponse> LoginAsync(LoginRequest request, IPAddress? ip, CancellationToken cancellationToken)
    {
        var user = await db.Users.SingleOrDefaultAsync(u => u.Email == request.Email.Trim(), cancellationToken);
        var passwordMatches = false;
        if (user?.PasswordHash is { } hash)
        {
            passwordMatches = passwordHasher.Verify(hash, request.Password);
        }
        else
        {
            passwordHasher.VerifyAgainstDecoy(request.Password);
        }

        if (user is null || !passwordMatches || user.Status == UserStatus.Deleted)
        {
            db.Record(AuditTrail.LoginFailed, user?.Id, ip);
            await db.SaveChangesAsync(cancellationToken);
            throw new UnauthorizedException(ErrorCodes.InvalidCredentials, "Wrong email or password.");
        }

        if (user.Status == UserStatus.Suspended)
        {
            db.Record(AuditTrail.LoginFailed, user.Id, ip);
            await db.SaveChangesAsync(cancellationToken);
            throw new ForbiddenException(ErrorCodes.UserSuspended, "The account is suspended.");
        }

        user.LastLoginAt = clock.GetUtcNow();
        var profile = await db.Profiles.SingleAsync(p => p.UserId == user.Id, cancellationToken);
        var response = IssueSession(user, profile, ip, request.Device);
        db.Record(AuditTrail.LoginSucceeded, user.Id, ip);
        await db.SaveChangesAsync(cancellationToken);
        return response;
    }

    public async Task<AuthResponse> RefreshAsync(RefreshRequest request, IPAddress? ip, CancellationToken cancellationToken)
    {
        var (current, refreshToken, refreshExpiresAt) = await refreshTokens.RotateAsync(request.RefreshToken, ip, request.Device, cancellationToken);
        var user = await db.Users.SingleAsync(u => u.Id == current.UserId, cancellationToken);
        if (user.Status != UserStatus.Active)
        {
            throw user.Status == UserStatus.Suspended
                ? new ForbiddenException(ErrorCodes.UserSuspended, "The account is suspended.")
                : new UnauthorizedException(ErrorCodes.InvalidToken, "The refresh token is not valid.");
        }

        var profile = await db.Profiles.SingleAsync(p => p.UserId == user.Id, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        var (accessToken, accessExpiresAt) = accessTokens.Issue(user);
        return new AuthResponse(accessToken, accessExpiresAt, refreshToken, refreshExpiresAt, ToUserResponse(user, profile));
    }

    public async Task LogoutAsync(Guid userId, string refreshToken, IPAddress? ip, CancellationToken cancellationToken)
    {
        await refreshTokens.RevokeAsync(refreshToken, userId, cancellationToken);
        db.Record(AuditTrail.LoggedOut, userId, ip);
        await db.SaveChangesAsync(cancellationToken);
    }

    private AuthResponse IssueSession(AppUser user, Profile profile, IPAddress? ip, string? device)
    {
        var (refreshToken, refreshExpiresAt) = refreshTokens.Issue(user.Id, familyId: Guid.CreateVersion7(), ip, device);
        var (accessToken, accessExpiresAt) = accessTokens.Issue(user);
        return new AuthResponse(accessToken, accessExpiresAt, refreshToken, refreshExpiresAt, ToUserResponse(user, profile));
    }

    private async Task EnsureAvailableAsync(string email, string handle, CancellationToken cancellationToken)
    {
        if (await db.Users.AnyAsync(u => u.Email == email, cancellationToken))
        {
            throw EmailTaken();
        }

        if (await db.Profiles.AnyAsync(p => p.Handle == handle, cancellationToken))
        {
            throw HandleTaken();
        }
    }

    // Two registrations racing for the same email or handle: the unique index decides and the loser gets the same 409
    private async Task SaveRegistrationAsync(CancellationToken cancellationToken)
    {
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } unique)
        {
            throw unique.ConstraintName switch
            {
                _emailUniqueConstraint => EmailTaken(),
                _handleUniqueConstraint => HandleTaken(),
                _ => exception,
            };
        }
    }

    private static AuthUserResponse ToUserResponse(AppUser user, Profile profile) =>
        new(user.Id, user.Email, profile.DisplayName, profile.Handle, user.IsEmailVerified, user.Role);

    private static ConflictException EmailTaken() => new(ErrorCodes.EmailAlreadyRegistered, "The email is already registered.");

    private static ConflictException HandleTaken() => new(ErrorCodes.HandleTaken, "The handle is already taken.");
}
