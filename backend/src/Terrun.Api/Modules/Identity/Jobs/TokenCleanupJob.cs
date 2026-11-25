using Microsoft.EntityFrameworkCore;
using Terrun.Api.Jobs;
using Terrun.Infrastructure.Persistence;

namespace Terrun.Api.Modules.Identity.Jobs;

// LLD §6: once a day, delete the refresh tokens and verification codes that can no longer be used. A revoked refresh
// token that has not expired stays, because it is what detects the reuse of a rotated token (§7.1)
public sealed class TokenCleanupJob(TerrunDbContext db, TimeProvider clock) : IScheduledJob
{
    public string Name => "token_cleanup";

    // First run of each day
    public string Cron => "0 0 * * *";

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow();
        await db.RefreshTokens.Where(t => t.ExpiresAt < now).ExecuteDeleteAsync(cancellationToken);
        await db.VerificationTokens.Where(t => t.ExpiresAt < now).ExecuteDeleteAsync(cancellationToken);
    }
}
