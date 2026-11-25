using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Terrun.Api.Tests.Persistence;
using Terrun.Domain.Identity;

namespace Terrun.Api.Tests.Identity;

// CTZ-17 (RF-12, Law 172-13, LLD §3.2): GET /legal/current and POST /me/consents
[Collection(MigratedDatabaseCollection.Name)]
public sealed class LegalEndpointsTests(MigratedDatabaseFixture database) : IAsyncLifetime
{
    private static readonly Guid _seededTermsId = new("6f1c2d0e-7a4b-4c5d-9e8f-0a1b2c3d4e01");
    private static readonly Guid _seededPrivacyId = new("6f1c2d0e-7a4b-4c5d-9e8f-0a1b2c3d4e02");

    private AuthTestApp _app = null!;

    public ValueTask InitializeAsync()
    {
        _app = new AuthTestApp(database);
        return ValueTask.CompletedTask;
    }

    public ValueTask DisposeAsync() => _app.DisposeAsync();

    [Fact]
    public async Task GetCurrent_SeededDatabase_ReturnsTermsAndPrivacyVersion1EffectiveNov17()
    {
        var response = await _app.Client.GetAsync("/api/v1/legal/current", AuthTestApp.Ct);
        var documents = await response.Content.ReadFromJsonAsync<JsonElement>(AuthTestApp.Ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(2, documents.GetArrayLength());

        var terms = documents[0];
        Assert.Equal(_seededTermsId, terms.GetProperty("id").GetGuid());
        Assert.Equal("terms", terms.GetProperty("type").GetString());
        Assert.Equal("1.0", terms.GetProperty("version").GetString());
        Assert.EndsWith("terminos-v1.0.md", terms.GetProperty("url").GetString(), StringComparison.Ordinal);
        Assert.Equal(new DateTimeOffset(2026, 11, 17, 0, 0, 0, TimeSpan.Zero), terms.GetProperty("effectiveFrom").GetDateTimeOffset());

        var privacy = documents[1];
        Assert.Equal(_seededPrivacyId, privacy.GetProperty("id").GetGuid());
        Assert.Equal("privacy", privacy.GetProperty("type").GetString());
        Assert.Equal("1.0", privacy.GetProperty("version").GetString());
        Assert.EndsWith("privacidad-v1.0.md", privacy.GetProperty("url").GetString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetCurrent_FutureVersionPublished_ReturnsCurrentUntilEffectiveFromThenSwitches()
    {
        var futureDocId = Guid.CreateVersion7();
        var effectiveAt = _app.Clock.GetUtcNow().AddDays(7);
        await InsertLegalDocumentAsync(futureDocId, LegalDocumentType.Terms, "2.0-future", effectiveAt);

        try
        {
            var beforeEffective = await _app.Client.GetFromJsonAsync<JsonElement>("/api/v1/legal/current", AuthTestApp.Ct);
            _app.Clock.Advance(TimeSpan.FromDays(7));
            var afterEffective = await _app.Client.GetFromJsonAsync<JsonElement>("/api/v1/legal/current", AuthTestApp.Ct);

            Assert.Equal("1.0", beforeEffective[0].GetProperty("version").GetString());
            Assert.Equal(futureDocId, afterEffective[0].GetProperty("id").GetGuid());
            Assert.Equal("2.0-future", afterEffective[0].GetProperty("version").GetString());
        }
        finally
        {
            await DeleteLegalDocumentAsync(futureDocId);
        }
    }

    [Fact]
    public async Task AcceptConsents_WithoutToken_Returns401InvalidToken()
    {
        var response = await _app.Client.PostAsJsonAsync(
            "/api/v1/me/consents",
            new { legalDocumentIds = new[] { _seededTermsId } },
            AuthTestApp.Ct);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("INVALID_TOKEN", await AuthTestApp.CodeOfAsync(response));
    }

    [Fact]
    public async Task AcceptConsents_NewEffectiveVersion_RecordsSettingsConsentWithClockAndAppVersion()
    {
        var session = await _app.RegisterOkAsync(AuthTestApp.NewEmail());
        var token = session.GetProperty("accessToken").GetString()!;
        var userId = session.GetProperty("user").GetProperty("id").GetGuid();
        var newPrivacyId = Guid.CreateVersion7();
        await InsertLegalDocumentAsync(newPrivacyId, LegalDocumentType.Privacy, "1.1-settings", _app.Clock.GetUtcNow());
        _app.Clock.Advance(TimeSpan.FromHours(2));

        try
        {
            var response = await PostConsentsAsync(token, new { legalDocumentIds = new[] { newPrivacyId }, appVersion = "  0.2.0  " });
            var consents = await response.Content.ReadFromJsonAsync<JsonElement>(AuthTestApp.Ct);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var recorded = Assert.Single(consents.EnumerateArray());
            Assert.Equal(newPrivacyId, recorded.GetProperty("legalDocumentId").GetGuid());
            Assert.Equal("privacy", recorded.GetProperty("type").GetString());
            Assert.Equal("1.1-settings", recorded.GetProperty("version").GetString());
            Assert.Equal("settings", recorded.GetProperty("method").GetString());
            Assert.Equal("0.2.0", recorded.GetProperty("appVersion").GetString());
            Assert.True((_app.Clock.GetUtcNow() - recorded.GetProperty("acceptedAt").GetDateTimeOffset()).Duration() < TimeSpan.FromMilliseconds(1));

            var totalForUser = await _app.QueryAsync(db => db.Consents.CountAsync(c => c.UserId == userId, AuthTestApp.Ct));
            Assert.Equal(3, totalForUser);
        }
        finally
        {
            await DeleteLegalDocumentAsync(newPrivacyId);
        }
    }

    [Fact]
    public async Task AcceptConsents_AlreadyAcceptedDocument_IsIdempotentAndPreservesOriginalEvidence()
    {
        var registeredAt = _app.Clock.GetUtcNow();
        var session = await _app.RegisterOkAsync(AuthTestApp.NewEmail());
        var token = session.GetProperty("accessToken").GetString()!;
        _app.Clock.Advance(TimeSpan.FromHours(5));

        var response = await PostConsentsAsync(token, new { legalDocumentIds = new[] { _seededTermsId }, appVersion = "0.9.9" });
        var consents = await response.Content.ReadFromJsonAsync<JsonElement>(AuthTestApp.Ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var preserved = Assert.Single(consents.EnumerateArray());
        Assert.Equal("registration", preserved.GetProperty("method").GetString());
        Assert.Equal("0.1.0", preserved.GetProperty("appVersion").GetString());
        Assert.True((registeredAt - preserved.GetProperty("acceptedAt").GetDateTimeOffset()).Duration() < TimeSpan.FromMilliseconds(1));
    }

    [Fact]
    public async Task AcceptConsents_EmptyOrUnknownOrFutureDocument_Returns400OnLegalDocumentIds()
    {
        var session = await _app.RegisterOkAsync(AuthTestApp.NewEmail());
        var token = session.GetProperty("accessToken").GetString()!;
        var futureDocId = Guid.CreateVersion7();
        await InsertLegalDocumentAsync(futureDocId, LegalDocumentType.Terms, "3.0-not-yet", _app.Clock.GetUtcNow().AddDays(1));

        try
        {
            var payloads = new object[]
            {
                new { legalDocumentIds = Array.Empty<Guid>() },
                new { legalDocumentIds = new[] { Guid.Empty } },
                new { legalDocumentIds = new[] { Guid.NewGuid() } },
                new { legalDocumentIds = new[] { futureDocId } },
            };

            foreach (var payload in payloads)
            {
                var response = await PostConsentsAsync(token, payload);
                var problem = await response.Content.ReadFromJsonAsync<JsonElement>(AuthTestApp.Ct);

                Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
                Assert.Equal("VALIDATION_ERROR", problem.GetProperty("code").GetString());
                Assert.True(problem.GetProperty("errors").TryGetProperty("legalDocumentIds", out _));
            }
        }
        finally
        {
            await DeleteLegalDocumentAsync(futureDocId);
        }
    }

    private Task<HttpResponseMessage> PostConsentsAsync(string token, object body)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/me/consents")
        {
            Content = JsonContent.Create(body),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return _app.Client.SendAsync(request, AuthTestApp.Ct);
    }

    private Task InsertLegalDocumentAsync(Guid id, LegalDocumentType type, string version, DateTimeOffset effectiveFrom) =>
        _app.QueryAsync(async db =>
        {
            db.LegalDocuments.Add(new LegalDocument
            {
                Id = id,
                Type = type,
                Version = version,
                Url = $"https://terrun.test/legal/{version}.md",
                EffectiveFrom = effectiveFrom,
            });
            return await db.SaveChangesAsync(AuthTestApp.Ct);
        });

    private Task DeleteLegalDocumentAsync(Guid id) =>
        _app.QueryAsync(async db =>
        {
            await db.Consents.Where(c => c.LegalDocumentId == id).ExecuteDeleteAsync(AuthTestApp.Ct);
            return await db.LegalDocuments.Where(d => d.Id == id).ExecuteDeleteAsync(AuthTestApp.Ct);
        });
}
