using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Terrun.Api.Modules.Activities.Processing;
using Terrun.Api.Tests.Identity;
using Terrun.Api.Tests.Persistence;
using Terrun.Domain.Activities;
using Terrun.Domain.Moderation;

namespace Terrun.Api.Tests.Activities;

// CTZ-31a (LLD §4.1, D-36): the cleaning stage marks the discarded points in gps_point and sends cheating activities to
// review with an activity_review, reading every limit from system_setting. Runs through the production pipeline
[Collection(MigratedDatabaseCollection.Name)]
public sealed class CleaningStepTests(MigratedDatabaseFixture database) : IAsyncLifetime
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

    [Fact]
    public async Task Process_NormalRun_IsProcessedWithoutDiscardsOrReview()
    {
        var id = await UploadAsync(hoursAgo: 40, Run(speedMps: 3, intervalS: 2, points: 60));

        await ProcessAsync(id);
        var activity = await LoadAsync(id);

        Assert.Equal(ActivityStatus.Processed, activity.Status);
        Assert.Equal(0, await _app.QueryAsync(db => db.GpsPoints.CountAsync(p => p.ActivityId == id && p.IsDiscarded, Ct)));
        Assert.False(await _app.QueryAsync(db => db.ActivityReviews.AnyAsync(r => r.ActivityId == id, Ct)));
    }

    [Fact]
    [Trait("Case", "MT-02")]
    public async Task Process_BadAccuracyAndJump_MarksThosePointsWithTheirReason()
    {
        var points = Run(speedMps: 3, intervalS: 2, points: 20);
        points[5] = points[5] with { AccuracyM = 45 };
        points[10] = points[10] with { MetersNorth = points[10].MetersNorth + 400 };

        var id = await UploadAsync(hoursAgo: 41, points);
        await ProcessAsync(id);
        var discarded = await _app.QueryAsync(db => db.GpsPoints.AsNoTracking()
            .Where(p => p.ActivityId == id && p.IsDiscarded)
            .OrderBy(p => p.Seq)
            .Select(p => new { p.Seq, p.DiscardReason })
            .ToListAsync(Ct));

        Assert.Equal([(5, DiscardReason.Accuracy), (10, DiscardReason.Jump)], discarded.Select(d => (d.Seq, d.DiscardReason!.Value)));
        Assert.Equal(ActivityStatus.Processed, (await LoadAsync(id)).Status);
    }

    // 2:26 min/km: every leg is valid (24.7 km/h) but the pace is under anticheat.min_pace_s_per_km (150 s/km)
    [Fact]
    [Trait("Case", "MT-04")]
    public async Task Process_PaceFasterThanTheMinimum_LeavesTheActivityInReview()
    {
        var id = await UploadAsync(hoursAgo: 42, Run(speedMps: 1000.0 / 146, intervalS: 5, points: 241));

        await ProcessAsync(id);
        var activity = await LoadAsync(id);
        var review = await _app.QueryAsync(db => db.ActivityReviews.AsNoTracking().SingleAsync(r => r.ActivityId == id, Ct));
        var rule = JsonDocument.Parse(review.ViolatedRules).RootElement.EnumerateArray().Single();

        Assert.Equal(ActivityStatus.InReview, activity.Status);
        Assert.Equal(CleaningStep.InReviewReason, activity.StatusReason);
        Assert.Equal(ReviewStatus.Pending, review.Status);
        Assert.Equal("min_pace", rule.GetProperty("rule").GetString());
        Assert.Equal(146, rule.GetProperty("value").GetDouble());
        Assert.Equal(150, rule.GetProperty("limit").GetDouble());
    }

    // A run with a car ride in the middle: 25 % of the distance in legs over 25 km/h
    [Fact]
    public async Task Process_CarRideInTheMiddle_LeavesTheActivityInReview()
    {
        var points = Enumerable.Range(0, 31).Select(i => new RawPoint(i * 10, i * 30)).ToList();
        points.AddRange(Enumerable.Range(1, 3).Select(i => new RawPoint(300 + (i * 5), 900 + (i * 100))));

        var id = await UploadAsync(hoursAgo: 43, points);
        await ProcessAsync(id);
        var review = await _app.QueryAsync(db => db.ActivityReviews.AsNoTracking().SingleAsync(r => r.ActivityId == id, Ct));

        Assert.Equal(ActivityStatus.InReview, (await LoadAsync(id)).Status);
        Assert.Contains("invalid_distance", review.ViolatedRules, StringComparison.Ordinal);
    }

    private sealed record RawPoint(double Seconds, double MetersNorth, double AccuracyM = 5);

    private static List<RawPoint> Run(double speedMps, double intervalS, int points) =>
        [.. Enumerable.Range(0, points).Select(i => new RawPoint(i * intervalS, i * speedMps * intervalS))];

    private async Task<Guid> UploadAsync(int hoursAgo, List<RawPoint> points)
    {
        var startedAt = new DateTimeOffset(_app.Clock.GetUtcNow().UtcDateTime, TimeSpan.Zero).AddHours(-hoursAgo);
        var body = new
        {
            clientId = Guid.NewGuid(),
            type = "run",
            startedAt,
            endedAt = startedAt.AddSeconds(points[^1].Seconds + 1),
            points = points.Select((p, i) => new
            {
                seq = i,
                segment = 0,
                recordedAt = startedAt.AddSeconds(p.Seconds),
                lat = 10.0 + (p.MetersNorth / _earthRadiusM * 180 / Math.PI),
                lon = -60.0,
                accuracyM = p.AccuracyM,
            }),
        };
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/activities") { Content = JsonContent.Create(body) };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        var response = await _app.Client.SendAsync(request, Ct);
        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<JsonElement>(Ct)).GetProperty("id").GetGuid();
    }

    // Since CTZ-31b the stages run in production: the upload is processed by the queue right away (D-36)
    private async Task ProcessAsync(Guid id)
    {
        var deadline = DateTimeOffset.UtcNow.AddSeconds(15);
        while (await _app.QueryAsync(db => db.Activities.AnyAsync(a => a.Id == id && a.Status == ActivityStatus.Pending, Ct)))
        {
            Assert.True(DateTimeOffset.UtcNow < deadline, "The activity was not processed in 15 s.");
            await Task.Delay(TimeSpan.FromMilliseconds(100), Ct);
        }
    }

    private Task<Activity> LoadAsync(Guid id) =>
        _app.QueryAsync(db => db.Activities.AsNoTracking().SingleAsync(a => a.Id == id, Ct));
}
