namespace Terrun.Api.Jobs;

// LLD §6: a job the hourly external trigger runs when its cron is due. Its work must be safe to repeat, because a job
// that fails runs again in the next trigger
public interface IScheduledJob
{
    // Key in job_run; never rename a job, or it runs again as if it were new
    string Name { get; }

    // Five-field cron in the time zone of JobsOptions.TimeZone (AST)
    string Cron { get; }

    Task RunAsync(CancellationToken cancellationToken);
}
