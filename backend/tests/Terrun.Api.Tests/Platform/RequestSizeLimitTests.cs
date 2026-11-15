using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Terrun.Api.Tests.Infrastructure;

namespace Terrun.Api.Tests.Platform;

// LLD §7.3: the body size limit is enforced by Kestrel, which the in-memory test server skips, so this runs on real Kestrel
public sealed class RequestSizeLimitTests
{
    [Fact]
    public async Task PostValidated_BodyOverTheLimit_ReturnsPayloadTooLarge()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var factory = new TerrunApiFactory(TerrunApiFactory.UnreachableDatabase)
        {
            Settings = new() { ["Limits:MaxRequestBodyBytes"] = "1024" },
            TestServices = services =>
            {
                services.AddControllers().AddApplicationPart(typeof(ProbeController).Assembly);
                services.AddScoped<IValidator<ProbeRequest>, ProbeRequestValidator>();
            },
        };
        factory.UseKestrel(0);
        factory.StartServer();
        using var client = factory.CreateClient();
        var oversized = JsonSerializer.Serialize(new { email = new string('a', 4096) + "@example.test", distanceM = 1 });
        using var content = new StringContent(oversized, Encoding.UTF8, "application/json");

        var response = await client.PostAsync("/api/v1/probe/validated", content, ct);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>(ct);

        Assert.Equal(HttpStatusCode.RequestEntityTooLarge, response.StatusCode);
        Assert.Equal("PAYLOAD_TOO_LARGE", problem.GetProperty("code").GetString());
    }
}
