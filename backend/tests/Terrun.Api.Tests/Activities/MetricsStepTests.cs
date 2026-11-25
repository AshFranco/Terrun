using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Terrun.Api.Modules.Activities.Processing;
using Terrun.Api.Tests.Identity;
using Terrun.Api.Tests.Persistence;
using Terrun.Domain.Activities;

namespace Terrun.Api.Tests.Activities;

// CTZ-31b (LLD §4.1, RNF-04, D-36, D-37): the production pipeline stores the official metrics, the splits and the route
[Collection(MigratedDatabaseCollection.Name)]
public sealed class MetricsStepTests(MigratedDatabaseFixture database) : IAsyncLifetime
{
    private const double _earthRadiusM = 6_371_008.8;

    private AuthTestApp _app = null!;
    private string _token = null!;

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        _app = new AuthTestApp(database);
        _token = (await _app.RegisterOkAsync(AuthTestApp.NewEmail())).GetProperty("accessToken").GetString()!;
    }

    public ValueTask DisposeAsync() => _app.DisposeAsync();

    // 5,400 m at 3 m/s (10.8 km/h), a point every 10 s, default weight 70 kg: see MetricsCalculatorTests for the numbers
    [Fact]
    [Trait("Case", "MT-15")]
    public async Task Upload_RunOf5Point4Km_StoresTheOfficialMetricsSplitsAndRoute()
    {
        var id = await UploadAsync(hoursAgo: 50, Run(speedMps: 3, intervalS: 10, points: 181));

        var activity = await WaitProcessedAsync(id);
        var splits = await _app.QueryAsync(db => db.Splits.AsNoTracking().Where(s => s.ActivityId == id).OrderBy(s => s.Number).ToListAsync(Ct));

        Assert.Equal(ActivityStatus.Processed, activity.Status);
        Assert.Null(activity.StatusReason);
        Assert.Equal(5400m, activity.DistanceM);
        Assert.Equal(1800, activity.MovingTimeS);
        Assert.Equal(1801, activity.ElapsedTimeS);
        Assert.Equal(333, activity.AvgPaceSPerKm);
        Assert.Equal(3m, activity.MaxSpeedMps);
        Assert.Equal(372, activity.Calories);
        Assert.Equal(2, activity.Route!.NumPoints);
        Assert.Equal(4326, activity.Route.SRID);
        Assert.Equal(6, splits.Count);
        Assert.All(splits.Take(5), s => Assert.Equal((1000m, 333), (s.DistanceM, s.DurationS)));
        Assert.Equal((400m, 133, 333), (splits[5].DistanceM, splits[5].DurationS, splits[5].PaceSPerKm));
    }

    // Shorter than anticheat.min_duration_s (180 s): processed, but it will add no territory
    [Fact]
    [Trait("Case", "MT-05")]
    public async Task Upload_ActivityOf2Minutes_IsProcessedWithoutTerritory()
    {
        var id = await UploadAsync(hoursAgo: 51, Run(speedMps: 3, intervalS: 10, points: 12));

        var activity = await WaitProcessedAsync(id);

        Assert.Equal(ActivityStatus.Processed, activity.Status);
        Assert.Equal(MetricsStep.TooShortReason, activity.StatusReason);
        Assert.Equal(330m, activity.DistanceM);
    }

    // 10.625 MET × 100 kg × 0.5 h = 531.25 kcal
    [Fact]
    public async Task Upload_RunnerWeighing100Kg_UsesTheWeightOfTheProfile()
    {
        var patch = new HttpRequestMessage(HttpMethod.Patch, "/api/v1/me/profile") { Content = JsonContent.Create(new { weightKg = 100 }) };
        patch.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        (await _app.Client.SendAsync(patch, Ct)).EnsureSuccessStatusCode();

        var id = await UploadAsync(hoursAgo: 52, Run(speedMps: 3, intervalS: 10, points: 181));

        Assert.Equal(531, (await WaitProcessedAsync(id)).Calories);
    }

    // 1.5 m/s = 5.4 km/h walking: MET = 3.5 + 1.5 × 0.4 / 1.4 = 3.928571; × 70 kg × 1 h = 275 kcal
    [Fact]
    public async Task Upload_Walk_UsesTheWalkingMetTable()
    {
        var id = await UploadAsync(hoursAgo: 53, Run(speedMps: 1.5, intervalS: 10, points: 361), type: "walk");

        Assert.Equal(275, (await WaitProcessedAsync(id)).Calories);
    }

    // The anti-cheat sends it to review, but the metrics are still stored for the moderators
    [Fact]
    public async Task Upload_ActivityInReview_StillGetsItsMetrics()
    {
        var id = await UploadAsync(hoursAgo: 54, Run(speedMps: 1000.0 / 146, intervalS: 5, points: 241));

        var activity = await WaitProcessedAsync(id);

        Assert.Equal(ActivityStatus.InReview, activity.Status);
        Assert.Equal(CleaningStep.InReviewReason, activity.StatusReason);
        Assert.True(activity.DistanceM > 8000);
        Assert.True(await _app.QueryAsync(db => db.Splits.AnyAsync(s => s.ActivityId == id, Ct)));
    }

    // D-37: GET /me shows the totals of the processed activities; one in review adds nothing
    [Fact]
    public async Task Upload_TwoProcessedAndOneInReview_AddOnlyTheProcessedToTheTotalsOfMe()
    {
        var first = await UploadAsync(hoursAgo: 60, Run(speedMps: 3, intervalS: 10, points: 181));
        var second = await UploadAsync(hoursAgo: 58, Run(speedMps: 3, intervalS: 10, points: 101));
        var inReview = await UploadAsync(hoursAgo: 56, Run(speedMps: 1000.0 / 146, intervalS: 5, points: 241));
        await WaitProcessedAsync(first);
        var last = await WaitProcessedAsync(second);
        await WaitProcessedAsync(inReview);

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        var stats = (await (await _app.Client.SendAsync(request, Ct)).Content.ReadFromJsonAsync<JsonElement>(Ct)).GetProperty("stats");

        Assert.Equal(5400 + 3000, stats.GetProperty("totalDistanceM").GetInt64());
        Assert.Equal(1800 + 1000, stats.GetProperty("totalTimeS").GetInt64());
        Assert.Equal(2, stats.GetProperty("totalActivities").GetInt32());
        Assert.Equal(last.EndedAt, stats.GetProperty("lastActivityAt").GetDateTimeOffset());
    }

    // RNF-04: one hour recorded at one point per second is processed in less than 10 s
    [Fact]
    public async Task Process_OneHourAtOnePointPerSecond_TakesLessThanTenSeconds()
    {
        await using var app = new AuthTestApp(database, ActivityTestServices.RemoveQueueWorker);
        var token = (await app.RegisterOkAsync(AuthTestApp.NewEmail())).GetProperty("accessToken").GetString()!;
        var id = await UploadAsync(app, token, hoursAgo: 55, Run(speedMps: 3, intervalS: 1, points: 3601), type: "run");

        await using var scope = app.Services.CreateAsyncScope();
        var clock = System.Diagnostics.Stopwatch.StartNew();
        var processed = await scope.ServiceProvider.GetRequiredService<ActivityProcessor>().ProcessAsync(id, Ct);
        clock.Stop();

        Assert.True(processed);
        Assert.True(clock.Elapsed < TimeSpan.FromSeconds(10), $"Took {clock.Elapsed}");
        Assert.Equal(10800m, (await app.QueryAsync(db => db.Activities.AsNoTracking().SingleAsync(a => a.Id == id, Ct))).DistanceM);
    }

    private sealed record RawPoint(double Seconds, double MetersNorth);

    private static List<RawPoint> Run(double speedMps, double intervalS, int points) =>
        [.. Enumerable.Range(0, points).Select(i => new RawPoint(i * intervalS, i * speedMps * intervalS))];

    private Task<Guid> UploadAsync(int hoursAgo, List<RawPoint> points, string type = "run") =>
        UploadAsync(_app, _token, hoursAgo, points, type);

    private static async Task<Guid> UploadAsync(AuthTestApp app, string token, int hoursAgo, List<RawPoint> points, string type)
    {
        var startedAt = new DateTimeOffset(app.Clock.GetUtcNow().UtcDateTime, TimeSpan.Zero).AddHours(-hoursAgo);
        var body = new
        {
            clientId = Guid.NewGuid(),
            type,
            startedAt,
            endedAt = startedAt.AddSeconds(points[^1].Seconds + 1),
            points = points.Select((p, i) => new
            {
                seq = i,
                segment = 0,
                recordedAt = startedAt.AddSeconds(p.Seconds),
                lat = 10.0 + (p.MetersNorth / _earthRadiusM * 180 / Math.PI),
                lon = -60.0,
                accuracyM = 5,
            }),
        };
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/activities") { Content = JsonContent.Create(body) };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await app.Client.SendAsync(request, Ct);
        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<JsonElement>(Ct)).GetProperty("id").GetGuid();
    }

    private async Task<Activity> WaitProcessedAsync(Guid id)
    {
        var deadline = DateTimeOffset.UtcNow.AddSeconds(15);
        while (true)
        {
            var activity = await _app.QueryAsync(db => db.Activities.AsNoTracking().SingleAsync(a => a.Id == id, Ct));
            if (activity.Status != ActivityStatus.Pending)
            {
                return activity;
            }

            Assert.True(DateTimeOffset.UtcNow < deadline, "The activity was not processed in 15 s.");
            await Task.Delay(TimeSpan.FromMilliseconds(100), Ct);
        }
    }
}
