namespace Terrun.Domain.Activities;

// LLD §4.1: why the cleaning step dropped a point (CTZ-31a)
public enum DiscardReason
{
    Accuracy,
    Timestamp,
    Jump,
}
