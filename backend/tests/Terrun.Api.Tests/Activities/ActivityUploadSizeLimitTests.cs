using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Terrun.Api.Tests.Identity;
using Terrun.Api.Tests.Infrastructure;
using Terrun.Api.Tests.Persistence;

namespace Terrun.Api.Tests.Activities;

// LLD §7.3: POST /activities accepts up to 8 MB while the rest of the API stays at 2 MB. Kestrel enforces the limit and the
// in-memory test server skips it, so this runs on real Kestrel
[Collection(MigratedDatabaseCollection.Name)]
public sealed class ActivityUploadSizeLimitTests(MigratedDatabaseFixture database) : IAsyncLifetime
{
    private const int _megabyte = 1024 * 1024;

    private TerrunApiFactory _factory = null!;
    private HttpClient _client = null!;

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public ValueTask InitializeAsync()
    {
        _factory = new TerrunApiFactory(database.ConnectionString)
        {
            Settings = new() { ["RateLimiting:AuthPerIpPerMinute"] = "10000" },
        };
        _factory.UseKestrel(0);
        _factory.StartServer();
        _client = _factory.CreateClient();
        return ValueTask.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }

    [Fact]
    public async Task Upload_BodyOverTwoMegabytes_IsAccepted()
    {
        var token = await RegisterAsync();
        var body = Json(ActivityUpload.At(DateTimeOffset.UtcNow.Date.AddDays(-2), points: 20_000, everySeconds: 1).Body());

        var response = await PostAsync(token, body);

        Assert.True(Encoding.UTF8.GetByteCount(body) > 2 * _megabyte);
        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
    }

    [Fact]
    public async Task Upload_BodyOverEightMegabytes_Returns413PayloadTooLarge()
    {
        var token = await RegisterAsync();
        var body = Json(ActivityUpload.At(DateTimeOffset.UtcNow.Date.AddDays(-3), points: 70_000, everySeconds: 1).Body());

        var response = await PostAsync(token, body, expectContinue: true);

        Assert.True(Encoding.UTF8.GetByteCount(body) > 8 * _megabyte);
        Assert.Equal(HttpStatusCode.RequestEntityTooLarge, response.StatusCode);
        Assert.Equal("PAYLOAD_TOO_LARGE", (await response.Content.ReadFromJsonAsync<JsonElement>(Ct)).GetProperty("code").GetString());
    }

    private async Task<string> RegisterAsync()
    {
        var legalIds = new[] { new Guid("6f1c2d0e-7a4b-4c5d-9e8f-0a1b2c3d4e01"), new Guid("6f1c2d0e-7a4b-4c5d-9e8f-0a1b2c3d4e02") };
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email = AuthTestApp.NewEmail(),
            password = AuthTestApp.StrongPassword,
            displayName = "Runner Test",
            handle = AuthTestApp.NewHandle(),
            acceptedLegalDocumentIds = legalIds,
        }, Ct);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<JsonElement>(Ct)).GetProperty("accessToken").GetString()!;
    }

    // Without "Expect: 100-continue" Kestrel answers 413 and closes the connection while the client is still sending,
    // so the client sees a broken pipe instead of the status (the app never gets near 8 MB: 30,000 points are ~4 MB)
    private Task<HttpResponseMessage> PostAsync(string token, string json, bool expectContinue = false)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/activities")
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json"),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Headers.ExpectContinue = expectContinue;
        return _client.SendAsync(request, Ct);
    }

    private static string Json(object body) => JsonSerializer.Serialize(body, JsonSerializerOptions.Web);
}
