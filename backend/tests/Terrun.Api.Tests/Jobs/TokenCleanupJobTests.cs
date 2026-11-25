using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Terrun.Api.Modules.Identity.Jobs;
using Terrun.Api.Tests.Identity;
using Terrun.Api.Tests.Persistence;
using Terrun.Domain.Identity;
using Terrun.Infrastructure.Persistence;

namespace Terrun.Api.Tests.Jobs;

// CTZ-104 (LLD §6): the daily cleanup removes only tokens that can no longer be used
[Collection(MigratedDatabaseCollection.Name)]
public sealed class TokenCleanupJobTests(MigratedDatabaseFixture database)
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Run_ExpiredAndValidTokens_DeletesOnlyTheExpiredOnes()
    {
        await using var app = new AuthTestApp(database);
        var session = await app.RegisterOkAsync(AuthTestApp.NewEmail());
        var userId = session.GetProperty("user").GetProperty("id").GetGuid();
        var now = app.Clock.GetUtcNow();
        var expired = Token(userId, now.AddMinutes(-1), revokedAt: null);
        var revokedButValid = Token(userId, now.AddDays(5), revokedAt: now.AddDays(-1));
        await app.QueryAsync(async db =>
        {
            db.RefreshTokens.AddRange(expired, revokedButValid);
            db.VerificationTokens.Add(new VerificationToken
            {
                Id = Guid.CreateVersion7(),
                UserId = userId,
                Type = TokenType.PasswordReset,
                TokenHash = Guid.NewGuid().ToByteArray(),
                ExpiresAt = now.AddHours(-2),
            });
            return await db.SaveChangesAsync(Ct);
        });

        await using (var scope = app.Services.CreateAsyncScope())
        {
            await new TokenCleanupJob(scope.ServiceProvider.GetRequiredService<TerrunDbContext>(), app.Clock).RunAsync(Ct);
        }

        var remaining = await app.QueryAsync(db => db.RefreshTokens.Where(t => t.UserId == userId).Select(t => t.Id).ToListAsync(Ct));
        Assert.DoesNotContain(expired.Id, remaining);
        Assert.Contains(revokedButValid.Id, remaining);
        Assert.Equal(2, remaining.Count);
        Assert.Equal(1, await app.QueryAsync(db => db.VerificationTokens.CountAsync(t => t.UserId == userId, Ct)));
    }

    private static RefreshToken Token(Guid userId, DateTimeOffset expiresAt, DateTimeOffset? revokedAt) => new()
    {
        Id = Guid.CreateVersion7(),
        UserId = userId,
        TokenHash = Guid.NewGuid().ToByteArray(),
        FamilyId = Guid.CreateVersion7(),
        ExpiresAt = expiresAt,
        RevokedAt = revokedAt,
    };
}
