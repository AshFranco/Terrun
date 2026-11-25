namespace Terrun.Engine.Tracks;

// LLD §4.1 step 1 (CTZ-31a): drops the points with bad accuracy, repeated or backwards timestamps and impossible jumps,
// then measures each leg between consecutive kept points of the same segment. Distance never joins two segments, so a
// pause adds no distance and no moving time (MT-14)
public static class TrackCleaner
{
    private const double _kmhPerMps = 3.6;

    public static CleaningResult Clean(IReadOnlyList<TrackPoint> points, CleaningRules rules)
    {
        var discarded = new List<DiscardedPoint>();
        var legs = new List<TrackLeg>();
        TrackPoint? previous = null;

        foreach (var point in points.OrderBy(p => p.Seq))
        {
            if (point.AccuracyM > rules.MaxAccuracyM)
            {
                discarded.Add(new DiscardedPoint(point.Seq, PointDiscardReason.Accuracy));
                continue;
            }

            if (previous is null || previous.Segment != point.Segment)
            {
                previous = point;
                continue;
            }

            var durationS = (point.RecordedAt - previous.RecordedAt).TotalSeconds;
            var distanceM = Geo.HaversineM(previous.Lat, previous.Lon, point.Lat, point.Lon);
            if (Discard(durationS, distanceM, rules) is { } reason)
            {
                discarded.Add(new DiscardedPoint(point.Seq, reason));
                continue;
            }

            var isValid = distanceM / durationS * _kmhPerMps <= rules.MaxSpeedKmh;
            legs.Add(new TrackLeg(previous.Seq, point.Seq, point.Segment, distanceM, durationS, isValid));
            previous = point;
        }

        return Summarize(discarded, legs, rules);
    }

    private static PointDiscardReason? Discard(double durationS, double distanceM, CleaningRules rules) =>
        durationS <= 0 ? PointDiscardReason.Timestamp
        : distanceM > rules.MaxJumpM && durationS < rules.JumpWindowS ? PointDiscardReason.Jump
        : null;

    // LLD §4.1 step 2: distance over valid legs; moving time only where the runner was moving
    private static CleaningResult Summarize(List<DiscardedPoint> discarded, List<TrackLeg> legs, CleaningRules rules)
    {
        var valid = legs.Where(l => l.IsValid).ToList();
        return new CleaningResult(
            discarded,
            legs,
            ValidDistanceM: valid.Sum(l => l.DistanceM),
            InvalidDistanceM: legs.Where(l => !l.IsValid).Sum(l => l.DistanceM),
            MovingTimeS: valid.Where(l => l.SpeedMps >= rules.MovingSpeedMps).Sum(l => l.DurationS));
    }
}
