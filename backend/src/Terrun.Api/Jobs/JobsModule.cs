using Terrun.Api.Modules.Identity.Jobs;

namespace Terrun.Api.Jobs;

public static class JobsModule
{
    public static WebApplicationBuilder AddJobsModule(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<JobsOptions>(builder.Configuration.GetSection(JobsOptions.SectionName));
        builder.Services.AddScoped<JobScheduler>();
        builder.Services.AddScoped<IScheduledJob, TokenCleanupJob>();

        return builder;
    }
}
