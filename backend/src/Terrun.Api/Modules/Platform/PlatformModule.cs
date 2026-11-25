using Terrun.Api.Modules.Platform.Services;
using Terrun.Infrastructure.Persistence;

namespace Terrun.Api.Modules.Platform;

public static class PlatformModule
{
    public const string HealthPath = "/health";

    public static IServiceCollection AddPlatformModule(this IServiceCollection services)
    {
        // LLD §8: /health checks the API and the database; Render uses it as its health check
        services.AddHealthChecks().AddDbContextCheck<TerrunDbContext>("database");
        services.AddScoped<SystemSettingsReader>();

        return services;
    }

    public static IEndpointRouteBuilder MapPlatformModule(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHealthChecks(HealthPath).AllowAnonymous();

        return endpoints;
    }
}
