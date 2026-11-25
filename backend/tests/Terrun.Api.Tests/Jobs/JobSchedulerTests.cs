using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Terrun.Api.Jobs;
using Terrun.Api.Tests.Identity;
using Terrun.Api.Tests.Persistence;

namespace Terrun.Api.Tests.Jobs;

// CTZ-104 (LLD §6, D-25): a job runs only when its cron is due after the last success, a failed or skipped run is
// recovered by the next trigger, and two triggers at once never run the same job twice
[Collection(MigratedDatabaseCollection.Name)]
public sealed class JobSchedulerTests(MigratedDatabaseFixture database)
{
    private static readonly DateTimeOffset _monday = new(2026, 11, 23, 14, 17, 0, TimeSpan.Zero);

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task RunDue_NeverRunJob_RunsItAndRecordsTheSuccess()
    {
        var job = new TestJob("0 * * * *");
        var clock = new MovableClock(_monday);

        var results = await RunAsync(job, clock);
        var run = await JobRunAsync(job.Name);

        Assert.Equal(JobOutcome.Succeeded, Assert.Single(results).Outcome);
        Assert.Equal(1, job.Runs);
        Assert.Equal(_monday, run.LastStartedAt);
        Assert.Equal(_monday, run.LastSucceededAt);
        Assert.Null(run.LastError);
    }

    [Fact]
    public async Task RunDue_BeforeTheNextOccurrence_DoesNotRunAgain()
    {
        var job = new TestJob("0 * * * *");
        var clock = new MovableClock(_monday);
        await RunAsync(job, clock);
        clock.Advance(TimeSpan.FromMinutes(30));

        var results = await RunAsync(job, clock);

        Assert.Equal(JobOutcome.NotDue, Assert.Single(results).Outcome);
        Assert.Equal(1, job.Runs);
    }

    // GitHub skipped two hourly triggers: the next one still runs the job once, not three times
    [Fact]
    public async Task RunDue_AfterSkippedTriggers_RunsOnceAndCatchesUp()
    {
        var job = new TestJob("0 * * * *");
        var clock = new MovableClock(_monday);
        await RunAsync(job, clock);
        clock.Advance(TimeSpan.FromHours(3));

        var first = await RunAsync(job, clock);
        var second = await RunAsync(job, clock);

        Assert.Equal(JobOutcome.Succeeded, Assert.Single(first).Outcome);
        Assert.Equal(JobOutcome.NotDue, Assert.Single(second).Outcome);
        Assert.Equal(2, job.Runs);
    }

    [Fact]
    public async Task RunDue_JobThatFails_RecordsTheErrorAndRunsAgainInTheNextTrigger()
    {
        var job = new TestJob("0 0 * * *") { FailuresLeft = 1 };
        var clock = new MovableClock(_monday);

        var failed = await RunAsync(job, clock);
        var afterFailure = await JobRunAsync(job.Name);
        clock.Advance(TimeSpan.FromMinutes(5));
        var retried = await RunAsync(job, clock);
        var afterRetry = await JobRunAsync(job.Name);

        Assert.Equal(JobOutcome.Failed, Assert.Single(failed).Outcome);
        Assert.Equal("boom", afterFailure.LastError);
        Assert.Null(afterFailure.LastSucceededAt);
        Assert.Equal(JobOutcome.Succeeded, Assert.Single(retried).Outcome);
        Assert.Null(afterRetry.LastError);
        Assert.Equal(clock.GetUtcNow(), afterRetry.LastSucceededAt);
    }

    // The daily jobs follow the day of the Dominican Republic (AST, UTC-4): midnight there is 04:00 UTC
    [Fact]
    public async Task RunDue_DailyJob_BecomesDueAtMidnightInSantoDomingo()
    {
        var job = new TestJob("0 0 * * *");
        var clock = new MovableClock(new DateTimeOffset(2026, 11, 24, 3, 30, 0, TimeSpan.Zero));
        await RunAsync(job, clock);

        clock.Advance(TimeSpan.FromMinutes(29));
        var beforeMidnight = await RunAsync(job, clock);
        clock.Advance(TimeSpan.FromMinutes(1));
        var atMidnight = await RunAsync(job, clock);

        Assert.Equal(JobOutcome.NotDue, Assert.Single(beforeMidnight).Outcome);
        Assert.Equal(JobOutcome.Succeeded, Assert.Single(atMidnight).Outcome);
    }

    [Fact]
    public async Task RunDue_JobStillRunningElsewhere_IsNotStartedAgain()
    {
        var job = new TestJob("0 * * * *");
        await MarkStartedAsync(job.Name, _monday.AddMinutes(-10));

        var results = await RunAsync(job, new MovableClock(_monday));

        Assert.Equal(JobOutcome.AlreadyRunning, Assert.Single(results).Outcome);
        Assert.Equal(0, job.Runs);
    }

    // The process that claimed the job died: after the 30-minute lease another trigger takes it over
    [Fact]
    public async Task RunDue_ClaimOlderThanTheLease_IsTakenOver()
    {
        var job = new TestJob("0 * * * *");
        await MarkStartedAsync(job.Name, _monday.AddMinutes(-31));

        var results = await RunAsync(job, new MovableClock(_monday));

        Assert.Equal(JobOutcome.Succeeded, Assert.Single(results).Outcome);
    }

    [Fact]
    public async Task RunDue_TwoTriggersAtTheSameTime_RunTheJobOnce()
    {
        var job = new TestJob("0 * * * *") { Delay = TimeSpan.FromMilliseconds(300) };
        var clock = new MovableClock(_monday);

        var results = await Task.WhenAll(Enumerable.Range(0, 4).Select(_ => RunAsync(job, clock)));

        Assert.Equal(1, job.Runs);
        Assert.Equal(1, results.Count(r => r.Single().Outcome == JobOutcome.Succeeded));
    }

    private async Task<IReadOnlyList<JobResult>> RunAsync(IScheduledJob job, TimeProvider clock)
    {
        await using var scope = database.CreateScope();
        var scheduler = new JobScheduler(
            MigratedDatabaseFixture.Db(scope), [job], Options.Create(new JobsOptions()), clock, NullLogger<JobScheduler>.Instance);
        return await scheduler.RunDueAsync(Ct);
    }

    private async Task<Terrun.Domain.Platform.JobRun> JobRunAsync(string name)
    {
        await using var scope = database.CreateScope();
        return await MigratedDatabaseFixture.Db(scope).JobRuns.AsNoTracking().SingleAsync(j => j.Name == name, Ct);
    }

    private async Task MarkStartedAsync(string name, DateTimeOffset startedAt)
    {
        await using var scope = database.CreateScope();
        var db = MigratedDatabaseFixture.Db(scope);
        db.JobRuns.Add(new Terrun.Domain.Platform.JobRun { Name = name, LastStartedAt = startedAt });
        await db.SaveChangesAsync(Ct);
    }

    private sealed class TestJob(string cron) : IScheduledJob
    {
        private int _runs;

        public string Name { get; } = $"test_{Guid.NewGuid():N}";

        public string Cron => cron;

        public int FailuresLeft { get; set; }

        public TimeSpan Delay { get; init; }

        public int Runs => _runs;

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            Interlocked.Increment(ref _runs);
            await Task.Delay(Delay, cancellationToken);
            if (FailuresLeft-- > 0)
            {
                throw new InvalidOperationException("boom");
            }
        }
    }
}
