using System.Net;
using System.Net.Http.Json;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Terrun.Api.Tests.Infrastructure;
using Terrun.Domain.Errors;

namespace Terrun.Api.Tests.Platform;

// CTZ-106 (LLD §3.1, §3.4): the OpenAPI declares the errors of every operation, and the errors the API really writes have
// the declared shape, so the app types (CTZ-19) can be trusted
public sealed class OpenApiErrorResponsesTests : IAsyncLifetime
{
    private const string _problemJson = "application/problem+json";
    private const string _problemRef = "#/components/schemas/ApiProblem";

    private readonly TerrunApiFactory _factory = new(TerrunApiFactory.UnreachableDatabase);
    private HttpClient _client = null!;
    private JsonElement _document;

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        _client = _factory.CreateClient();
        _document = await _client.GetFromJsonAsync<JsonElement>("/openapi/v1.json", Ct);
    }

    public async ValueTask DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }

    [Fact]
    public void ApiProblem_Always_ListsEveryCodeOfTheCatalog()
    {
        var schema = _document.GetProperty("components").GetProperty("schemas").GetProperty("ApiProblem");

        var declared = schema.GetProperty("properties").GetProperty("code").GetProperty("enum")
            .EnumerateArray().Select(code => code.GetString()).ToHashSet();
        var catalog = typeof(ErrorCodes).GetFields(BindingFlags.Public | BindingFlags.Static)
            .Select(field => (string?)field.GetRawConstantValue()).ToHashSet();

        Assert.Equal(catalog, declared);
        Assert.True(schema.GetProperty("properties").TryGetProperty("traceId", out _));
        Assert.True(schema.GetProperty("properties").TryGetProperty("errors", out _));
    }

    [Fact]
    public void EveryOperation_Always_DeclaresTheSharedErrorsAsApiProblem()
    {
        foreach (var (path, method, operation) in Operations())
        {
            var responses = operation.GetProperty("responses");
            foreach (var status in new[] { "400", "429", "500" })
            {
                Assert.True(responses.TryGetProperty(status, out var response), $"{method} {path} lacks {status}");
                Assert.Equal(_problemRef, SchemaRef(response));
            }

            var isProtected = operation.TryGetProperty("security", out _);
            if (isProtected)
            {
                Assert.True(responses.TryGetProperty("401", out var unauthorized), $"{method} {path} lacks 401");
                Assert.Contains("INVALID_TOKEN", unauthorized.GetProperty("description").GetString(), StringComparison.Ordinal);
            }

            var hasBody = operation.TryGetProperty("requestBody", out _);
            Assert.True(hasBody == responses.TryGetProperty("413", out _), $"{method} {path}: 413 vs body");
            Assert.True(hasBody == responses.TryGetProperty("415", out _), $"{method} {path}: 415 vs body");
        }
    }

    [Theory]
    [InlineData("/api/v1/auth/register", "post", "409", "EMAIL_ALREADY_REGISTERED", "HANDLE_TAKEN")]
    [InlineData("/api/v1/auth/register", "post", "400", "CONSENT_REQUIRED", "VALIDATION_ERROR")]
    [InlineData("/api/v1/auth/login", "post", "401", "INVALID_CREDENTIALS")]
    [InlineData("/api/v1/auth/login", "post", "403", "USER_SUSPENDED")]
    [InlineData("/api/v1/auth/refresh", "post", "403", "USER_SUSPENDED")]
    [InlineData("/api/v1/auth/verify-email", "post", "400", "VERIFICATION_CODE_INVALID", "VALIDATION_ERROR")]
    [InlineData("/api/v1/me", "get", "404", "NOT_FOUND")]
    [InlineData("/api/v1/me/profile", "patch", "409", "HANDLE_TAKEN")]
    public void Operation_WithItsOwnErrors_ListsTheirCodes(string path, string method, string status, params string[] codes)
    {
        var description = _document.GetProperty("paths").GetProperty(path).GetProperty(method)
            .GetProperty("responses").GetProperty(status).GetProperty("description").GetString()!;

        Assert.All(codes, code => Assert.Contains(code, description, StringComparison.Ordinal));
    }

    // The JWT challenge has no body of its own: the status code pages turn it into the declared problem
    // An anonymous operation only declares 401 when it can really answer it (login and refresh)
    [Fact]
    public void AnonymousOperation_WithoutOwn401_DoesNotDeclareIt()
    {
        var register = _document.GetProperty("paths").GetProperty("/api/v1/auth/register").GetProperty("post");

        Assert.False(register.GetProperty("responses").TryGetProperty("401", out _));
    }

    [Fact]
    public async Task ProtectedOperation_WithoutToken_AnswersTheDeclaredProblem()
    {
        var response = await _client.GetAsync("/api/v1/me", Ct);

        await AssertProblemAsync(response, HttpStatusCode.Unauthorized, ErrorCodes.InvalidToken);
    }

    [Fact]
    public async Task BodyOperation_WithoutJson_AnswersTheDeclaredProblem()
    {
        using var content = new StringContent("email=a", Encoding.UTF8, "text/plain");

        var response = await _client.PostAsync("/api/v1/auth/login", content, Ct);

        await AssertProblemAsync(response, HttpStatusCode.UnsupportedMediaType, ErrorCodes.ValidationError);
    }

    private static async Task AssertProblemAsync(HttpResponseMessage response, HttpStatusCode status, string code)
    {
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>(Ct);

        Assert.Equal(status, response.StatusCode);
        Assert.Equal(_problemJson, response.Content.Headers.ContentType?.MediaType);
        Assert.Equal(code, problem.GetProperty("code").GetString());
        Assert.Equal((int)status, problem.GetProperty("status").GetInt32());
        Assert.False(string.IsNullOrEmpty(problem.GetProperty("traceId").GetString()));
    }

    private IEnumerable<(string Path, string Method, JsonElement Operation)> Operations() =>
        _document.GetProperty("paths").EnumerateObject()
            .SelectMany(path => path.Value.EnumerateObject().Select(method => (path.Name, method.Name, method.Value)));

    private static string? SchemaRef(JsonElement response) =>
        response.GetProperty("content").GetProperty(_problemJson).GetProperty("schema").GetProperty("$ref").GetString();
}
