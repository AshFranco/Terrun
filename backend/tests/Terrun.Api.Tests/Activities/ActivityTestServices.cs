using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Terrun.Api.Modules.Activities.Processing;

namespace Terrun.Api.Tests.Activities;

internal static class ActivityTestServices
{
    // Without the background worker nothing processes a pending activity while a test processes it by hand
    public static void RemoveQueueWorker(IServiceCollection services)
    {
        foreach (var worker in services.Where(d => d.ServiceType == typeof(IHostedService) && d.ImplementationType == typeof(ActivityQueueWorker)).ToList())
        {
            services.Remove(worker);
        }
    }
}
