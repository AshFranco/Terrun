using System.Net;
using Terrun.Api.Modules.Platform;
using Terrun.Api.Tests.Infrastructure;

namespace Terrun.Api.Tests.Platform;

public sealed class HealthEndpointTests(PostgisFixture postgis) : IClassFixture<PostgisFixture>
{
    [Fact]
    public async Task GetHealth_DatabaseAvailable_ReturnsHealthy()
    {
        await using var factory = new TerrunApiFactory(postgis.ConnectionString);
        using var client = factory.CreateClient();

        var response = await client.GetAsync(PlatformModule.HealthPath, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
    }
}
