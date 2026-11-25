using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Terrun.Api.Tests.Identity;
using Terrun.Api.Tests.Persistence;

namespace Terrun.Api.Tests.Profiles;

// CTZ-16 (RF-07, D-32): GET /me and PATCH /me/profile with JSON merge patch semantics
[Collection(MigratedDatabaseCollection.Name)]
public sealed class MeTests(MigratedDatabaseFixture database) : IAsyncLifetime
{
    private AuthTestApp _app = null!;

    public ValueTask InitializeAsync()
    {
        _app = new AuthTestApp(database);
        return ValueTask.CompletedTask;
    }

    public ValueTask DisposeAsync() => _app.DisposeAsync();

    [Fact]
    public async Task GetMe_NewRunner_ReturnsProfileDefaultPreferencesAndEmptyStats()
    {
        var email = AuthTestApp.NewEmail();
        var token = await RegisterAsync(email);

        var response = await SendAsync(HttpMethod.Get, "/api/v1/me", token);
        var me = await response.Content.ReadFromJsonAsync<JsonElement>(AuthTestApp.Ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(email, me.GetProperty("user").GetProperty("email").GetString());
        Assert.False(me.GetProperty("user").GetProperty("isEmailVerified").GetBoolean());
        Assert.Equal("runner", me.GetProperty("user").GetProperty("role").GetString());
        Assert.Equal("Runner Test", me.GetProperty("profile").GetProperty("displayName").GetString());
        Assert.Equal("undisclosed", me.GetProperty("profile").GetProperty("sex").GetString());
        Assert.Equal(JsonValueKind.Null, me.GetProperty("profile").GetProperty("photoUrl").ValueKind);
        var preferences = me.GetProperty("preferences");
        Assert.Equal("metric", preferences.GetProperty("units").GetString());
        Assert.Equal("followers", preferences.GetProperty("defaultVisibility").GetString());
        Assert.True(preferences.GetProperty("isAutoPauseEnabled").GetBoolean());
        Assert.True(preferences.GetProperty("isVoiceCuesEnabled").GetBoolean());
        Assert.Equal(1m, preferences.GetProperty("voiceCueIntervalKm").GetDecimal());
        Assert.True(preferences.GetProperty("notifyZoneStolen").GetBoolean());
        Assert.Equal(0, me.GetProperty("stats").GetProperty("totalDistanceM").GetInt64());
        Assert.Equal(1, me.GetProperty("stats").GetProperty("level").GetInt16());
    }

    [Fact]
    public async Task GetMe_WithoutToken_Returns401()
    {
        var response = await _app.Client.GetAsync("/api/v1/me", AuthTestApp.Ct);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PatchProfile_SomeFields_UpdatesThemAndKeepsTheRest()
    {
        var token = await RegisterAsync(AuthTestApp.NewEmail());
        await PatchAsync(token, """{ "bio": "Corro por Piantini", "city": "Santo Domingo" }""");

        var response = await PatchAsync(token, """
            { "displayName": "  Ashley F  ", "country": "do", "birthDate": "1999-05-10", "sex": "female",
              "weightKg": 61.5, "heightCm": 165, "isPrivate": true }
            """);
        var profile = (await response.Content.ReadFromJsonAsync<JsonElement>(AuthTestApp.Ct)).GetProperty("profile");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Ashley F", profile.GetProperty("displayName").GetString());
        Assert.Equal("Corro por Piantini", profile.GetProperty("bio").GetString());
        Assert.Equal("Santo Domingo", profile.GetProperty("city").GetString());
        Assert.Equal("DO", profile.GetProperty("country").GetString());
        Assert.Equal("1999-05-10", profile.GetProperty("birthDate").GetString());
        Assert.Equal("female", profile.GetProperty("sex").GetString());
        Assert.Equal(61.5m, profile.GetProperty("weightKg").GetDecimal());
        Assert.Equal(165, profile.GetProperty("heightCm").GetInt16());
        Assert.True(profile.GetProperty("isPrivate").GetBoolean());
    }

    [Fact]
    public async Task PatchProfile_ExplicitNullOrBlank_ClearsOptionalFields()
    {
        var token = await RegisterAsync(AuthTestApp.NewEmail());
        await PatchAsync(token, """{ "city": "Santo Domingo", "weightKg": 70, "bio": "Hola" }""");

        var response = await PatchAsync(token, """{ "city": null, "weightKg": null, "bio": "   " }""");
        var profile = (await response.Content.ReadFromJsonAsync<JsonElement>(AuthTestApp.Ct)).GetProperty("profile");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(JsonValueKind.Null, profile.GetProperty("city").ValueKind);
        Assert.Equal(JsonValueKind.Null, profile.GetProperty("weightKg").ValueKind);
        Assert.Equal(JsonValueKind.Null, profile.GetProperty("bio").ValueKind);
    }

    [Theory]
    [InlineData("""{ "displayName": null }""", "displayName")]
    [InlineData("""{ "displayName": "   " }""", "displayName")]
    [InlineData("""{ "handle": null }""", "handle")]
    [InlineData("""{ "handle": "ab" }""", "handle")]
    [InlineData("""{ "sex": null }""", "sex")]
    [InlineData("""{ "isPrivate": null }""", "isPrivate")]
    [InlineData("""{ "weightKg": 24.9 }""", "weightKg")]
    [InlineData("""{ "weightKg": 70.125 }""", "weightKg")]
    [InlineData("""{ "heightCm": 99 }""", "heightCm")]
    [InlineData("""{ "country": "DOM" }""", "country")]
    [InlineData("""{ "birthDate": "1899-12-31" }""", "birthDate")]
    public async Task PatchProfile_InvalidField_Returns400ValidationErrorForThatField(string body, string field)
    {
        var token = await RegisterAsync(AuthTestApp.NewEmail());

        var response = await PatchAsync(token, body);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>(AuthTestApp.Ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("VALIDATION_ERROR", problem.GetProperty("code").GetString());
        Assert.True(problem.GetProperty("errors").TryGetProperty(field, out _));
    }

    [Fact]
    public async Task PatchProfile_UnknownSex_Returns400ValidationError()
    {
        var token = await RegisterAsync(AuthTestApp.NewEmail());

        var response = await PatchAsync(token, """{ "sex": "robot" }""");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("VALIDATION_ERROR", await AuthTestApp.CodeOfAsync(response));
    }

    // D-28: adults only. The clock of the test app decides "today"
    [Fact]
    public async Task PatchProfile_BirthDateUnder18_Returns400AndExactly18IsAccepted()
    {
        var token = await RegisterAsync(AuthTestApp.NewEmail());
        var today = DateOnly.FromDateTime(_app.Clock.GetUtcNow().UtcDateTime);

        var tooYoung = await PatchAsync(token, $$"""{ "birthDate": "{{today.AddYears(-18).AddDays(1):yyyy-MM-dd}}" }""");
        var justAdult = await PatchAsync(token, $$"""{ "birthDate": "{{today.AddYears(-18):yyyy-MM-dd}}" }""");

        Assert.Equal(HttpStatusCode.BadRequest, tooYoung.StatusCode);
        Assert.Equal(HttpStatusCode.OK, justAdult.StatusCode);
    }

    [Fact]
    public async Task PatchProfile_NewHandle_NormalizesSavesAndAuditsTheChange()
    {
        var email = AuthTestApp.NewEmail();
        var token = await RegisterAsync(email);
        var handle = AuthTestApp.NewHandle();

        var response = await PatchAsync(token, $$"""{ "handle": "@{{handle.ToUpperInvariant()}}" }""");
        var profile = (await response.Content.ReadFromJsonAsync<JsonElement>(AuthTestApp.Ct)).GetProperty("profile");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(handle, profile.GetProperty("handle").GetString());
        Assert.Equal(1, await CountHandleChangesAsync(email));
    }

    [Fact]
    public async Task PatchProfile_SameHandleInOtherCase_DoesNotAuditAChange()
    {
        var email = AuthTestApp.NewEmail();
        var handle = AuthTestApp.NewHandle();
        var token = await RegisterAsync(email, handle);

        var response = await PatchAsync(token, $$"""{ "handle": "{{handle.ToUpperInvariant()}}" }""");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(0, await CountHandleChangesAsync(email));
    }

    [Fact]
    public async Task PatchProfile_HandleOfAnotherRunner_Returns409HandleTaken()
    {
        var takenHandle = AuthTestApp.NewHandle();
        await RegisterAsync(AuthTestApp.NewEmail(), takenHandle);
        var token = await RegisterAsync(AuthTestApp.NewEmail());

        var response = await PatchAsync(token, $$"""{ "handle": "{{takenHandle}}" }""");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("HANDLE_TAKEN", await AuthTestApp.CodeOfAsync(response));
    }

    // CTZ-24: the changes are still there when the app opens again (a new session reads them with GET /me)
    [Fact]
    public async Task PatchProfile_ThenLoginAgain_GetMeReturnsTheChanges()
    {
        var email = AuthTestApp.NewEmail();
        var token = await RegisterAsync(email);
        await PatchAsync(token, """{ "displayName": "Nuevo Nombre", "city": "Santiago", "weightKg": 68 }""");

        var login = await (await _app.LoginAsync(email)).Content.ReadFromJsonAsync<JsonElement>(AuthTestApp.Ct);
        var me = await (await SendAsync(HttpMethod.Get, "/api/v1/me", login.GetProperty("accessToken").GetString()!))
            .Content.ReadFromJsonAsync<JsonElement>(AuthTestApp.Ct);

        Assert.Equal("Nuevo Nombre", me.GetProperty("profile").GetProperty("displayName").GetString());
        Assert.Equal("Santiago", me.GetProperty("profile").GetProperty("city").GetString());
        Assert.Equal(68m, me.GetProperty("profile").GetProperty("weightKg").GetDecimal());
    }

    [Fact]
    public async Task PatchProfile_PhotoUrlInBody_IsIgnored()
    {
        var token = await RegisterAsync(AuthTestApp.NewEmail());

        var response = await PatchAsync(token, """{ "photoUrl": "https://tracker.example/pixel.png" }""");
        var profile = (await response.Content.ReadFromJsonAsync<JsonElement>(AuthTestApp.Ct)).GetProperty("profile");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(JsonValueKind.Null, profile.GetProperty("photoUrl").ValueKind);
    }

    private async Task<string> RegisterAsync(string email, string? handle = null)
    {
        var response = await _app.RegisterAsync(email, handle);
        response.EnsureSuccessStatusCode();
        var session = await response.Content.ReadFromJsonAsync<JsonElement>(AuthTestApp.Ct);
        return session.GetProperty("accessToken").GetString()!;
    }

    private Task<HttpResponseMessage> PatchAsync(string token, string json) =>
        SendAsync(HttpMethod.Patch, "/api/v1/me/profile", token, json);

    private Task<HttpResponseMessage> SendAsync(HttpMethod method, string path, string token, string? json = null)
    {
        var request = new HttpRequestMessage(method, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        if (json is not null)
        {
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        return _app.Client.SendAsync(request, AuthTestApp.Ct);
    }

    private Task<int> CountHandleChangesAsync(string email) =>
        _app.QueryAsync(db =>
            (from log in db.AuditLogs
             join user in db.Users on log.UserId equals user.Id
             where user.Email == email && log.Action == "handle_changed"
             select log).CountAsync(AuthTestApp.Ct));
}
