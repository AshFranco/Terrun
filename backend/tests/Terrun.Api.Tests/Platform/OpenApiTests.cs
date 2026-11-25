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
}
