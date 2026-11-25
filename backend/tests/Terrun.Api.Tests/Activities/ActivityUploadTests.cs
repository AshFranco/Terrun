using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Terrun.Api.Tests.Identity;
using Terrun.Api.Tests.Persistence;
using Terrun.Domain.Activities;
using Terrun.Domain.Common;
using Terrun.Domain.Equipment;

namespace Terrun.Api.Tests.Activities;

// CTZ-30 (RF-16, LLD §3.3, D-34): POST /activities is idempotent by clientId (MT-09), limited in points (413) and
// duration, and never overlaps another activity of the same runner (409)
[Collection(MigratedDatabaseCollection.Name)]
public sealed class ActivityUploadTests(MigratedDatabaseFixture database) : IAsyncLifetime
{
    private AuthTestApp _app = null!;

    public ValueTask InitializeAsync()
    {
        _app = new AuthTestApp(database);
        return ValueTask.CompletedTask;
    }

    public ValueTask DisposeAsync() => _app.DisposeAsync();

    [Fact]
    public async Task Upload_NewActivity_Returns202PendingAndStoresItWithItsPoints()
    {
        var runner = await RunnerAsync();
        var upload = ActivityUpload.At(Today(8), points: 120);

        var response = await PostAsync(runner.Token, upload.Body());
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(AuthTestApp.Ct);
        var id = body.GetProperty("id").GetGuid();
        var stored = await _app.QueryAsync(db => db.Activities.AsNoTracking().SingleAsync(a => a.Id == id, AuthTestApp.Ct));
        var points = await _app.QueryAsync(db => db.GpsPoints.AsNoTracking().Where(p => p.ActivityId == id).OrderBy(p => p.Seq).ToListAsync(AuthTestApp.Ct));

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        Assert.Equal("pending", body.GetProperty("status").GetString());
        Assert.Equal(runner.Id, stored.UserId);
        Assert.Equal(ActivityStatus.Pending, stored.Status);
        Assert.Equal(ActivityType.Run, stored.Type);
        Assert.Equal(Visibility.Followers, stored.Visibility);
        Assert.Equal(upload.StartedAt, stored.StartedAt);
        Assert.Null(stored.DistanceM);
        Assert.Equal(120, points.Count);
        Assert.Equal([0, 1], points.Select(p => (int)p.Segment).Distinct());
        Assert.All(points, p => Assert.False(p.IsDiscarded));
        Assert.Equal(upload.Points[5].Lat, points[5].Lat);
    }

    [Fact]
    [Trait("Case", "MT-09")]
    public async Task Upload_SameClientIdTwice_Returns200WithTheSameActivityAndNoDuplicatedPoints()
    {
        var runner = await RunnerAsync();
        var upload = ActivityUpload.At(Today(9), points: 50);

        var first = await PostAsync(runner.Token, upload.Body());
        var second = await PostAsync(runner.Token, upload.Body());
        var firstId = (await first.Content.ReadFromJsonAsync<JsonElement>(AuthTestApp.Ct)).GetProperty("id").GetGuid();
        var secondId = (await second.Content.ReadFromJsonAsync<JsonElement>(AuthTestApp.Ct)).GetProperty("id").GetGuid();

        Assert.Equal(HttpStatusCode.Accepted, first.StatusCode);
        Assert.Equal(HttpStatusCode.OK, second.StatusCode);
        Assert.Equal(firstId, secondId);
        Assert.Equal(1, await CountActivitiesAsync(runner.Id));
        Assert.Equal(50, await _app.QueryAsync(db => db.GpsPoints.CountAsync(p => p.ActivityId == firstId, AuthTestApp.Ct)));
    }

    // The phone retries while the first request is still running: the unique index keeps one and both answer its id
    [Fact]
    [Trait("Case", "MT-09")]
    public async Task Upload_SameClientIdAtTheSameTime_KeepsOneActivity()
    {
        var runner = await RunnerAsync();
        var upload = ActivityUpload.At(Today(10), points: 2000);

        var responses = await Task.WhenAll(Enumerable.Range(0, 8).Select(_ => PostAsync(runner.Token, upload.Body())));
        var ids = await Task.WhenAll(responses.Select(async r => (await r.Content.ReadFromJsonAsync<JsonElement>(AuthTestApp.Ct)).GetProperty("id").GetGuid()));

        Assert.All(responses, r => Assert.True(r.StatusCode is HttpStatusCode.Accepted or HttpStatusCode.OK, $"Unexpected {r.StatusCode}"));
        Assert.Single(ids.Distinct());
        Assert.Equal(1, await CountActivitiesAsync(runner.Id));
    }

