using System.Net;
using Terrun.Domain.Moderation;

namespace Terrun.Infrastructure.Persistence;

// LLD §7.3: security-relevant actions. Saved with the same SaveChanges as the change they describe, so both commit
// together. Never put emails, tokens, passwords or coordinates in it
public static class AuditTrail
{
    public const string UserRegistered = "user_registered";
    public const string EmailVerified = "email_verified";
    public const string LoginSucceeded = "login_succeeded";
    public const string LoginFailed = "login_failed";
    public const string RefreshTokenReused = "refresh_token_reused";
    public const string LoggedOut = "logged_out";
    public const string HandleChanged = "handle_changed";

    private const string _userEntity = "app_user";

    public static void Record(this TerrunDbContext db, string action, Guid? userId, IPAddress? ip) =>
        db.AuditLogs.Add(new AuditLog
        {
            Action = action,
            Entity = _userEntity,
            EntityId = userId?.ToString(),
            UserId = userId,
            Ip = ip,
        });
}
