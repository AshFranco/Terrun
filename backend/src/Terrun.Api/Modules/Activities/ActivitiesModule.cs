using Terrun.Api.Modules.Activities.Services;

namespace Terrun.Api.Modules.Activities;

public static class ActivitiesModule
{
    public static IServiceCollection AddActivitiesModule(this IServiceCollection services)
    {
        services.AddScoped<ActivityUploadService>();

        return services;
    }
}
