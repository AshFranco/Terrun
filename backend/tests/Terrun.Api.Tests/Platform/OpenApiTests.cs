using System.Net;
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
}
