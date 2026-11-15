using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Terrun.Api.Tests.Persistence;
using Terrun.Domain.Identity;

namespace Terrun.Api.Tests.Identity;

// CTZ-14 (RF-01, RF-12): registration criteria plus the evidence Law 172-13 asks for
[Collection(MigratedDatabaseCollection.Name)]
public sealed class RegistrationTests(MigratedDatabaseFixture database) : IAsyncLifetime
{
    private AuthTestApp _app = null!;

    public ValueTask InitializeAsync()
    {
        _app = new AuthTestApp(database);
        return ValueTask.CompletedTask;
    }

    public ValueTask DisposeAsync() => _app.DisposeAsync();

    [Fact]
    public async Task Register_ValidRequest_SignsInAndCreatesTheWholeAccount()
    {
        var email = AuthTestApp.NewEmail();

        var response = await _app.RegisterAsync(email);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(AuthTestApp.Ct);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.False(string.IsNullOrEmpty(body.GetProperty("accessToken").GetString()));
        Assert.False(string.IsNullOrEmpty(body.GetProperty("refreshToken").GetString()));
        Assert.False(body.GetProperty("user").GetProperty("isEmailVerified").GetBoolean());
        Assert.Equal("runner", body.GetProperty("user").GetProperty("role").GetString());

        var userId = Guid.Parse(body.GetProperty("user").GetProperty("id").GetString()!);
        var stored = await _app.QueryAsync(async db => new
        {
            Hash = await db.Users.Where(u => u.Id == userId).Select(u => u.PasswordHash).SingleAsync(AuthTestApp.Ct),
            HasProfile = await db.Profiles.AnyAsync(p => p.UserId == userId, AuthTestApp.Ct),
            HasPreferences = await db.UserPreferences.AnyAsync(p => p.UserId == userId, AuthTestApp.Ct),
            HasStats = await db.UserStats.AnyAsync(s => s.UserId == userId, AuthTestApp.Ct),
            Consents = await db.Consents.Where(c => c.UserId == userId).ToListAsync(AuthTestApp.Ct),
            Audited = await db.AuditLogs.AnyAsync(a => a.UserId == userId && a.Action == "user_registered", AuthTestApp.Ct),
        });
        Assert.StartsWith("pbkdf2-sha512$210000$", stored.Hash, StringComparison.Ordinal);
        Assert.True(stored.HasProfile && stored.HasPreferences && stored.HasStats && stored.Audited);
        Assert.Equal(2, stored.Consents.Count);
        Assert.All(stored.Consents, c => Assert.Equal((ConsentMethod.Registration, "0.1.0"), (c.Method, c.AppVersion)));
    }

    [Fact]
    public async Task Register_ValidRequest_EmailsASixDigitCode()
    {
        var email = AuthTestApp.NewEmail();

        await _app.RegisterOkAsync(email);

        Assert.Matches("^[0-9]{6}$", _app.LastCodeFor(email));
    }

    [Fact]
    public async Task Register_EmailAlreadyRegisteredInOtherCase_Returns409()
    {
        var email = AuthTestApp.NewEmail();
        await _app.RegisterOkAsync(email);

        var response = await _app.RegisterAsync(email.ToUpperInvariant());

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("EMAIL_ALREADY_REGISTERED", await AuthTestApp.CodeOfAsync(response));
    }

    [Fact]
    public async Task Register_HandleTaken_Returns409()
    {
        var handle = AuthTestApp.NewHandle();
        (await _app.RegisterAsync(AuthTestApp.NewEmail(), handle)).EnsureSuccessStatusCode();

        var response = await _app.RegisterAsync(AuthTestApp.NewEmail(), "@" + handle.ToUpperInvariant());

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("HANDLE_TAKEN", await AuthTestApp.CodeOfAsync(response));
    }

    [Theory]
    [InlineData("Corta-1")]
    [InlineData("1234567890")]
    [InlineData("qwertyuiop")]
    public async Task Register_WeakPassword_Returns400OnThePasswordField(string password)
    {
        var response = await _app.RegisterAsync(AuthTestApp.NewEmail(), password: password);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(AuthTestApp.Ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("VALIDATION_ERROR", body.GetProperty("code").GetString());
        Assert.True(body.GetProperty("errors").TryGetProperty("password", out _));
    }

    [Fact]
    public async Task Register_OnlyTermsAccepted_Returns400ConsentRequired()
    {
        var termsOnly = new[] { new Guid("6f1c2d0e-7a4b-4c5d-9e8f-0a1b2c3d4e01") };

        var response = await _app.RegisterAsync(AuthTestApp.NewEmail(), legalIds: termsOnly);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("CONSENT_REQUIRED", await AuthTestApp.CodeOfAsync(response));
    }

    [Theory]
    [InlineData("ab")]
    [InlineData(".runner")]
    [InlineData("runner name")]
    public async Task Register_InvalidHandle_Returns400(string handle)
    {
        var response = await _app.RegisterAsync(AuthTestApp.NewEmail(), handle);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("VALIDATION_ERROR", await AuthTestApp.CodeOfAsync(response));
    }
}
