using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Terrun.Api.Tests.Infrastructure;

namespace Terrun.Api.Tests.Platform;

public sealed class OpenApiTests
{
    [Fact]
    public async Task GetOpenApiDocument_OutsideProduction_ReturnsOpenApi31()
    {
        await using var factory = new TerrunApiFactory(TerrunApiFactory.UnreachableDatabase);
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/openapi/v1.json", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("\"openapi\": \"3.1", await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken), StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetSwaggerUi_OutsideProduction_ReturnsPage()
    {
        await using var factory = new TerrunApiFactory(TerrunApiFactory.UnreachableDatabase);
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/swagger/index.html", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetOpenApiDocument_Always_DeclaresBearerOnlyOnProtectedOperations()
    {
        await using var factory = new TerrunApiFactory(TerrunApiFactory.UnreachableDatabase);
        using var client = factory.CreateClient();

        var document = await client.GetFromJsonAsync<JsonElement>("/openapi/v1.json", TestContext.Current.CancellationToken);
        var paths = document.GetProperty("paths");

        Assert.Equal("bearer", document.GetProperty("components").GetProperty("securitySchemes").GetProperty("Bearer").GetProperty("scheme").GetString());
        Assert.True(paths.GetProperty("/api/v1/auth/logout").GetProperty("post").TryGetProperty("security", out _));
        Assert.False(paths.GetProperty("/api/v1/auth/login").GetProperty("post").TryGetProperty("security", out _));
    }

    // LLD §3.1: query parameters in camelCase and GeoJSON "type" always present, as the app types read them (CTZ-32)
    [Fact]
    public async Task GetOpenApiDocument_ActivityEndpoints_UseCamelCaseQueriesAndARequiredGeoJsonType()
    {
        await using var factory = new TerrunApiFactory(TerrunApiFactory.UnreachableDatabase);
        using var client = factory.CreateClient();

        var document = await client.GetFromJsonAsync<JsonElement>("/openapi/v1.json", TestContext.Current.CancellationToken);
        var parameters = document.GetProperty("paths").GetProperty("/api/v1/me/activities").GetProperty("get").GetProperty("parameters")
            .EnumerateArray().Select(p => p.GetProperty("name").GetString());
        var routeRequired = document.GetProperty("components").GetProperty("schemas").GetProperty("RouteResponse").GetProperty("required")
            .EnumerateArray().Select(r => r.GetString());

        Assert.Equal(["type", "from", "to", "limit", "cursor"], parameters);
        Assert.Contains("type", routeRequired);
    }

    // CTZ-17 (LLD §3.2): public legal/settings endpoints have no bearer requirement, /me/consents requires bearer, and
    // their typed schemas are published for npm run api:types
    [Fact]
    public async Task GetOpenApiDocument_LegalAndPublicSettingsEndpoints_DeclareExpectedSecurityAndSchemas()
    {
        await using var factory = new TerrunApiFactory(TerrunApiFactory.UnreachableDatabase);
        using var client = factory.CreateClient();

        var document = await client.GetFromJsonAsync<JsonElement>("/openapi/v1.json", TestContext.Current.CancellationToken);
        var paths = document.GetProperty("paths");
        var schemas = document.GetProperty("components").GetProperty("schemas");

        Assert.False(paths.GetProperty("/api/v1/legal/current").GetProperty("get").TryGetProperty("security", out _));
        Assert.False(paths.GetProperty("/api/v1/settings/public").GetProperty("get").TryGetProperty("security", out _));
        Assert.True(paths.GetProperty("/api/v1/me/consents").GetProperty("post").TryGetProperty("security", out _));
        Assert.True(schemas.TryGetProperty("LegalDocumentResponse", out _));
        Assert.True(schemas.TryGetProperty("AcceptConsentsRequest", out _));
        Assert.True(schemas.TryGetProperty("ConsentResponse", out _));
        Assert.True(schemas.TryGetProperty("PublicSettingsResponse", out var publicSettings));
        Assert.True(publicSettings.GetProperty("properties").TryGetProperty("territory", out _));
        Assert.True(publicSettings.GetProperty("properties").TryGetProperty("activity", out _));
        Assert.True(publicSettings.GetProperty("properties").TryGetProperty("xp", out _));
        Assert.True(publicSettings.GetProperty("properties").TryGetProperty("privacy", out _));
    }
}
