namespace Terrun.Engine.Tracks;

// A leg joins two consecutive kept points of the same segment; an invalid leg (too fast) adds neither distance nor
// territory (MT-03)
public sealed record TrackLeg(int FromSeq, int ToSeq, int Segment, double DistanceM, double DurationS, bool IsValid)
{
    public double SpeedMps => DurationS > 0 ? DistanceM / DurationS : 0;
}

public sealed record DiscardedPoint(int Seq, PointDiscardReason Reason);

public sealed record CleaningResult(
    IReadOnlyList<DiscardedPoint> Discarded,
    IReadOnlyList<TrackLeg> Legs,
    double ValidDistanceM,
    double InvalidDistanceM,
    double MovingTimeS)
{
    // Pace over the valid distance; null when nothing valid was run
    public double? AvgPaceSPerKm => ValidDistanceM > 0 ? MovingTimeS / (ValidDistanceM / 1000) : null;

    public double InvalidDistancePct =>
        ValidDistanceM + InvalidDistanceM > 0 ? InvalidDistanceM * 100 / (ValidDistanceM + InvalidDistanceM) : 0;
}