    [Fact]
    public async Task Upload_SameClientIdOfAnotherRunner_CreatesAnIndependentActivity()
    {
        var upload = ActivityUpload.At(Today(11), points: 10);

        var first = await PostAsync((await RunnerAsync()).Token, upload.Body());
        var second = await PostAsync((await RunnerAsync()).Token, upload.Body());

        Assert.Equal(HttpStatusCode.Accepted, first.StatusCode);
        Assert.Equal(HttpStatusCode.Accepted, second.StatusCode);
    }

    [Fact]
    public async Task Upload_WithoutToken_Returns401()
    {
        var response = await _app.Client.PostAsJsonAsync("/api/v1/activities", ActivityUpload.At(Today(12), points: 10).Body(), AuthTestApp.Ct);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Upload_MoreThanMaxPoints_Returns413PayloadTooLarge()
    {
        var runner = await RunnerAsync();

        var response = await PostAsync(runner.Token, ActivityUpload.At(Today(0), points: 30_001, everySeconds: 1).Body());

        Assert.Equal(HttpStatusCode.RequestEntityTooLarge, response.StatusCode);
        Assert.Equal("PAYLOAD_TOO_LARGE", await AuthTestApp.CodeOfAsync(response));
        Assert.Equal(0, await CountActivitiesAsync(runner.Id));
    }

    // The largest upload the settings allow is stored completely and quickly (binary COPY of the points)
    [Fact]
    public async Task Upload_ExactlyMaxPoints_StoresEveryPointInAFewSeconds()
    {
        var runner = await RunnerAsync();
        var clock = Stopwatch.StartNew();

        var response = await PostAsync(runner.Token, ActivityUpload.At(Today(0), points: 30_000, everySeconds: 1).Body());
        var id = (await response.Content.ReadFromJsonAsync<JsonElement>(AuthTestApp.Ct)).GetProperty("id").GetGuid();

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        Assert.Equal(30_000, await _app.QueryAsync(db => db.GpsPoints.CountAsync(p => p.ActivityId == id, AuthTestApp.Ct)));
        Assert.True(clock.Elapsed < TimeSpan.FromSeconds(15), $"Took {clock.Elapsed}");
    }

    [Fact]
    public async Task Upload_OverlappingAnotherActivity_Returns409ActivityOverlap()
    {
        var runner = await RunnerAsync();
        await PostAsync(runner.Token, ActivityUpload.At(Today(13), points: 60).Body());

        var response = await PostAsync(runner.Token, ActivityUpload.At(Today(13).AddMinutes(1), points: 60).Body());

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("ACTIVITY_OVERLAP", await AuthTestApp.CodeOfAsync(response));
    }

    [Fact]
    public async Task Upload_StartingExactlyWhenTheOtherEnds_IsAccepted()
    {
        var runner = await RunnerAsync();
        var first = ActivityUpload.At(Today(14), points: 60);
        await PostAsync(runner.Token, first.Body());

        var response = await PostAsync(runner.Token, ActivityUpload.At(first.EndedAt, points: 60).Body());

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
    }

    [Theory]
    [InlineData(ActivityStatus.Rejected)]
    [InlineData(ActivityStatus.Failed)]
    public async Task Upload_OverlappingARejectedOrFailedActivity_IsAccepted(ActivityStatus status)
    {
        var runner = await RunnerAsync();
        var first = await PostAsync(runner.Token, ActivityUpload.At(Today(15), points: 60).Body());
        var firstId = (await first.Content.ReadFromJsonAsync<JsonElement>(AuthTestApp.Ct)).GetProperty("id").GetGuid();
        await _app.QueryAsync(db => db.Activities.Where(a => a.Id == firstId).ExecuteUpdateAsync(set => set.SetProperty(a => a.Status, status), AuthTestApp.Ct));

        var response = await PostAsync(runner.Token, ActivityUpload.At(Today(15).AddMinutes(1), points: 60).Body());

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
    }

    // Different activities of the same time uploaded at once: only one gets in and the rest answer 409, never 500
    // (concurrent inserts waiting on an EXCLUDE constraint can deadlock; D-34). Several rounds, because it is a race
    [Fact]
    public async Task Upload_OverlappingActivitiesAtTheSameTime_AcceptsOnlyOneAndTheRestGet409()
    {
        for (var round = 0; round < 3; round++)
        {
            var runner = await RunnerAsync();

            var responses = await Task.WhenAll(Enumerable.Range(0, 8)
                .Select(i => PostAsync(runner.Token, ActivityUpload.At(Today(16).AddSeconds(i), points: 1500).Body())));

            Assert.Equal(1, responses.Count(r => r.StatusCode == HttpStatusCode.Accepted));
            Assert.All(responses.Where(r => r.StatusCode != HttpStatusCode.Accepted), r => Assert.Equal(HttpStatusCode.Conflict, r.StatusCode));
            Assert.Equal(1, await CountActivitiesAsync(runner.Id));
        }
    }

    [Fact]
    public async Task Upload_ShoeOfAnotherRunner_Returns400ForShoeId()
    {
        var owner = await RunnerAsync();
        var shoeId = await AddShoeAsync(owner.Id);
        var runner = await RunnerAsync();

        var response = await PostAsync(runner.Token, ActivityUpload.At(Today(17), points: 10).Body(shoeId: shoeId));
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>(AuthTestApp.Ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("VALIDATION_ERROR", problem.GetProperty("code").GetString());
        Assert.True(problem.GetProperty("errors").TryGetProperty("shoeId", out _));
    }

    [Fact]
    public async Task Upload_OwnShoe_IsStoredWithTheActivity()
    {
        var runner = await RunnerAsync();
        var shoeId = await AddShoeAsync(runner.Id);

        var response = await PostAsync(runner.Token, ActivityUpload.At(Today(18), points: 10).Body(shoeId: shoeId));
        var id = (await response.Content.ReadFromJsonAsync<JsonElement>(AuthTestApp.Ct)).GetProperty("id").GetGuid();

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        Assert.Equal(shoeId, await _app.QueryAsync(db => db.Activities.Where(a => a.Id == id).Select(a => a.ShoeId).SingleAsync(AuthTestApp.Ct)));
    }

    [Fact]
    public async Task Upload_PrivateDefaultVisibility_IsCopiedToTheActivity()
    {
        var runner = await RunnerAsync();
        await _app.QueryAsync(db => db.UserPreferences.Where(p => p.UserId == runner.Id)
            .ExecuteUpdateAsync(set => set.SetProperty(p => p.DefaultVisibility, Visibility.Private), AuthTestApp.Ct));

        var response = await PostAsync(runner.Token, ActivityUpload.At(Today(19), points: 10).Body());
        var id = (await response.Content.ReadFromJsonAsync<JsonElement>(AuthTestApp.Ct)).GetProperty("id").GetGuid();

        Assert.Equal(Visibility.Private, await _app.QueryAsync(db => db.Activities.Where(a => a.Id == id).Select(a => a.Visibility).SingleAsync(AuthTestApp.Ct)));
    }

    [Theory]
    [InlineData("endedBeforeStart", "endedAt")]
    [InlineData("future", "endedAt")]
    [InlineData("longerThan24h", "endedAt")]
    [InlineData("noType", "type")]
    [InlineData("unknownType", "type")]
    [InlineData("emptyClientId", "clientId")]
    [InlineData("onePoint", "points")]
    [InlineData("noPoints", "points")]
    [InlineData("latOutOfRange", "points")]
    [InlineData("missingLon", "points")]
    [InlineData("seqNotIncreasing", "points")]
    [InlineData("segmentGoesBack", "points")]
    [InlineData("pointOutsideTheTimes", "points")]
    [InlineData("negativeAccuracy", "points")]
    [InlineData("longDevice", "device")]
    public async Task Upload_InvalidBody_Returns400ValidationErrorForThatField(string scenario, string field)
    {
        var runner = await RunnerAsync();

        var response = await PostAsync(runner.Token, InvalidBody(scenario));
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>(AuthTestApp.Ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("VALIDATION_ERROR", problem.GetProperty("code").GetString());
        Assert.True(problem.GetProperty("errors").TryGetProperty(field, out _), $"Errors: {problem.GetProperty("errors")}");
        Assert.Equal(0, await CountActivitiesAsync(runner.Id));
    }

    // LLD §7.3: the serializer message names internal .NET types; the API answers a neutral message under the field
    [Fact]
    public async Task Upload_UnconvertibleValue_ReportsTheFieldWithoutInternalDetails()
    {
        var runner = await RunnerAsync();
        var body = ActivityUpload.At(Today(21), points: 10).Body();
        body["startedAt"] = "yesterday";

        var response = await PostAsync(runner.Token, body);
        var text = await response.Content.ReadAsStringAsync(AuthTestApp.Ct);
        var errors = JsonDocument.Parse(text).RootElement.GetProperty("errors");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.True(errors.TryGetProperty("startedAt", out _), text);
        Assert.False(errors.TryGetProperty("request", out _), text);
        Assert.DoesNotContain("Terrun.", text, StringComparison.Ordinal);
        Assert.DoesNotContain("$.", text, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Upload_EmptyBody_Returns400ForTheBody()
    {
        var runner = await RunnerAsync();
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/activities")
        {
            Content = new StringContent(string.Empty, System.Text.Encoding.UTF8, "application/json"),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", runner.Token);

        var response = await _app.Client.SendAsync(request, AuthTestApp.Ct);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>(AuthTestApp.Ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("VALIDATION_ERROR", problem.GetProperty("code").GetString());
        Assert.True(problem.GetProperty("errors").TryGetProperty("body", out _));
    }

    private Dictionary<string, object?> InvalidBody(string scenario)
    {
        var upload = ActivityUpload.At(Today(20), points: 10);
        var body = upload.Body();
        var points = (List<Dictionary<string, object?>>)body["points"]!;
        switch (scenario)
        {
            case "endedBeforeStart": body["endedAt"] = upload.StartedAt.AddMinutes(-1); break;
            case "future": body["endedAt"] = _app.Clock.GetUtcNow().AddHours(1); break;
            case "longerThan24h": body["startedAt"] = upload.EndedAt.AddHours(-25); points[0]["recordedAt"] = upload.EndedAt.AddHours(-25); break;
            case "noType": body.Remove("type"); break;
            case "unknownType": body["type"] = "cycling"; break;
            case "emptyClientId": body["clientId"] = Guid.Empty; break;
            case "onePoint": points.RemoveRange(1, points.Count - 1); break;
            case "noPoints": body.Remove("points"); break;
            case "latOutOfRange": points[3]["lat"] = 91.0; break;
            case "missingLon": points[3].Remove("lon"); break;
            case "seqNotIncreasing": points[4]["seq"] = 2; break;
            case "segmentGoesBack": points[^1]["segment"] = 0; points[^2]["segment"] = 1; break;
            case "pointOutsideTheTimes": points[^1]["recordedAt"] = upload.EndedAt.AddMinutes(1); break;
            case "negativeAccuracy": points[2]["accuracyM"] = -1f; break;
            case "longDevice": body["device"] = new string('x', 101); break;
        }

        return body;
    }

    // Activities in the past of the test clock, one hour apart per test, so they never overlap by accident
    private DateTimeOffset Today(int hour) =>
        new DateTimeOffset(_app.Clock.GetUtcNow().UtcDateTime.Date, TimeSpan.Zero).AddDays(-1).AddHours(hour);

    private async Task<(Guid Id, string Token)> RunnerAsync()
    {
        var session = await _app.RegisterOkAsync(AuthTestApp.NewEmail());
        return (session.GetProperty("user").GetProperty("id").GetGuid(), session.GetProperty("accessToken").GetString()!);
    }

    private Task<HttpResponseMessage> PostAsync(string token, object body)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/activities") { Content = JsonContent.Create(body) };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return _app.Client.SendAsync(request, AuthTestApp.Ct);
    }

    private Task<int> CountActivitiesAsync(Guid userId) =>
        _app.QueryAsync(db => db.Activities.CountAsync(a => a.UserId == userId, AuthTestApp.Ct));

    private Task<Guid> AddShoeAsync(Guid userId) =>
        _app.QueryAsync(async db =>
        {
            var shoe = new Shoe { Id = Guid.CreateVersion7(), UserId = userId, Brand = "Brand", Model = "Model" };
            db.Shoes.Add(shoe);
            await db.SaveChangesAsync(AuthTestApp.Ct);
            return shoe.Id;
        });
}
