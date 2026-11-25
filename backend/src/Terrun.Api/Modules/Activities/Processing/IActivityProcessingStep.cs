namespace Terrun.Api.Modules.Activities.Processing;

// LLD §4.1: one stage of the processing of an activity (cleaning, metrics, verdict, territory, progression...). The
// stages run in Order inside one transaction; a stage that decides the verdict sets ActivityProcessingContext.Activity.Status
public interface IActivityProcessingStep
{
    int Order { get; }

    Task RunAsync(ActivityProcessingContext context, CancellationToken cancellationToken);
}
