namespace Terrun.Api.Modules.Activities.Processing;

// LLD §6: at startup, processes the activities left pending (an upload whose API restarted, a failed attempt); then it
// processes every upload as soon as it arrives. It never polls the database (D-25). An error is logged and the worker
// keeps going, because the hourly job retries what is still pending
public sealed class ActivityQueueWorker(IServiceScopeFactory scopes, ActivityQueue queue, ILogger<ActivityQueueWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await RunSafelyAsync(processor => processor.ProcessPendingAsync(stoppingToken), "startup sweep");

        await foreach (var activityId in queue.Reader.ReadAllAsync(stoppingToken))
        {
            await RunSafelyAsync(processor => processor.ProcessAsync(activityId, stoppingToken), "queued activity");
        }
    }

    private async Task RunSafelyAsync<T>(Func<ActivityProcessor, Task<T>> work, string what)
    {
        try
        {
            await using var scope = scopes.CreateAsyncScope();
            await work(scope.ServiceProvider.GetRequiredService<ActivityProcessor>());
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogError(exception, "Activity processing ({What}) failed", what);
        }
    }
}
