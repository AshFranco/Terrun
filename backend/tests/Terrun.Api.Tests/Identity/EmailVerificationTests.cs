using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Terrun.Api.Tests.Persistence;

namespace Terrun.Api.Tests.Identity;

// D-26: the 6-digit code typed in P-04
[Collection(MigratedDatabaseCollection.Name)]
public sealed class EmailVerificationTests(MigratedDatabaseFixture database) : IAsyncLifetime
{
    private AuthTestApp _app = null!;

    public ValueTask InitializeAsync()
    {
        _app = new AuthTestApp(database);
        return ValueTask.CompletedTask;
    }

    public ValueTask DisposeAsync() => _app.DisposeAsync();

    [Fact]
    public async Task VerifyEmail_RightCode_MarksTheEmailAsVerified()
    {
        var email = AuthTestApp.NewEmail();
        await _app.RegisterOkAsync(email);

        var response = await _app.VerifyAsync(email, _app.LastCodeFor(email));

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.True(await _app.QueryAsync(db => db.Users.Where(u => u.Email == email).Select(u => u.IsEmailVerified).SingleAsync(AuthTestApp.Ct)));
    }

    [Fact]
    public async Task VerifyEmail_FiveWrongCodes_ThenEvenTheRightCodeFails()
    {
        var email = AuthTestApp.NewEmail();
        await _app.RegisterOkAsync(email);
        var rightCode = _app.LastCodeFor(email);
        var wrongCode = rightCode == "000000" ? "111111" : "000000";

        for (var attempt = 0; attempt < 5; attempt++)
        {
            Assert.Equal("VERIFICATION_CODE_INVALID", await AuthTestApp.CodeOfAsync(await _app.VerifyAsync(email, wrongCode)));
        }

        var response = await _app.VerifyAsync(email, rightCode);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("VERIFICATION_CODE_INVALID", await AuthTestApp.CodeOfAsync(response));
    }

    [Fact]
    public async Task VerifyEmail_After24Hours_IsRejected()
    {
        var email = AuthTestApp.NewEmail();
        await _app.RegisterOkAsync(email);
        _app.Clock.Advance(TimeSpan.FromHours(24) + TimeSpan.FromMinutes(1));

        var response = await _app.VerifyAsync(email, _app.LastCodeFor(email));

        Assert.Equal("VERIFICATION_CODE_INVALID", await AuthTestApp.CodeOfAsync(response));
    }

    [Fact]
    public async Task ResendVerification_UnknownEmail_Returns202AndSendsNothing()
    {
        var before = _app.Mailbox.Messages.Count;

        var response = await _app.Client.PostAsJsonAsync("/api/v1/auth/resend-verification", new { email = AuthTestApp.NewEmail() }, AuthTestApp.Ct);

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        Assert.Equal(before, _app.Mailbox.Messages.Count);
    }

    [Fact]
    public async Task ResendVerification_AfterTheCooldown_ReplacesTheOldCode()
    {
        var email = AuthTestApp.NewEmail();
        await _app.RegisterOkAsync(email);
        var firstCode = _app.LastCodeFor(email);
        var resend = () => _app.Client.PostAsJsonAsync("/api/v1/auth/resend-verification", new { email }, AuthTestApp.Ct);

        await resend();
        var sentDuringCooldown = _app.Mailbox.Messages.Count(m => m.To == email);
        _app.Clock.Advance(TimeSpan.FromSeconds(61));
        await resend();
        var secondCode = _app.LastCodeFor(email);

        Assert.Equal(1, sentDuringCooldown);
        Assert.Equal(2, _app.Mailbox.Messages.Count(m => m.To == email));
        if (firstCode != secondCode)
        {
            Assert.Equal("VERIFICATION_CODE_INVALID", await AuthTestApp.CodeOfAsync(await _app.VerifyAsync(email, firstCode)));
        }

        Assert.Equal(HttpStatusCode.NoContent, (await _app.VerifyAsync(email, secondCode)).StatusCode);
    }
}
