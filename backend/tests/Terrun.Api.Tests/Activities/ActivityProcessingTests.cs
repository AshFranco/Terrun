using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Terrun.Api.Modules.Activities.Processing;
using Terrun.Api.Tests.Identity;
using Terrun.Api.Tests.Persistence;
using Terrun.Domain.Activities;
using Terrun.Infrastructure.Persistence;

namespace Terrun.Api.Tests.Activities;

// CTZ-104 (LLD §4.1, §6, D-35): an upload is processed right away through the in-memory queue, the startup sweep picks up
// what was left pending, a failure counts an attempt (failed after 3) and nothing is taken while no stage is registered
[Collection(MigratedDatabaseCollection.Name)]
public sealed class ActivityProcessingTests(MigratedDatabaseFixture database)
{
    private static readonly TimeSpan _waitLimit = TimeSpan.FromSeconds(15);

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Upload_WithStagesRegistered_IsProcessedRightAway()
    {
        var stage = new TestStage();
        await using var app = AppWith(stage);
        var runner = await RunnerAsync(app);

        var id = await UploadAsync(app, runner, hoursAgo: 30);
        var activity = await WaitUntilAsync(app, id, a => a.Status != ActivityStatus.Pending);

        Assert.Equal(ActivityStatus.Processed, activity.Status);
        Assert.NotNull(activity.ProcessedAt);
        Assert.Contains(id, stage.Seen);
    }

    [Fact]
    public async Task Process_StageThatSetsTheVerdict_KeepsThatStatus()
    {
        var stage = new TestStage { Verdict = ActivityStatus.InReview };
        await using var app = AppWith(stage);
        var runner = await RunnerAsync(app);

        var id = await UploadAsync(app, runner, hoursAgo: 31);
        var activity = await WaitUntilAsync(app, id, a => a.Status != ActivityStatus.Pending);

        Assert.Equal(ActivityStatus.InReview, activity.Status);
    }

    // LLD §4.1: everything in one transaction; on failure attempts += 1, and failed after three attempts
    [Fact]
    public async Task Process_StageThatFails_RollsBackAndFailsTheActivityAfterThreeAttempts()
    {
        await using var uploader = new AuthTestApp(database);
        var runner = await RunnerAsync(uploader);
        var id = await UploadAsync(uploader, runner, hoursAgo: 32);
        var stage = new TestStage();
        stage.FailFor.Add(id);

        var attempts = new List<(ActivityStatus Status, short Attempts, string? Title)>();
        for (var i = 0; i < ActivityProcessor.MaxAttempts; i++)
        {
            await ProcessWithAsync(uploader, stage, id);
            var activity = await LoadAsync(uploader, id);
            attempts.Add((activity.Status, activity.Attempts, activity.Title));
        }

        Assert.Equal((ActivityStatus.Pending, (short)1, (string?)null), attempts[0]);
        Assert.Equal((ActivityStatus.Pending, (short)2, (string?)null), attempts[1]);
        Assert.Equal((ActivityStatus.Failed, (short)3, (string?)null), attempts[2]);
        Assert.Equal(ActivityProcessor.FailureReason, (await LoadAsync(uploader, id)).StatusReason);
    }

    // The upload was received but the API restarted before processing it: the startup sweep takes it
    [Fact]
    public async Task Startup_PendingActivityLeftBehind_IsProcessedBySweep()
    {
        await using var uploader = new AuthTestApp(database);
        var runner = await RunnerAsync(uploader);
        var id = await UploadAsync(uploader, runner, hoursAgo: 33);

        await using var app = AppWith(new TestStage());
        var activity = await WaitUntilAsync(app, id, a => a.Status != ActivityStatus.Pending);

        Assert.Equal(ActivityStatus.Processed, activity.Status);
    }

    [Fact]
    public async Task Process_SameActivityTwiceAtTheSameTime_ProcessesItOnce()
    {
        await using var uploader = new AuthTestApp(database);
        var runner = await RunnerAsync(uploader);
        var id = await UploadAsync(uploader, runner, hoursAgo: 34);
        var stage = new TestStage { Delay = TimeSpan.FromMilliseconds(300) };

        var results = await Task.WhenAll(Enumerable.Range(0, 3).Select(_ => ProcessWithAsync(uploader, stage, id)));

        Assert.Equal(1, results.Count(processed => processed));
        Assert.Equal(1, stage.Seen.Count(seen => seen == id));
        Assert.Equal(ActivityStatus.Processed, (await LoadAsync(uploader, id)).Status);
    }

