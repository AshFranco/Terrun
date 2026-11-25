using Terrun.Api.Jobs;
using Terrun.Api.Modules.Activities.Processing;
using Terrun.Api.Modules.Activities.Services;

namespace Terrun.Api.Modules.Activities;

public static class ActivitiesModule
{
    public static IServiceCollection AddActivitiesModule(this IServiceCollection services)
    {
        services.AddScoped<ActivityUploadService>();

        // LLD §4.1 and §6. The processing stages (IActivityProcessingStep) arrive with CTZ-31a and CTZ-31b; until then the
        // processor takes nothing and uploads stay pending (D-35)
        services.AddSingleton<ActivityQueue>();
        services.AddScoped<ActivityProcessor>();
        services.AddScoped<IScheduledJob, ActivityProcessorJob>();
        services.AddHostedService<ActivityQueueWorker>();

        return services;
    }
}
