using Cronos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Terrun.Domain.Platform;
using Terrun.Infrastructure.Persistence;

namespace Terrun.Api.Jobs;

public enum JobOutcome
{
    Succeeded,
    Failed,
    NotDue,
    AlreadyRunning,
}

public sealed record JobResult(string Name, JobOutcome Outcome);

// LLD §6 (D-25): runs every due job once. A job is claimed with one conditional UPDATE, so two triggers at the same time
// never run it twice; a claim older than the lease is taken over, because the process that held it died
public sealed class JobScheduler(
    TerrunDbContext db,
    IEnumerable<IScheduledJob> jobs,
    IOptions<JobsOptions> options,
    TimeProvider clock,
    ILogger<JobScheduler> logger)
{
    private const int _maxErrorLength = 2000;
    private static readonly TimeSpan _lease = TimeSpan.FromMinutes(30);

    public async Task<IReadOnlyList<JobResult>> RunDueAsync(CancellationToken cancellationToken)
    {
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(options.Value.TimeZone);
        var results = new List<JobResult>();
        foreach (var job in jobs)
        {
            results.Add(await RunIfDueAsync(job, timeZone, cancellationToken));
        }

        return results;
    }

    private async Task<JobResult> RunIfDueAsync(IScheduledJob job, TimeZoneInfo timeZone, CancellationToken cancellationToken)
    {
        var run = await GetOrCreateAsync(job.Name, cancellationToken);
        var now = clock.GetUtcNow();
        if (!IsDue(job, run, now, timeZone))
        {
            return new JobResult(job.Name, JobOutcome.NotDue);
        }

        if (!await TryClaimAsync(job.Name, now, cancellationToken))
        {
            return new JobResult(job.Name, JobOutcome.AlreadyRunning);
        }

        logger.LogInformation("Job {Job} started", job.Name);
        try
        {
            await job.RunAsync(cancellationToken);
            await FinishAsync(job.Name, error: null, cancellationToken);
            logger.LogInformation("Job {Job} succeeded", job.Name);
            return new JobResult(job.Name, JobOutcome.Succeeded);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogError(exception, "Job {Job} failed", job.Name);
            await FinishAsync(job.Name, exception.Message, CancellationToken.None);
            return new JobResult(job.Name, JobOutcome.Failed);
        }
    }

    // Due when the cron has an occurrence after the last success; a job that never succeeded is always due
    private static bool IsDue(IScheduledJob job, JobRun run, DateTimeOffset now, TimeZoneInfo timeZone)
    {
        if (run.LastSucceededAt is not { } lastSuccess)
        {
            return true;
        }

        var next = CronExpression.Parse(job.Cron).GetNextOccurrence(lastSuccess, timeZone);
        return next is not null && next <= now;
    }

    private async Task<JobRun> GetOrCreateAsync(string name, CancellationToken cancellationToken)
    {
        await db.Database.ExecuteSqlAsync($"INSERT INTO job_run (name) VALUES ({name}) ON CONFLICT (name) DO NOTHING", cancellationToken);
        return await db.JobRuns.AsNoTracking().SingleAsync(j => j.Name == name, cancellationToken);
    }

    private async Task<bool> TryClaimAsync(string name, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var staleBefore = now - _lease;
        var claimed = await db.JobRuns
            .Where(j => j.Name == name
                && (j.LastStartedAt == null
                    || (j.LastFinishedAt != null && j.LastFinishedAt >= j.LastStartedAt)
                    || j.LastStartedAt < staleBefore))
            .ExecuteUpdateAsync(set => set.SetProperty(j => j.LastStartedAt, now), cancellationToken);
        return claimed == 1;
    }

    private async Task FinishAsync(string name, string? error, CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow();
        var trimmed = error is { Length: > _maxErrorLength } ? error[.._maxErrorLength] : error;
        await db.JobRuns.Where(j => j.Name == name).ExecuteUpdateAsync(
            set => set
                .SetProperty(j => j.LastFinishedAt, now)
                .SetProperty(j => j.LastSucceededAt, j => error == null ? now : j.LastSucceededAt)
                .SetProperty(j => j.LastError, trimmed),
            cancellationToken);
    }
}