    // D-35: until CTZ-31a registers the first stage, uploads stay pending and no attempt is counted
    [Fact]
    public async Task Upload_WithoutStages_StaysPendingWithoutAttempts()
    {
        await using var app = new AuthTestApp(database);
        var runner = await RunnerAsync(app);

        var id = await UploadAsync(app, runner, hoursAgo: 35);
        await Task.Delay(TimeSpan.FromMilliseconds(500), Ct);
        var activity = await LoadAsync(app, id);
        var processed = await ProcessAsync(app, id);

        Assert.Equal(ActivityStatus.Pending, activity.Status);
        Assert.Equal(0, activity.Attempts);
        Assert.False(processed);
    }

    private AuthTestApp AppWith(TestStage stage) =>
        new(database, services => services.AddSingleton<IActivityProcessingStep>(stage));

    private static async Task<string> RunnerAsync(AuthTestApp app) =>
        (await app.RegisterOkAsync(AuthTestApp.NewEmail())).GetProperty("accessToken").GetString()!;

    private static async Task<Guid> UploadAsync(AuthTestApp app, string token, int hoursAgo)
    {
        var startedAt = app.Clock.GetUtcNow().AddHours(-hoursAgo);
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/activities")
        {
            Content = JsonContent.Create(ActivityUpload.At(new DateTimeOffset(startedAt.UtcDateTime, TimeSpan.Zero), points: 20).Body()),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await app.Client.SendAsync(request, Ct);
        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<JsonElement>(Ct)).GetProperty("id").GetGuid();
    }

    private static async Task<bool> ProcessAsync(AuthTestApp app, Guid id)
    {
        await using var scope = app.Services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<ActivityProcessor>().ProcessAsync(id, Ct);
    }

    // A processor with the given stage on an app without the queue worker stages, so no startup sweep runs beside the test
    private static async Task<bool> ProcessWithAsync(AuthTestApp app, TestStage stage, Guid id)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var processor = new ActivityProcessor(
            scope.ServiceProvider.GetRequiredService<TerrunDbContext>(), [stage], app.Clock, NullLogger<ActivityProcessor>.Instance);
        return await processor.ProcessAsync(id, Ct);
    }

    private static Task<Activity> LoadAsync(AuthTestApp app, Guid id) =>
        app.QueryAsync(db => db.Activities.AsNoTracking().SingleAsync(a => a.Id == id, Ct));

    private static async Task<Activity> WaitUntilAsync(AuthTestApp app, Guid id, Func<Activity, bool> condition)
    {
        var deadline = DateTimeOffset.UtcNow + _waitLimit;
        while (true)
        {
            var activity = await LoadAsync(app, id);
            if (condition(activity) || DateTimeOffset.UtcNow > deadline)
            {
                return activity;
            }

            await Task.Delay(TimeSpan.FromMilliseconds(100), Ct);
        }
    }

    // Writes a title so a rollback is visible; fails only for the activities in FailFor, so the startup sweep of other
    // tests' pending activities never counts attempts against them
    private sealed class TestStage : IActivityProcessingStep
    {
        public int Order => 0;

        public ActivityStatus? Verdict { get; init; }

        public TimeSpan Delay { get; init; }

        public ConcurrentBag<Guid> FailFor { get; } = [];

        public ConcurrentBag<Guid> Seen { get; } = [];

        public async Task RunAsync(ActivityProcessingContext context, CancellationToken cancellationToken)
        {
            Seen.Add(context.Activity.Id);
            context.Activity.Title = "Processed by the test stage";
            await context.Db.SaveChangesAsync(cancellationToken);
            await Task.Delay(Delay, cancellationToken);
            if (FailFor.Contains(context.Activity.Id))
            {
                throw new InvalidOperationException("stage failed");
            }

            if (Verdict is { } verdict)
            {
                context.Activity.Status = verdict;
            }
        }
    }
}
