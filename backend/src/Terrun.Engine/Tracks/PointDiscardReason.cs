namespace Terrun.Engine.Tracks;

// LLD §4.1: why the cleaning dropped a point
public enum PointDiscardReason
{
    Accuracy,
    Timestamp,
    Jump,
}
