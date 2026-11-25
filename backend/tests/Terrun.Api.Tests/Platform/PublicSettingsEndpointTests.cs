using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Terrun.Api.Modules.Platform.Services;
using Terrun.Api.Tests.Identity;
using Terrun.Api.Tests.Persistence;

namespace Terrun.Api.Tests.Platform;

// CTZ-17 (LLD §2.4, §3.2): GET /settings/public exposes only public game parameters through SystemSettingsReader
[Collection(MigratedDatabaseCollection.Name)]
public sealed class PublicSettingsEndpointTests(MigratedDatabaseFixture database) : IAsyncLifetime
{
    private AuthTestApp _app = null!;

    public ValueTask InitializeAsync()
    {
        _app = new AuthTestApp(database);
        return ValueTask.CompletedTask;
    }

    public ValueTask DisposeAsync() => _app.DisposeAsync();

    [Fact]
    public async Task GetPublicSettings_AnonymousCaller_ReturnsTypedPublicParametersWithoutPrivateAntiCheatLimits()
    {
        var response = await _app.Client.GetAsync("/api/v1/settings/public", AuthTestApp.Ct);
        var raw = await response.Content.ReadAsStringAsync(AuthTestApp.Ct);
        var settings = JsonDocument.Parse(raw).RootElement;

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(9, settings.GetProperty("territory").GetProperty("zoneResolution").GetInt32());
        Assert.Equal(5, settings.GetProperty("territory").GetProperty("conquestThreshold").GetInt32());
        Assert.Equal(30000, settings.GetProperty("activity").GetProperty("maxPoints").GetInt32());
        Assert.Equal(24, settings.GetProperty("activity").GetProperty("maxDurationH").GetInt32());
        Assert.Equal(0.5, settings.GetProperty("activity").GetProperty("autoPauseSpeedMps").GetDouble());
        Assert.Equal(100, settings.GetProperty("xp").GetProperty("levelCurve").GetProperty("base").GetInt32());
        Assert.Equal(1.5, settings.GetProperty("xp").GetProperty("levelCurve").GetProperty("exponent").GetDouble());
        Assert.Equal(200, settings.GetProperty("privacy").GetProperty("routeTrimM").GetInt32());
        Assert.DoesNotContain("anticheat", raw, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("maxAccuracyM", raw, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("maxSpeedKmh", raw, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task PublicKeys_ComparedToDatabaseSeed_MatchesEveryIsPublicRow()
    {
        var dbPublicKeys = await _app.QueryAsync(db =>
            db.SystemSettings.AsNoTracking()
                .Where(s => s.IsPublic)
                .Select(s => s.Key)
                .ToListAsync(AuthTestApp.Ct));

        Assert.Equal(
            SystemSettingsReader.PublicKeys.OrderBy(k => k, StringComparer.Ordinal),
            dbPublicKeys.OrderBy(k => k, StringComparer.Ordinal));
    }
}
