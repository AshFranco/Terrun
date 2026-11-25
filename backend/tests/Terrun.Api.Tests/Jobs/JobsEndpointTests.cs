using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Terrun.Api.Tests.Identity;
using Terrun.Api.Tests.Infrastructure;
using Terrun.Api.Tests.Persistence;

namespace Terrun.Api.Tests.Jobs;

// CTZ-104 (LLD §6): POST /internal/jobs/run only answers to the X-Jobs-Key shared with the GitHub Actions workflow
[Collection(MigratedDatabaseCollection.Name)]
public sealed class JobsEndpointTests(MigratedDatabaseFixture database)
{
    private const string _path = "/api/v1/internal/jobs/run";
    private const string _key = "test-jobs-key-0123456789abcdefghijklmnop";

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Theory]
    [InlineData(null)]
    [InlineData("wrong-key-0123456789abcdefghijklmnopqrstuv")]
    public async Task Run_WithoutTheRightKey_Returns401(string? key)
    {
        await using var app = new AuthTestApp(database, settings: new() { ["Jobs:Key"] = _key });

        var response = await PostAsync(app.Client, key);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("INVALID_TOKEN", await AuthTestApp.CodeOfAsync(response));
    }

    // A deployment without Jobs__Key refuses every call instead of running jobs for anyone
    [Fact]
    public async Task Run_KeyNotConfigured_Returns401EvenWithAKey()
    {
        await using var app = new AuthTestApp(database);

        var response = await PostAsync(app.Client, _key);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Run_WithTheRightKey_RunsTheRegisteredJobsAndListsTheirOutcome()
    {
        await using var app = new AuthTestApp(database, settings: new() { ["Jobs:Key"] = _key });

        var response = await PostAsync(app.Client, _key);
        var jobs = (await response.Content.ReadFromJsonAsync<JsonElement>(Ct)).GetProperty("jobs").EnumerateArray()
            .ToDictionary(j => j.GetProperty("name").GetString()!, j => j.GetProperty("outcome").GetString());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("activity_processor", jobs.Keys);
        Assert.Contains("token_cleanup", jobs.Keys);
        Assert.All(jobs.Values, outcome => Assert.Contains(outcome, new[] { "succeeded", "not_due" }));
    }

    [Fact]
    public async Task OpenApi_Always_LeavesTheInternalEndpointOut()
    {
        await using var factory = new TerrunApiFactory(TerrunApiFactory.UnreachableDatabase);
        using var client = factory.CreateClient();

        var document = await client.GetStringAsync("/openapi/v1.json", Ct);

        Assert.DoesNotContain("internal/jobs", document, StringComparison.Ordinal);
    }

    private static Task<HttpResponseMessage> PostAsync(HttpClient client, string? key)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, _path);
        if (key is not null)
        {
            request.Headers.Add("X-Jobs-Key", key);
        }

        return client.SendAsync(request, Ct);
    }
}
