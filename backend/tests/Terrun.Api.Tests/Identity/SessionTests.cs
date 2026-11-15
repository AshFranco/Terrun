using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Terrun.Api.Tests.Persistence;
using Terrun.Domain.Identity;

namespace Terrun.Api.Tests.Identity;

// CTZ-14 (RF-02) and LLD §7.1: login, rotation, family revocation, logout and stale access tokens
[Collection(MigratedDatabaseCollection.Name)]
public sealed class SessionTests(MigratedDatabaseFixture database) : IAsyncLifetime
{
    private AuthTestApp _app = null!;

    public ValueTask InitializeAsync()
    {
        _app = new AuthTestApp(database);
        return ValueTask.CompletedTask;
    }

    public ValueTask DisposeAsync() => _app.DisposeAsync();

    [Fact]
    public async Task Login_RightPassword_ReturnsTokensAndAuditsTheSuccess()
    {
        var email = AuthTestApp.NewEmail();
        await _app.RegisterOkAsync(email);

        var response = await _app.LoginAsync(email.ToUpperInvariant());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(await CountAuditAsync(email, "login_succeeded") == 1);
    }

    [Fact]
    public async Task Login_WrongPassword_Returns401InvalidCredentialsAndAuditsTheFailure()
    {
        var email = AuthTestApp.NewEmail();
        await _app.RegisterOkAsync(email);

        var response = await _app.LoginAsync(email, "Not-The-Password-2026");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("INVALID_CREDENTIALS", await AuthTestApp.CodeOfAsync(response));
        Assert.True(await CountAuditAsync(email, "login_failed") == 1);
    }

    [Fact]
    public async Task Login_UnknownEmail_Returns401InvalidCredentials()
    {
        var response = await _app.LoginAsync(AuthTestApp.NewEmail());

        Assert.Equal("INVALID_CREDENTIALS", await AuthTestApp.CodeOfAsync(response));
    }

    [Fact]
    public async Task Login_SuspendedAccount_Returns403UserSuspended()
    {
        var email = AuthTestApp.NewEmail();
        await _app.RegisterOkAsync(email);
        await SetStatusAsync(email, UserStatus.Suspended);

        var response = await _app.LoginAsync(email);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal("USER_SUSPENDED", await AuthTestApp.CodeOfAsync(response));
    }

    [Fact]
    public async Task Refresh_ValidToken_ReturnsANewPairThatWorks()
    {
        var session = await _app.RegisterOkAsync(AuthTestApp.NewEmail());

        var first = await _app.RefreshAsync(Token(session, "refreshToken"));
        var rotated = await first.Content.ReadFromJsonAsync<JsonElement>(AuthTestApp.Ct);
        var second = await _app.RefreshAsync(Token(rotated, "refreshToken"));

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.NotEqual(Token(session, "refreshToken"), Token(rotated, "refreshToken"));
        Assert.Equal(HttpStatusCode.OK, second.StatusCode);
    }

    [Fact]
    public async Task Refresh_ReusedRotatedToken_RevokesTheWholeFamily()
    {
        var email = AuthTestApp.NewEmail();
        var session = await _app.RegisterOkAsync(email);
        var stolen = Token(session, "refreshToken");
        var rotated = await (await _app.RefreshAsync(stolen)).Content.ReadFromJsonAsync<JsonElement>(AuthTestApp.Ct);

        var reuse = await _app.RefreshAsync(stolen);
        var legitimateAfterReuse = await _app.RefreshAsync(Token(rotated, "refreshToken"));

        Assert.Equal(HttpStatusCode.Unauthorized, reuse.StatusCode);
        Assert.Equal("INVALID_TOKEN", await AuthTestApp.CodeOfAsync(reuse));
        Assert.Equal(HttpStatusCode.Unauthorized, legitimateAfterReuse.StatusCode);
        Assert.True(await CountAuditAsync(email, "refresh_token_reused") == 1);
    }

    [Fact]
    public async Task Logout_ThenRefreshWithThatToken_Fails_WithoutRaisingTheTheftAlarm()
    {
        var email = AuthTestApp.NewEmail();
        var session = await _app.RegisterOkAsync(email);

        var logout = await _app.LogoutAsync(Token(session, "accessToken"), Token(session, "refreshToken"));
        var refresh = await _app.RefreshAsync(Token(session, "refreshToken"));

        Assert.Equal(HttpStatusCode.NoContent, logout.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, refresh.StatusCode);
        Assert.True(await CountAuditAsync(email, "refresh_token_reused") == 0);
    }

    [Fact]
    public async Task Logout_WithoutAccessToken_Returns401InvalidTokenProblem()
    {
        var response = await _app.Client.PostAsJsonAsync("/api/v1/auth/logout", new { refreshToken = "x" }, AuthTestApp.Ct);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("INVALID_TOKEN", await AuthTestApp.CodeOfAsync(response));
    }

    [Fact]
    public async Task AccessToken_AfterTheSecurityVersionChanges_IsRejected()
    {
        var email = AuthTestApp.NewEmail();
        var session = await _app.RegisterOkAsync(email);
        await _app.QueryAsync(db => db.Users.Where(u => u.Email == email)
            .ExecuteUpdateAsync(set => set.SetProperty(u => u.SecurityVersion, u => u.SecurityVersion + 1), AuthTestApp.Ct));

        var response = await _app.LogoutAsync(Token(session, "accessToken"), Token(session, "refreshToken"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AccessToken_OfASuspendedAccount_IsRejected()
    {
        var email = AuthTestApp.NewEmail();
        var session = await _app.RegisterOkAsync(email);
        await SetStatusAsync(email, UserStatus.Suspended);

        var response = await _app.LogoutAsync(Token(session, "accessToken"), Token(session, "refreshToken"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private static string Token(JsonElement session, string name) => session.GetProperty(name).GetString()!;

    private Task<int> SetStatusAsync(string email, UserStatus status) =>
        _app.QueryAsync(db => db.Users.Where(u => u.Email == email)
            .ExecuteUpdateAsync(set => set.SetProperty(u => u.Status, status), AuthTestApp.Ct));

    private Task<int> CountAuditAsync(string email, string action) =>
        _app.QueryAsync(db => db.AuditLogs.CountAsync(
            a => a.Action == action && db.Users.Any(u => u.Id == a.UserId && u.Email == email), AuthTestApp.Ct));
}
