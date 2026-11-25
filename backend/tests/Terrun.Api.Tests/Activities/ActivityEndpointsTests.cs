using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Terrun.Api.Modules.Activities.Contracts;
using Terrun.Api.Modules.Activities.Services;
using Terrun.Api.Tests.Identity;
using Terrun.Api.Tests.Persistence;
using Terrun.Domain.Activities;
using Terrun.Domain.Equipment;

namespace Terrun.Api.Tests.Activities;

// CTZ-32 (RF-18, LLD §3.2, §4.5, §7.2, D-38): reading, editing and deleting activities, with the visibility rules inside
// the query (owner sees everything; others only processed public activities) and the 200 m trim for others (MT-21)
[Collection(MigratedDatabaseCollection.Name)]
public sealed class ActivityEndpointsTests(MigratedDatabaseFixture database) : IAsyncLifetime
{
    private const double _earthRadiusM = 6_371_008.8;

    private AuthTestApp _app = null!;

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public ValueTask InitializeAsync()
    {
        _app = new AuthTestApp(database);
        return ValueTask.CompletedTask;
    }

    public ValueTask DisposeAsync() => _app.DisposeAsync();

    [Fact]
    public async Task Get_OwnActivity_ReturnsTheOfficialMetricsAndTheOwnerOnlyFields()
    {
        var owner = await RunnerAsync();
        var id = await UploadProcessedAsync(owner, hoursAgo: 70);

        var activity = await GetJsonAsync(owner, $"/api/v1/activities/{id}");

        Assert.True(activity.GetProperty("isOwner").GetBoolean());
        Assert.Equal("processed", activity.GetProperty("status").GetString());
        Assert.Equal(5400m, activity.GetProperty("distanceM").GetDecimal());
        Assert.Equal(333, activity.GetProperty("avgPaceSPerKm").GetInt32());
        Assert.Equal("Test phone", activity.GetProperty("device").GetString());
        Assert.Equal(owner.Id, activity.GetProperty("owner").GetProperty("id").GetGuid());
    }

    [Fact]
    public async Task Get_PublicActivityOfAnotherRunner_HidesTheOwnerOnlyFields()
    {
        var owner = await RunnerAsync();
        var id = await UploadProcessedAsync(owner, hoursAgo: 71);
        await PatchAsync(owner, id, new { visibility = "public" });

        var activity = await GetJsonAsync(await RunnerAsync(), $"/api/v1/activities/{id}");

        Assert.False(activity.GetProperty("isOwner").GetBoolean());
        Assert.Equal(JsonValueKind.Null, activity.GetProperty("device").ValueKind);
        Assert.Equal(JsonValueKind.Null, activity.GetProperty("appVersion").ValueKind);
    }

    // D-38: "followers" is owner-only until the follow table exists; private always is
    [Theory]
    [InlineData("followers")]
    [InlineData("private")]
    public async Task Get_NotPublicActivityOfAnotherRunner_Returns404(string visibility)
    {
        var owner = await RunnerAsync();
        var id = await UploadProcessedAsync(owner, hoursAgo: 72);
        await PatchAsync(owner, id, new { visibility });
        var other = await RunnerAsync();

        foreach (var path in new[] { $"/api/v1/activities/{id}", $"/api/v1/activities/{id}/route", $"/api/v1/activities/{id}/splits" })
        {
            var response = await SendAsync(other, HttpMethod.Get, path);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal("NOT_FOUND", await AuthTestApp.CodeOfAsync(response));
        }
    }

