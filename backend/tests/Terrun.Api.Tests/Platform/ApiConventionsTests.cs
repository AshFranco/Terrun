using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Terrun.Api.Modules.Platform;
using Terrun.Api.Tests.Infrastructure;

namespace Terrun.Api.Tests.Platform;

// LLD §3.1 (prefix, enums, rate limit), §3.4 (error catalog) and §7.3 (headers, HSTS, no internal details)
public sealed class ApiConventionsTests : IAsyncDisposable
{
    private const string _probe = "/api/v1/probe";

    private readonly List<TerrunApiFactory> _factories = [];

    [Fact]
    public async Task GetProbe_WithAndWithoutPrefix_IsServedOnlyUnderApiV1()
    {
        using var client = CreateClient();

        var prefixed = await client.GetAsync($"{_probe}/enum", Ct);
        var bare = await client.GetAsync("/probe/enum", Ct);

        Assert.Equal(HttpStatusCode.OK, prefixed.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, bare.StatusCode);
    }

    [Fact]
    public async Task GetEnum_Always_WritesEnumValuesInSnakeCase()
    {
        using var client = CreateClient();

        var body = await client.GetStringAsync($"{_probe}/enum", Ct);

        Assert.Contains("\"status\":\"owned_by_me\"", body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task PostValidated_InvalidFields_ReturnsValidationErrorPerCamelCaseField()
    {
        using var client = CreateClient();

        var response = await client.PostAsJsonAsync($"{_probe}/validated", new { email = "not-an-email", distanceM = 0 }, Ct);
        var problem = await ReadProblemAsync(response);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal("VALIDATION_ERROR", problem.GetProperty("code").GetString());
        var errors = problem.GetProperty("errors");
        Assert.True(errors.TryGetProperty("email", out _));
        Assert.True(errors.TryGetProperty("distanceM", out _));
    }

    [Fact]
    public async Task PostValidated_MalformedJson_ReturnsValidationError()
    {
        using var client = CreateClient();
        using var content = new StringContent("{ not json", Encoding.UTF8, "application/json");

        var response = await client.PostAsync($"{_probe}/validated", content, Ct);
        var problem = await ReadProblemAsync(response);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("VALIDATION_ERROR", problem.GetProperty("code").GetString());
    }

    [Theory]
    [InlineData("not-found", HttpStatusCode.NotFound, "NOT_FOUND")]
    [InlineData("conflict", HttpStatusCode.Conflict, "EMAIL_ALREADY_REGISTERED")]
    public async Task GetProbe_DomainException_ReturnsItsStatusCodeAndTraceId(string path, HttpStatusCode status, string code)
    {
        using var client = CreateClient();

        var response = await client.GetAsync($"{_probe}/{path}", Ct);
        var problem = await ReadProblemAsync(response);

        Assert.Equal(status, response.StatusCode);
        Assert.Equal(code, problem.GetProperty("code").GetString());
        Assert.False(string.IsNullOrEmpty(problem.GetProperty("traceId").GetString()));
    }

    [Fact]
    public async Task GetCrash_InProduction_ReturnsInternalErrorWithoutTheExceptionMessage()
    {
        using var client = CreateClient(environment: "Production");

        var response = await client.GetAsync($"{_probe}/crash", Ct);
        var body = await response.Content.ReadAsStringAsync(Ct);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Contains("\"code\":\"INTERNAL_ERROR\"", body, StringComparison.Ordinal);
        Assert.Contains("\"traceId\"", body, StringComparison.Ordinal);
        Assert.DoesNotContain("secret internal detail", body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetUnknownRoute_Always_ReturnsNotFoundProblem()
    {
        using var client = CreateClient();

        var response = await client.GetAsync("/api/v1/does-not-exist", Ct);
        var problem = await ReadProblemAsync(response);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("NOT_FOUND", problem.GetProperty("code").GetString());
    }

    [Fact]
    public async Task PostAuth_OverTheLimit_ReturnsRateLimitedWithRetryAfter()
    {
        using var client = CreateClient(settings: new() { ["RateLimiting:AuthPerIpPerMinute"] = "2" });

        await client.PostAsync($"{_probe}/auth", null, Ct);
        await client.PostAsync($"{_probe}/auth", null, Ct);
        var rejected = await client.PostAsync($"{_probe}/auth", null, Ct);
        var problem = await ReadProblemAsync(rejected);

        Assert.Equal(HttpStatusCode.TooManyRequests, rejected.StatusCode);
        Assert.Equal("RATE_LIMITED", problem.GetProperty("code").GetString());
        Assert.NotNull(rejected.Headers.RetryAfter);
    }

    [Fact]
    public async Task GetHealth_ManyTimes_IsNeverRateLimited()
    {
        using var client = CreateClient(settings: new() { ["RateLimiting:PerCallerPerMinute"] = "2" });

        var statuses = new List<HttpStatusCode>();
        for (var attempt = 0; attempt < 5; attempt++)
        {
            statuses.Add((await client.GetAsync(PlatformModule.HealthPath, Ct)).StatusCode);
        }

        Assert.DoesNotContain(HttpStatusCode.TooManyRequests, statuses);
    }

    [Fact]
    public async Task AnyResponse_Always_CarriesTheSecurityHeaders()
    {
        using var client = CreateClient();

        var response = await client.GetAsync($"{_probe}/enum", Ct);

        Assert.Equal("nosniff", response.Headers.GetValues("X-Content-Type-Options").Single());
        Assert.Equal("DENY", response.Headers.GetValues("X-Frame-Options").Single());
        Assert.Equal("no-referrer", response.Headers.GetValues("Referrer-Policy").Single());
        Assert.Contains("default-src 'none'", response.Headers.GetValues("Content-Security-Policy").Single(), StringComparison.Ordinal);
    }

    [Fact]
    // HSTS is never sent to localhost (ASP.NET default), so the request uses a real-looking host
    public async Task AnyHttpsResponse_InProduction_EnablesHsts()
    {
        using var client = CreateClient(environment: "Production", baseAddress: new Uri("https://api.terrun.test"));

        var response = await client.GetAsync($"{_probe}/enum", Ct);

        Assert.True(response.Headers.Contains("Strict-Transport-Security"));
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var factory in _factories)
        {
            await factory.DisposeAsync();
        }
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private HttpClient CreateClient(
        string environment = "Development",
        Dictionary<string, string?>? settings = null,
        Uri? baseAddress = null)
    {
        var factory = new TerrunApiFactory(TerrunApiFactory.UnreachableDatabase)
        {
            EnvironmentName = environment,
            Settings = settings ?? [],
            TestServices = services =>
            {
                services.AddControllers().AddApplicationPart(typeof(ProbeController).Assembly);
                services.AddScoped<IValidator<ProbeRequest>, ProbeRequestValidator>();
            },
        };
        _factories.Add(factory);

        return factory.CreateClient(new() { BaseAddress = baseAddress ?? new Uri("http://localhost") });
    }

    private static async Task<JsonElement> ReadProblemAsync(HttpResponseMessage response) =>
        (await response.Content.ReadFromJsonAsync<JsonElement>(Ct));
}
