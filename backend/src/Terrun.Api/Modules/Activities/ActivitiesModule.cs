using Terrun.Api.Jobs;
using Terrun.Api.Modules.Activities.Processing;
using Terrun.Api.Modules.Activities.Services;

namespace Terrun.Api.Modules.Activities;

public static class ActivitiesModule
{
    public static IServiceCollection AddActivitiesModule(this IServiceCollection services)
    {
        services.AddScoped<ActivityUploadService>();

        // LLD §4.1 and §6. The cleaning and the metrics go live together, so no activity is processed without its
        // metrics (D-36); territory and progression add their own stages later
        services.AddScoped<IActivityProcessingStep, CleaningStep>();
        services.AddScoped<IActivityProcessingStep, MetricsStep>();
        services.AddScoped<IActivityProcessingStep, StatsStep>();
        services.AddSingleton<ActivityQueue>();
        services.AddScoped<ActivityProcessor>();
        services.AddScoped<IScheduledJob, ActivityProcessorJob>();
        services.AddHostedService<ActivityQueueWorker>();

        return services;
    }
}
