namespace Terrun.Domain.Activities;

// LLD §4.1: an upload starts pending; the processor moves it to processed, in_review, rejected or failed
public enum ActivityStatus
{
    Pending,
    Processing,
    Processed,
    InReview,
    Rejected,
    Failed,
}
