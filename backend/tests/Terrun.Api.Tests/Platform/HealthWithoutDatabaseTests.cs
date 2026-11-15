using System.Net;
using Terrun.Api.Modules.Platform;
using Terrun.Api.Tests.Infrastructure;

namespace Terrun.Api.Tests.Platform;

public sealed class HealthWithoutDatabaseTests
{
    [Fact]
    public async Task GetHealth_DatabaseUnreachable_ReturnsServiceUnavailable()
    {
        await using var factory = new TerrunApiFactory(TerrunApiFactory.UnreachableDatabase);
        using var client = factory.CreateClient();

        var response = await client.GetAsync(PlatformModule.HealthPath, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.Equal("Unhealthy", await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
    }
}
