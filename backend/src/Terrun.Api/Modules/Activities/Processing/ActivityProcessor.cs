using Microsoft.EntityFrameworkCore;
using Terrun.Domain.Activities;
using Terrun.Infrastructure.Persistence;

namespace Terrun.Api.Modules.Activities.Processing;

// LLD §4.1 and §6: processes one pending activity in one transaction, locking it with SKIP LOCKED so the queue, the
// startup sweep and the hourly job never process it twice. A failure rolls everything back and counts an attempt; after
// three the activity is failed. With no stages registered (until CTZ-31a) nothing is taken and activities stay pending
public sealed class ActivityProcessor(
    TerrunDbContext db,
    IEnumerable<IActivityProcessingStep> steps,
    TimeProvider clock,
    ILogger<ActivityProcessor> logger)
{
    public const int MaxAttempts = 3;
    public const string FailureReason = "processing_failed";
    private const int _sweepBatch = 50;

    private readonly IActivityProcessingStep[] _steps = [.. steps.OrderBy(s => s.Order)];

    public bool IsEnabled => _steps.Length > 0;

    public async Task<bool> ProcessAsync(Guid activityId, CancellationToken cancellationToken)
    {
        if (!IsEnabled)
        {
            return false;
        }

        try
        {
            return await ProcessInTransactionAsync(activityId, cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogError(exception, "Processing of activity {ActivityId} failed", activityId);
            db.ChangeTracker.Clear();
            await RecordFailedAttemptAsync(activityId, CancellationToken.None);
            return false;
        }
    }

    // Every pending activity, oldest first (partial index on created_at); used at startup and by the hourly job
    public async Task<int> ProcessPendingAsync(CancellationToken cancellationToken)
    {
        if (!IsEnabled)
        {
            return 0;
        }

        var processed = 0;
        var seen = new HashSet<Guid>();
        while (true)
        {
            var batch = await db.Activities.AsNoTracking()
                .Where(a => a.Status == ActivityStatus.Pending && !seen.Contains(a.Id))
                .OrderBy(a => a.CreatedAt)
                .Select(a => a.Id)
                .Take(_sweepBatch)
                .ToListAsync(cancellationToken);
            if (batch.Count == 0)
            {
                return processed;
            }

            foreach (var id in batch)
            {
                seen.Add(id);
                processed += await ProcessAsync(id, cancellationToken) ? 1 : 0;
            }
        }
    }

    private async Task<bool> ProcessInTransactionAsync(Guid activityId, CancellationToken cancellationToken)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        var activity = await db.Activities
            .FromSql($"SELECT * FROM activity WHERE id = {activityId} AND status = 'pending' FOR UPDATE SKIP LOCKED")
            .SingleOrDefaultAsync(cancellationToken);
        if (activity is null)
        {
            return false;
        }

        var context = new ActivityProcessingContext(activity, db);
        foreach (var step in _steps)
        {
            await step.RunAsync(context, cancellationToken);
        }

        if (activity.Status is ActivityStatus.Pending or ActivityStatus.Processing)
        {
            activity.Status = ActivityStatus.Processed;
        }

        activity.ProcessedAt = clock.GetUtcNow();
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return true;
    }

    private Task RecordFailedAttemptAsync(Guid activityId, CancellationToken cancellationToken) =>
        db.Activities
            .Where(a => a.Id == activityId && a.Status == ActivityStatus.Pending)
            .ExecuteUpdateAsync(
                set => set
                    .SetProperty(a => a.Attempts, a => (short)(a.Attempts + 1))
                    .SetProperty(a => a.Status, a => a.Attempts + 1 >= MaxAttempts ? ActivityStatus.Failed : ActivityStatus.Pending)
                    .SetProperty(a => a.StatusReason, a => a.Attempts + 1 >= MaxAttempts ? FailureReason : a.StatusReason),
                cancellationToken);
}
