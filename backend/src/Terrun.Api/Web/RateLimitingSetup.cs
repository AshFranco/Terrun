using System.Globalization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.JsonWebTokens;
using Terrun.Api.Modules.Platform;

namespace Terrun.Api.Web;

// LLD §3.1: 100 requests/min per user (or per IP when anonymous) and 10/min per IP on /auth/*. Over the limit: 429 with
// Retry-After. These are technical limits, not game rules, so they live in configuration ("RateLimiting")
internal static class RateLimitingSetup
{
    public const string AuthPolicy = "auth";
    private const string _sectionName = "RateLimiting";
    private static readonly TimeSpan _window = TimeSpan.FromMinutes(1);

    public static IServiceCollection AddApiRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        var limits = configuration.GetSection(_sectionName).Get<RateLimitingOptions>() ?? new RateLimitingOptions();

        return services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = WriteRejectionAsync;
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(http =>
                IsExempt(http)
                    ? RateLimitPartition.GetNoLimiter("exempt")
                    : FixedWindow(CallerKey(http), limits.PerCallerPerMinute));
            options.AddPolicy(AuthPolicy, http => FixedWindow($"auth:{ClientIp(http)}", limits.AuthPerIpPerMinute));
        });
    }

    private static RateLimitPartition<string> FixedWindow(string key, int permitsPerMinute) =>
        RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = permitsPerMinute,
            Window = _window,
            QueueLimit = 0,
        });

    // The health check is polled by Render and by uptime monitors; limiting it would mark a healthy API as down
    private static bool IsExempt(HttpContext http) => http.Request.Path.StartsWithSegments(PlatformModule.HealthPath);

    private static string CallerKey(HttpContext http) =>
        http.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value is { } userId ? $"user:{userId}" : $"ip:{ClientIp(http)}";

    // Behind Render the forwarded-headers middleware has already replaced RemoteIpAddress with the client address
    private static string ClientIp(HttpContext http) => http.Connection.RemoteIpAddress?.ToString() ?? "unknown";

    private static async ValueTask WriteRejectionAsync(OnRejectedContext context, CancellationToken cancellationToken)
    {
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            context.HttpContext.Response.Headers.RetryAfter =
                ((int)Math.Ceiling(retryAfter.TotalSeconds)).ToString(CultureInfo.InvariantCulture);
        }

        var problemDetails = context.HttpContext.RequestServices.GetRequiredService<IProblemDetailsService>();
        await problemDetails.WriteAsync(new ProblemDetailsContext
        {
            HttpContext = context.HttpContext,
            ProblemDetails = { Status = StatusCodes.Status429TooManyRequests },
        });
    }
}

internal sealed class RateLimitingOptions
{
    public int PerCallerPerMinute { get; init; } = 100;

    public int AuthPerIpPerMinute { get; init; } = 10;
}
