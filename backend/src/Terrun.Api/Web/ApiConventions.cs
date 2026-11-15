using System.Text.Json;
using System.Text.Json.Serialization;
using FluentValidation;
using Microsoft.AspNetCore.HttpLogging;

namespace Terrun.Api.Web;

// Cross-cutting rules every endpoint shares, so no module decides them on its own (LLD §3.1, §3.4, §7.3 and §8)
internal static class ApiConventions
{
    public const string RoutePrefix = "api/v1";
    private const string _maxRequestBodyKey = "Limits:MaxRequestBodyBytes";
    private const long _defaultMaxRequestBodyBytes = 2 * 1024 * 1024;
    private const string _swaggerPath = "/swagger";

    public static WebApplicationBuilder AddApiConventions(this WebApplicationBuilder builder)
    {
        var services = builder.Services;

        services.AddControllers(options =>
            {
                options.Conventions.Add(new GlobalRoutePrefixConvention(RoutePrefix));
                options.Filters.Add<FluentValidationFilter>();
            })
            .AddJsonOptions(options => UseSnakeCaseEnums(options.JsonSerializerOptions));
        services.ConfigureHttpJsonOptions(options => UseSnakeCaseEnums(options.SerializerOptions));
        services.AddValidatorsFromAssemblyContaining<Program>(includeInternalTypes: true);

        services.AddProblemDetails(options => options.CustomizeProblemDetails = ProblemDetailsSetup.Customize);
        services.AddExceptionHandler<DomainExceptionHandler>();
        services.AddApiRateLimiting(builder.Configuration);

        // LLD §7.3: 2 MB per request in general (POST /activities raises its own limit to 8 MB)
        var maxRequestBodyBytes = builder.Configuration.GetValue(_maxRequestBodyKey, _defaultMaxRequestBodyBytes);
        builder.WebHost.ConfigureKestrel(kestrel => kestrel.Limits.MaxRequestBodySize = maxRequestBodyBytes);

        // LLD §8 and §7.3: one log line per request with method, path, status and duration; never the query string or
        // the body, which may carry coordinates or tokens
        services.AddHttpLogging(options =>
        {
            options.LoggingFields = HttpLoggingFields.RequestMethod | HttpLoggingFields.RequestPath |
                HttpLoggingFields.ResponseStatusCode | HttpLoggingFields.Duration;
            options.CombineLogs = true;
        });
        if (!builder.Environment.IsDevelopment())
        {
            builder.Logging.ClearProviders();
            builder.Logging.AddJsonConsole(options =>
            {
                options.IncludeScopes = true;
                options.UseUtcTimestamp = true;
                options.TimestampFormat = "O";
            });
        }

        builder.Logging.Configure(options => options.ActivityTrackingOptions =
            ActivityTrackingOptions.TraceId | ActivityTrackingOptions.SpanId);

        return builder;
    }

    public static WebApplication UseApiConventions(this WebApplication app)
    {
        app.UseExceptionHandler();
        app.UseStatusCodePages();
        if (app.Environment.IsProduction())
        {
            app.UseHsts();
        }

        app.Use(AddSecurityHeaders);
        app.UseHttpLogging();

        // Authentication runs before the rate limiter so that the limit counts per user, not per shared IP
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseRateLimiter();

        return app;
    }

    private static void UseSnakeCaseEnums(JsonSerializerOptions options) =>
        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower));

    // LLD §7.3: security headers. The API only returns JSON, so the strict policy is safe everywhere except Swagger UI
    private static Task AddSecurityHeaders(HttpContext context, RequestDelegate next)
    {
        var headers = context.Response.Headers;
        headers.XContentTypeOptions = "nosniff";
        headers.XFrameOptions = "DENY";
        headers["Referrer-Policy"] = "no-referrer";
        if (!context.Request.Path.StartsWithSegments(_swaggerPath))
        {
            headers.ContentSecurityPolicy = "default-src 'none'; frame-ancestors 'none'";
        }

        return next(context);
    }
}
