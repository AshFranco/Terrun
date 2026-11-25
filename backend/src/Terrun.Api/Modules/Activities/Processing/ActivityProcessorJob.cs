using Terrun.Api.Jobs;

namespace Terrun.Api.Modules.Activities.Processing;

// LLD §6: in every hourly trigger, process whatever is still pending (an upload the queue lost, a failed attempt)
public sealed class ActivityProcessorJob(ActivityProcessor processor) : IScheduledJob
{
    public string Name => "activity_processor";

    // Due in every hourly trigger
    public string Cron => "* * * * *";

    public Task RunAsync(CancellationToken cancellationToken) => processor.ProcessPendingAsync(cancellationToken);
}
