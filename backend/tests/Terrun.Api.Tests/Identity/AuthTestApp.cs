using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Terrun.Api.Tests.Infrastructure;
using Terrun.Api.Tests.Persistence;
using Terrun.Infrastructure.Email;
using Terrun.Infrastructure.Persistence;

namespace Terrun.Api.Tests.Identity;

// The API on the migrated test database, with a mail box that keeps every email so the tests can read the code
public sealed partial class AuthTestApp : IAsyncDisposable
{
    public const string StrongPassword = "Corre-Piantini-2026!";

    private readonly TerrunApiFactory _factory;

    public AuthTestApp(MigratedDatabaseFixture database)
    {
        _factory = new TerrunApiFactory(database.ConnectionString)
        {
            Settings = new() { ["RateLimiting:AuthPerIpPerMinute"] = "10000" },
            TestServices = services =>
            {
                services.AddSingleton(Mailbox);
                services.AddScoped<IEmailSender>(provider => provider.GetRequiredService<CapturingEmailSender>());
                services.AddSingleton<TimeProvider>(Clock);
            },
        };
        Client = _factory.CreateClient();
    }

    public HttpClient Client { get; }

    public CapturingEmailSender Mailbox { get; } = new();

    public MovableClock Clock { get; } = new(DateTimeOffset.UtcNow);

    public static CancellationToken Ct => TestContext.Current.CancellationToken;

    public static string NewEmail() => $"runner-{Guid.NewGuid():N}@example.test";

    public static string NewHandle() => $"r{Guid.NewGuid():N}"[..20];

    public async Task<Guid[]> CurrentLegalDocumentIdsAsync()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<TerrunDbContext>().LegalDocuments.Select(d => d.Id).ToArrayAsync(Ct);
    }

    public async Task<HttpResponseMessage> RegisterAsync(string email, string? handle = null, string password = StrongPassword, Guid[]? legalIds = null) =>
        await Client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email,
            password,
            displayName = "Runner Test",
            handle = handle ?? NewHandle(),
            acceptedLegalDocumentIds = legalIds ?? await CurrentLegalDocumentIdsAsync(),
            appVersion = "0.1.0",
            device = "Test device",
        }, Ct);

    public async Task<JsonElement> RegisterOkAsync(string email)
    {
        var response = await RegisterAsync(email);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<JsonElement>(Ct);
    }

    public Task<HttpResponseMessage> LoginAsync(string email, string password = StrongPassword) =>
        Client.PostAsJsonAsync("/api/v1/auth/login", new { email, password }, Ct);

    public Task<HttpResponseMessage> RefreshAsync(string refreshToken) =>
        Client.PostAsJsonAsync("/api/v1/auth/refresh", new { refreshToken }, Ct);

    public Task<HttpResponseMessage> LogoutAsync(string accessToken, string refreshToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/logout") { Content = JsonContent.Create(new { refreshToken }) };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return Client.SendAsync(request, Ct);
    }

    public Task<HttpResponseMessage> VerifyAsync(string email, string code) =>
        Client.PostAsJsonAsync("/api/v1/auth/verify-email", new { email, code }, Ct);

    public string LastCodeFor(string email) =>
        SixDigits().Match(Mailbox.Messages.Last(m => m.To == email).TextBody).Value;

    public async Task<T> QueryAsync<T>(Func<TerrunDbContext, Task<T>> query)
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        return await query(scope.ServiceProvider.GetRequiredService<TerrunDbContext>());
    }

    public static async Task<string?> CodeOfAsync(HttpResponseMessage response) =>
        (await response.Content.ReadFromJsonAsync<JsonElement>(Ct)).GetProperty("code").GetString();

    public async ValueTask DisposeAsync()
    {
        Client.Dispose();
        await _factory.DisposeAsync();
    }

    [GeneratedRegex(@"\b\d{6}\b")]
    private static partial Regex SixDigits();
}

public sealed class CapturingEmailSender : IEmailSender
{
    private readonly ConcurrentQueue<EmailMessage> _messages = new();

    public IReadOnlyCollection<EmailMessage> Messages => _messages;

    public Task SendAsync(EmailMessage message, CancellationToken cancellationToken)
    {
        _messages.Enqueue(message);
        return Task.CompletedTask;
    }
}

public sealed class MovableClock(DateTimeOffset start) : TimeProvider
{
    private DateTimeOffset _now = start;

    public override DateTimeOffset GetUtcNow() => _now;

    public void Advance(TimeSpan by) => _now = _now.Add(by);
}
