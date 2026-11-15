using System.Buffers.Text;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Terrun.Domain.Errors;
using Terrun.Domain.Identity;
using Terrun.Infrastructure.Persistence;

namespace Terrun.Api.Modules.Identity.Services;

// LLD §7.1: 32 random bytes, only their SHA-256 is stored, 30 days, rotated on every use. Reusing a rotated token
// revokes its whole family, because it means someone else holds a copy
public sealed class RefreshTokenService(TerrunDbContext db, IOptions<JwtOptions> options, TimeProvider clock)
{
    private const int _tokenBytes = 32;

    public (string Token, DateTimeOffset ExpiresAt) Issue(Guid userId, Guid familyId, IPAddress? ip, string? device)
    {
        var (plain, entity) = NewToken(userId, familyId, ip, device);
        db.RefreshTokens.Add(entity);
        return (plain, entity.ExpiresAt);
    }

    public async Task<(RefreshToken Current, string Token, DateTimeOffset ExpiresAt)> RotateAsync(
        string presentedToken, IPAddress? ip, string? device, CancellationToken cancellationToken)
    {
        var hash = HashOf(presentedToken);
        var current = await db.RefreshTokens.SingleOrDefaultAsync(t => t.TokenHash == hash, cancellationToken)
            ?? throw InvalidToken();

        // A token replaced by a newer one is a reuse; one revoked without a successor was closed by a logout
        if (current.RevokedAt is not null)
        {
            if (current.ReplacedBy is not null)
            {
                await RevokeFamilyAfterReuseAsync(current, ip, cancellationToken);
            }

            throw InvalidToken();
        }

        var now = clock.GetUtcNow();
        if (current.ExpiresAt <= now)
        {
            throw InvalidToken();
        }

        var (plain, next) = NewToken(current.UserId, current.FamilyId, ip, device);

        // Conditional update: if two requests rotate the same token at once, only one wins and the other is a reuse
        var rotated = await db.RefreshTokens
            .Where(t => t.Id == current.Id && t.RevokedAt == null)
            .ExecuteUpdateAsync(set => set.SetProperty(t => t.RevokedAt, now).SetProperty(t => t.ReplacedBy, next.Id), cancellationToken);
        if (rotated == 0)
        {
            await RevokeFamilyAfterReuseAsync(current, ip, cancellationToken);
        }

        db.RefreshTokens.Add(next);
        return (current, plain, next.ExpiresAt);
    }

    public async Task RevokeAsync(string presentedToken, Guid userId, CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow();
        var hash = HashOf(presentedToken);
        await db.RefreshTokens
            .Where(t => t.TokenHash == hash && t.UserId == userId && t.RevokedAt == null)
            .ExecuteUpdateAsync(set => set.SetProperty(t => t.RevokedAt, now), cancellationToken);
    }

    public static byte[] HashOf(string token) => SHA256.HashData(Encoding.UTF8.GetBytes(token));

    private async Task RevokeFamilyAfterReuseAsync(RefreshToken reused, IPAddress? ip, CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow();
        await db.RefreshTokens
            .Where(t => t.FamilyId == reused.FamilyId && t.RevokedAt == null)
            .ExecuteUpdateAsync(set => set.SetProperty(t => t.RevokedAt, now), cancellationToken);
        db.Record(AuditTrail.RefreshTokenReused, reused.UserId, ip);
        await db.SaveChangesAsync(cancellationToken);
        throw InvalidToken();
    }

    private (string Plain, RefreshToken Entity) NewToken(Guid userId, Guid familyId, IPAddress? ip, string? device)
    {
        var plain = Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(_tokenBytes));
        var entity = new RefreshToken
        {
            Id = Guid.CreateVersion7(),
            UserId = userId,
            FamilyId = familyId,
            TokenHash = HashOf(plain),
            ExpiresAt = clock.GetUtcNow().AddDays(options.Value.RefreshTokenDays),
            Ip = ip,
            Device = device,
        };
        return (plain, entity);
    }

    private static UnauthorizedException InvalidToken() => new(ErrorCodes.InvalidToken, "The refresh token is not valid.");
}
