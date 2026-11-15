using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Terrun.Domain.Errors;
using Terrun.Domain.Identity;
using Terrun.Infrastructure.Email;
using Terrun.Infrastructure.Persistence;

namespace Terrun.Api.Modules.Identity.Services;

// D-26: a 6-digit code typed in P-04 instead of a terrun:// link, which Gmail web does not make clickable. It lasts
// 24 h (LLD §2.3), dies after 5 wrong attempts and only one is active per account
public sealed partial class EmailVerificationService(
    TerrunDbContext db,
    IEmailSender emailSender,
    TimeProvider clock,
    ILogger<EmailVerificationService> logger)
{
    private const int _maxAttempts = 5;
    private const int _codeUpperBound = 1_000_000;
    private static readonly TimeSpan _validity = TimeSpan.FromHours(24);

    // Stops anyone from flooding an inbox through the public resend endpoint
    private static readonly TimeSpan _resendCooldown = TimeSpan.FromSeconds(60);

    public string CreateCode(Guid userId)
    {
        var code = RandomNumberGenerator.GetInt32(_codeUpperBound).ToString("D6", CultureInfo.InvariantCulture);
        db.VerificationTokens.Add(new VerificationToken
        {
            UserId = userId,
            Type = TokenType.EmailVerification,
            TokenHash = HashOf(userId, code),
            ExpiresAt = clock.GetUtcNow().Add(_validity),
        });
        return code;
    }

    // Sending happens after the commit and never fails the request: the runner can always ask for another code
    public async Task SendAsync(string email, string displayName, string code, CancellationToken cancellationToken)
    {
        try
        {
            await emailSender.SendAsync(VerificationEmail.Build(email, displayName, code), cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            LogSendFailed(logger, exception);
        }
    }

    public async Task VerifyAsync(string email, string code, IPAddress? ip, CancellationToken cancellationToken)
    {
        var user = await db.Users.SingleOrDefaultAsync(u => u.Email == email.Trim(), cancellationToken)
            ?? throw InvalidCode();
        if (user.IsEmailVerified)
        {
            return;
        }

        var now = clock.GetUtcNow();
        var token = await db.VerificationTokens
            .Where(t => t.UserId == user.Id && t.Type == TokenType.EmailVerification
                && t.UsedAt == null && t.ExpiresAt > now && t.AttemptCount < _maxAttempts)
            .OrderByDescending(t => t.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw InvalidCode();

        if (!CryptographicOperations.FixedTimeEquals(token.TokenHash, HashOf(user.Id, code)))
        {
            await db.VerificationTokens
                .Where(t => t.Id == token.Id)
                .ExecuteUpdateAsync(set => set.SetProperty(t => t.AttemptCount, t => (short)(t.AttemptCount + 1)), cancellationToken);
            throw InvalidCode();
        }

        token.UsedAt = now;
        user.IsEmailVerified = true;
        db.Record(AuditTrail.EmailVerified, user.Id, ip);
        await db.SaveChangesAsync(cancellationToken);
    }

    // Always answers the same way, so it does not reveal which emails have an account
    public async Task ResendAsync(string email, CancellationToken cancellationToken)
    {
        var user = await db.Users.SingleOrDefaultAsync(u => u.Email == email.Trim(), cancellationToken);
        if (user is null || user.IsEmailVerified || user.Status != UserStatus.Active)
        {
            return;
        }

        var now = clock.GetUtcNow();
        var active = db.VerificationTokens.Where(t =>
            t.UserId == user.Id && t.Type == TokenType.EmailVerification && t.UsedAt == null && t.ExpiresAt > now);
        if (await active.AnyAsync(t => t.CreatedAt > now - _resendCooldown, cancellationToken))
        {
            return;
        }

        await active.ExecuteUpdateAsync(set => set.SetProperty(t => t.ExpiresAt, now), cancellationToken);
        var code = CreateCode(user.Id);
        await db.SaveChangesAsync(cancellationToken);

        var displayName = await db.Profiles.Where(p => p.UserId == user.Id).Select(p => p.DisplayName).SingleAsync(cancellationToken);
        await SendAsync(user.Email, displayName, code, cancellationToken);
    }

    private static byte[] HashOf(Guid userId, string code) => SHA256.HashData(Encoding.UTF8.GetBytes($"{userId:N}:{code}"));

    private static InvalidRequestException InvalidCode() =>
        new(ErrorCodes.VerificationCodeInvalid, "The verification code is wrong, expired or used up.");

    [LoggerMessage(Level = LogLevel.Error, Message = "The verification email could not be sent")]
    private static partial void LogSendFailed(ILogger logger, Exception exception);
}