    // Others only see processed activities: one in review stays with its owner
    [Fact]
    public async Task Get_PublicActivityInReviewOfAnotherRunner_Returns404()
    {
        var owner = await RunnerAsync();
        var id = await UploadProcessedAsync(owner, hoursAgo: 73, speedMps: 1000.0 / 146, intervalS: 5, points: 241);
        await PatchAsync(owner, id, new { visibility = "public" });

        var response = await SendAsync(await RunnerAsync(), HttpMethod.Get, $"/api/v1/activities/{id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Get_UnknownActivity_Returns404()
    {
        var response = await SendAsync(await RunnerAsync(), HttpMethod.Get, $"/api/v1/activities/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // MT-21: a 3 km route seen by another runner starts 200 m later and ends 200 m earlier; the owner sees all of it
    [Fact]
    [Trait("Case", "MT-21")]
    public async Task GetRoute_ThreeKilometers_IsCompleteForTheOwnerAndTrimmedForOthers()
    {
        var owner = await RunnerAsync();
        var id = await UploadProcessedAsync(owner, hoursAgo: 74, speedMps: 3, intervalS: 10, points: 101);
        await PatchAsync(owner, id, new { visibility = "public" });

        var own = await GetJsonAsync(owner, $"/api/v1/activities/{id}/route");
        var others = await GetJsonAsync(await RunnerAsync(), $"/api/v1/activities/{id}/route");

        Assert.Equal("Feature", own.GetProperty("type").GetString());
        Assert.False(own.GetProperty("properties").GetProperty("isTrimmed").GetBoolean());
        Assert.Equal(0, MetersNorth(First(own)), 0.01);
        Assert.Equal(3000, MetersNorth(Last(own)), 0.01);
        Assert.True(others.GetProperty("properties").GetProperty("isTrimmed").GetBoolean());
        Assert.Equal(200, others.GetProperty("properties").GetProperty("trimM").GetInt32());
        Assert.Equal(200, MetersNorth(First(others)), 0.01);
        Assert.Equal(2800, MetersNorth(Last(others)), 0.01);
    }

    [Fact]
    [Trait("Case", "MT-21")]
    public async Task GetRoute_ThreeHundredFiftyMetersSeenByOthers_HasNoGeometry()
    {
        var owner = await RunnerAsync();
        var id = await UploadProcessedAsync(owner, hoursAgo: 75, speedMps: 3, intervalS: 10, points: 61, metersPerLeg: 350.0 / 60);
        await PatchAsync(owner, id, new { visibility = "public" });

        var others = await GetJsonAsync(await RunnerAsync(), $"/api/v1/activities/{id}/route");
        var own = await GetJsonAsync(owner, $"/api/v1/activities/{id}/route");

        Assert.Equal(JsonValueKind.Null, others.GetProperty("geometry").ValueKind);
        Assert.Equal(JsonValueKind.Object, own.GetProperty("geometry").ValueKind);
    }

    [Fact]
    [Trait("Case", "MT-15")]
    public async Task GetSplits_RunOf5Point4Km_ReturnsSixSplitsInOrder()
    {
        var owner = await RunnerAsync();
        var id = await UploadProcessedAsync(owner, hoursAgo: 76);

        var splits = (await GetJsonAsync(owner, $"/api/v1/activities/{id}/splits")).EnumerateArray().ToList();

        Assert.Equal([1, 2, 3, 4, 5, 6], splits.Select(s => s.GetProperty("number").GetInt32()));
        Assert.Equal(400m, splits[5].GetProperty("distanceM").GetDecimal());
    }

    [Fact]
    public async Task ListMine_FiveActivitiesInPagesOfTwo_ReturnsAllNewestFirstAndThenNoCursor()
    {
        var owner = await RunnerAsync();
        var ids = new List<Guid>();
        for (var i = 0; i < 5; i++)
        {
            ids.Add(await UploadProcessedAsync(owner, hoursAgo: 90 - i, points: 20));
        }

        var seen = new List<Guid>();
        string? cursor = null;
        var pages = 0;
        do
        {
            var page = await GetJsonAsync(owner, $"/api/v1/me/activities?limit=2{(cursor is null ? string.Empty : $"&cursor={cursor}")}");
            seen.AddRange(page.GetProperty("items").EnumerateArray().Select(i => i.GetProperty("id").GetGuid()));
            cursor = page.GetProperty("nextCursor").GetString();
            pages++;
        }
        while (cursor is not null);

        Assert.Equal(3, pages);
        Assert.Equal(Enumerable.Reverse(ids), seen);
    }

    [Fact]
    public async Task ListMine_FilterByTypeAndDates_ReturnsOnlyTheMatchingActivities()
    {
        var owner = await RunnerAsync();
        var walk = await UploadProcessedAsync(owner, hoursAgo: 100, points: 20, type: "walk");
        await UploadProcessedAsync(owner, hoursAgo: 99, points: 20);
        var recentRun = await UploadProcessedAsync(owner, hoursAgo: 98, points: 20);
        var from = Uri.EscapeDataString(_app.Clock.GetUtcNow().AddHours(-98.5).ToString("O"));

        var walks = await GetJsonAsync(owner, "/api/v1/me/activities?type=walk");
        var recent = await GetJsonAsync(owner, $"/api/v1/me/activities?from={from}");

        Assert.Equal([walk], walks.GetProperty("items").EnumerateArray().Select(i => i.GetProperty("id").GetGuid()));
        Assert.Equal([recentRun], recent.GetProperty("items").EnumerateArray().Select(i => i.GetProperty("id").GetGuid()));
    }

    [Theory]
    [InlineData("limit=51", "limit")]
    [InlineData("limit=0", "limit")]
    [InlineData("cursor=not-a-cursor", "cursor")]
    public async Task ListMine_InvalidQuery_Returns400ForThatField(string query, string field)
    {
        var response = await SendAsync(await RunnerAsync(), HttpMethod.Get, $"/api/v1/me/activities?{query}");
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>(Ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.True(problem.GetProperty("errors").TryGetProperty(field, out _), problem.ToString());
    }

    // Wednesday 25 Nov 2026 10:00 AST: the week started on Monday 23 at 00:00 AST (04:00 UTC)
    [Theory]
    [InlineData(SummaryPeriod.Week, "2026-11-23T04:00:00Z", "2026-11-30T04:00:00Z", "2026-11-16T04:00:00Z")]
    [InlineData(SummaryPeriod.Month, "2026-11-01T04:00:00Z", "2026-12-01T04:00:00Z", "2026-10-01T04:00:00Z")]
    [InlineData(SummaryPeriod.Year, "2026-01-01T04:00:00Z", "2027-01-01T04:00:00Z", "2025-01-01T04:00:00Z")]
    public void Bounds_WednesdayMorningInSantoDomingo_FollowTheCalendarOfTheGame(SummaryPeriod period, string from, string to, string previousFrom)
    {
        var (currentFrom, currentTo, previous) = ActivityQueryService.Bounds(period, DateTimeOffset.Parse("2026-11-25T14:00:00Z"));

        Assert.Equal((DateTimeOffset.Parse(from), DateTimeOffset.Parse(to), DateTimeOffset.Parse(previousFrom)), (currentFrom, currentTo, previous));
    }

    [Fact]
    public void Bounds_SundayNightInSantoDomingo_IsStillTheSameWeek()
    {
        // Sunday 29 Nov 23:30 AST = Monday 30 Nov 03:30 UTC
        var (currentFrom, _, _) = ActivityQueryService.Bounds(SummaryPeriod.Week, DateTimeOffset.Parse("2026-11-30T03:30:00Z"));

        Assert.Equal(DateTimeOffset.Parse("2026-11-23T04:00:00Z"), currentFrom);
    }

    [Fact]
    public async Task Summary_Year_SplitsTheTotalsByPeriodAndLeavesOutActivitiesInReview()
    {
        var owner = await RunnerAsync();
        var (yearStart, _, _) = ActivityQueryService.Bounds(SummaryPeriod.Year, _app.Clock.GetUtcNow());
        var hoursIntoYear = (int)(_app.Clock.GetUtcNow() - yearStart).TotalHours;
        await UploadProcessedAsync(owner, hoursAgo: Math.Min(110, hoursIntoYear - 2));
        await UploadProcessedAsync(owner, hoursAgo: hoursIntoYear + (24 * 30));
        await UploadProcessedAsync(owner, hoursAgo: Math.Min(112, hoursIntoYear - 4), speedMps: 1000.0 / 146, intervalS: 5, points: 241);

        var summary = await GetJsonAsync(owner, "/api/v1/me/summary?period=year");

        Assert.Equal("year", summary.GetProperty("period").GetString());
        Assert.Equal(1, summary.GetProperty("current").GetProperty("activities").GetInt32());
        Assert.Equal(5400m, summary.GetProperty("current").GetProperty("distanceM").GetDecimal());
        Assert.Equal(333, summary.GetProperty("current").GetProperty("avgPaceSPerKm").GetInt32());
        Assert.Equal(1, summary.GetProperty("previous").GetProperty("activities").GetInt32());
    }

    [Fact]
    public async Task Summary_WithoutPeriod_Returns400()
    {
        var response = await SendAsync(await RunnerAsync(), HttpMethod.Get, "/api/v1/me/summary");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Patch_OwnActivity_UpdatesTheSentFieldsAndClearsWithNull()
    {
        var owner = await RunnerAsync();
        var id = await UploadProcessedAsync(owner, hoursAgo: 77, points: 20);
        await PatchAsync(owner, id, new { title = "Rodaje", description = "Suave" });

        var response = await SendAsync(owner, HttpMethod.Patch, $"/api/v1/activities/{id}", """{ "title": null, "visibility": "private" }""");
        var activity = await response.Content.ReadFromJsonAsync<JsonElement>(Ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(JsonValueKind.Null, activity.GetProperty("title").ValueKind);
        Assert.Equal("Suave", activity.GetProperty("description").GetString());
        Assert.Equal("private", activity.GetProperty("visibility").GetString());
    }

    // D-38: 10.8 km/h as a walk takes the last row of the walking table (5.0 MET): 5.0 × 70 kg × 0.5 h = 175 kcal
    [Fact]
    public async Task Patch_TypeFromRunToWalk_RecalculatesTheCalories()
    {
        var owner = await RunnerAsync();
        var id = await UploadProcessedAsync(owner, hoursAgo: 78);

        var activity = await (await SendAsync(owner, HttpMethod.Patch, $"/api/v1/activities/{id}", """{ "type": "walk" }"""))
            .Content.ReadFromJsonAsync<JsonElement>(Ct);

        Assert.Equal("walk", activity.GetProperty("type").GetString());
        Assert.Equal(175, activity.GetProperty("calories").GetInt32());
        Assert.Equal(5400m, activity.GetProperty("distanceM").GetDecimal());
    }

    [Fact]
    public async Task Patch_PublicActivityOfAnotherRunner_Returns403()
    {
        var owner = await RunnerAsync();
        var id = await UploadProcessedAsync(owner, hoursAgo: 79, points: 20);
        await PatchAsync(owner, id, new { visibility = "public" });

        var response = await SendAsync(await RunnerAsync(), HttpMethod.Patch, $"/api/v1/activities/{id}", """{ "title": "Mía" }""");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal("FORBIDDEN", await AuthTestApp.CodeOfAsync(response));
    }

    [Theory]
    [InlineData("""{ "type": null }""", "type")]
    [InlineData("""{ "visibility": null }""", "visibility")]
    [InlineData("""{ "type": "cycling" }""", "type")]
    public async Task Patch_InvalidBody_Returns400ForThatField(string body, string field)
    {
        var owner = await RunnerAsync();
        var id = await UploadProcessedAsync(owner, hoursAgo: 80, points: 20);

        var response = await SendAsync(owner, HttpMethod.Patch, $"/api/v1/activities/{id}", body);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>(Ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.True(problem.GetProperty("errors").TryGetProperty(field, out _), problem.ToString());
    }

    [Fact]
    public async Task Patch_TitleTooLongOrShoeOfAnotherRunner_Returns400()
    {
        var owner = await RunnerAsync();
        var id = await UploadProcessedAsync(owner, hoursAgo: 81, points: 20);
        var foreignShoe = await AddShoeAsync((await RunnerAsync()).Id);

        var longTitle = await SendAsync(owner, HttpMethod.Patch, $"/api/v1/activities/{id}", JsonSerializer.Serialize(new { title = new string('x', 81) }));
        var shoe = await SendAsync(owner, HttpMethod.Patch, $"/api/v1/activities/{id}", JsonSerializer.Serialize(new { shoeId = foreignShoe }));

        Assert.Equal(HttpStatusCode.BadRequest, longTitle.StatusCode);
        Assert.True((await shoe.Content.ReadFromJsonAsync<JsonElement>(Ct)).GetProperty("errors").TryGetProperty("shoeId", out _));
    }

    // LLD §4.5: the activity goes with its points and leaves the runner's totals; the last activity date goes back
    [Fact]
    public async Task Delete_OwnProcessedActivity_RemovesItAndSubtractsItFromTheTotals()
    {
        var owner = await RunnerAsync();
        var older = await UploadProcessedAsync(owner, hoursAgo: 85, points: 20);
        var newer = await UploadProcessedAsync(owner, hoursAgo: 84);
        var olderEndedAt = (await GetJsonAsync(owner, $"/api/v1/activities/{older}")).GetProperty("endedAt").GetDateTimeOffset();
        var olderDistance = (await GetJsonAsync(owner, $"/api/v1/activities/{older}")).GetProperty("distanceM").GetDecimal();

        var response = await SendAsync(owner, HttpMethod.Delete, $"/api/v1/activities/{newer}");
        var stats = (await GetJsonAsync(owner, "/api/v1/me")).GetProperty("stats");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await SendAsync(owner, HttpMethod.Get, $"/api/v1/activities/{newer}")).StatusCode);
        Assert.Equal(0, await _app.QueryAsync(db => db.GpsPoints.CountAsync(p => p.ActivityId == newer, Ct)));
        Assert.Equal(1, stats.GetProperty("totalActivities").GetInt32());
        Assert.Equal((long)Math.Round(olderDistance), stats.GetProperty("totalDistanceM").GetInt64());
        Assert.Equal(olderEndedAt, stats.GetProperty("lastActivityAt").GetDateTimeOffset());
    }

    [Fact]
    public async Task Delete_ActivityInReview_LeavesTheTotalsAsTheyWere()
    {
        var owner = await RunnerAsync();
        var id = await UploadProcessedAsync(owner, hoursAgo: 86, speedMps: 1000.0 / 146, intervalS: 5, points: 241);

        await SendAsync(owner, HttpMethod.Delete, $"/api/v1/activities/{id}");
        var stats = (await GetJsonAsync(owner, "/api/v1/me")).GetProperty("stats");

        Assert.Equal(0, stats.GetProperty("totalActivities").GetInt32());
        Assert.False(await _app.QueryAsync(db => db.ActivityReviews.AnyAsync(r => r.ActivityId == id, Ct)));
    }

    [Fact]
    public async Task Delete_ActivityOfAnotherRunner_Returns403WhenVisibleAnd404WhenNot()
    {
        var owner = await RunnerAsync();
        var publicId = await UploadProcessedAsync(owner, hoursAgo: 87, points: 20);
        await PatchAsync(owner, publicId, new { visibility = "public" });
        var followersId = await UploadProcessedAsync(owner, hoursAgo: 88, points: 20);
        var other = await RunnerAsync();

        var forbidden = await SendAsync(other, HttpMethod.Delete, $"/api/v1/activities/{publicId}");
        var notFound = await SendAsync(other, HttpMethod.Delete, $"/api/v1/activities/{followersId}");

        Assert.Equal(HttpStatusCode.Forbidden, forbidden.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, notFound.StatusCode);
        Assert.Equal(2, await _app.QueryAsync(db => db.Activities.CountAsync(a => a.UserId == owner.Id, Ct)));
    }

    private sealed record Runner(Guid Id, string Token);

    private async Task<Runner> RunnerAsync()
    {
        var session = await _app.RegisterOkAsync(AuthTestApp.NewEmail());
        return new Runner(session.GetProperty("user").GetProperty("id").GetGuid(), session.GetProperty("accessToken").GetString()!);
    }

    // A straight run north on a made-up meridian, processed by the production pipeline
    private async Task<Guid> UploadProcessedAsync(
        Runner runner, int hoursAgo, double speedMps = 3, double intervalS = 10, int points = 181, string type = "run", double? metersPerLeg = null)
    {
        var step = metersPerLeg ?? speedMps * intervalS;
        var startedAt = new DateTimeOffset(_app.Clock.GetUtcNow().UtcDateTime, TimeSpan.Zero).AddHours(-hoursAgo);
        var body = new
        {
            clientId = Guid.NewGuid(),
            type,
            startedAt,
            endedAt = startedAt.AddSeconds(((points - 1) * intervalS) + 1),
            device = "Test phone",
            appVersion = "0.1.0",
            points = Enumerable.Range(0, points).Select(i => new
            {
                seq = i,
                segment = 0,
                recordedAt = startedAt.AddSeconds(i * intervalS),
                lat = 10.0 + (i * step / _earthRadiusM * 180 / Math.PI),
                lon = -60.0,
                accuracyM = 5,
            }),
        };
        var response = await SendAsync(runner, HttpMethod.Post, "/api/v1/activities", JsonSerializer.Serialize(body, JsonSerializerOptions.Web));
        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        var id = (await response.Content.ReadFromJsonAsync<JsonElement>(Ct)).GetProperty("id").GetGuid();

        var deadline = DateTimeOffset.UtcNow.AddSeconds(15);
        while (await _app.QueryAsync(db => db.Activities.AnyAsync(a => a.Id == id && a.Status == ActivityStatus.Pending, Ct)))
        {
            Assert.True(DateTimeOffset.UtcNow < deadline, "The activity was not processed in 15 s.");
            await Task.Delay(TimeSpan.FromMilliseconds(100), Ct);
        }

        return id;
    }

    private async Task PatchAsync(Runner runner, Guid id, object body) =>
        Assert.Equal(HttpStatusCode.OK, (await SendAsync(runner, HttpMethod.Patch, $"/api/v1/activities/{id}", JsonSerializer.Serialize(body))).StatusCode);

    private async Task<JsonElement> GetJsonAsync(Runner runner, string path)
    {
        var response = await SendAsync(runner, HttpMethod.Get, path);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return await response.Content.ReadFromJsonAsync<JsonElement>(Ct);
    }

    private Task<HttpResponseMessage> SendAsync(Runner runner, HttpMethod method, string path, string? json = null)
    {
        var request = new HttpRequestMessage(method, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", runner.Token);
        if (json is not null)
        {
            request.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        }

        return _app.Client.SendAsync(request, Ct);
    }

    private Task<Guid> AddShoeAsync(Guid userId) =>
        _app.QueryAsync(async db =>
        {
            var shoe = new Shoe { Id = Guid.CreateVersion7(), UserId = userId, Brand = "Brand", Model = "Model" };
            db.Shoes.Add(shoe);
            await db.SaveChangesAsync(Ct);
            return shoe.Id;
        });

    private static double[] First(JsonElement route) =>
        route.GetProperty("geometry").GetProperty("coordinates").EnumerateArray().First().EnumerateArray().Select(c => c.GetDouble()).ToArray();

    private static double[] Last(JsonElement route) =>
        route.GetProperty("geometry").GetProperty("coordinates").EnumerateArray().Last().EnumerateArray().Select(c => c.GetDouble()).ToArray();

    private static double MetersNorth(double[] lonLat) => (lonLat[1] - 10.0) * Math.PI / 180 * _earthRadiusM;
}
