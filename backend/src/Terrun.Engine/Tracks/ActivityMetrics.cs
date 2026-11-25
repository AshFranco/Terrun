namespace Terrun.Engine.Tracks;

public sealed record MetricsOptions(bool IsWalk, double WeightKg, double MovingSpeedMps);

public sealed record ActivityMetrics(
    double ElapsedTimeS,
    double MovingTimeS,
    double DistanceM,
    double? AvgPaceSPerKm,
    double MaxSpeedMps,
    double ElevationGainM,
    double ElevationLossM,
    double Calories,
    IReadOnlyList<SplitResult> Splits,
    IReadOnlyList<GeoPoint> Route);

// LLD §4.1 step 2 (CTZ-31b): the official metrics, computed only over what the cleaning kept and the valid legs
public static class MetricsCalculator
{
    private const double _kmhPerMps = 3.6;
    private const double _secondsPerHour = 3600;

    public static ActivityMetrics Calculate(
        IReadOnlyList<TrackPoint> points, CleaningResult cleaning, DateTimeOffset startedAt, DateTimeOffset endedAt, MetricsOptions options)
    {
        var discarded = cleaning.Discarded.Select(d => d.Seq).ToHashSet();
        var kept = points.Where(p => !discarded.Contains(p.Seq)).OrderBy(p => p.Seq).ToList();
        var validLegs = cleaning.Legs.Where(l => l.IsValid).ToList();
        var smoothed = ElevationProfile.Smooth(kept);
        var (gain, loss) = ElevationProfile.GainAndLoss(kept, smoothed);

        return new ActivityMetrics(
            ElapsedTimeS: (endedAt - startedAt).TotalSeconds,
            MovingTimeS: cleaning.MovingTimeS,
            DistanceM: cleaning.ValidDistanceM,
            AvgPaceSPerKm: cleaning.AvgPaceSPerKm,
            MaxSpeedMps: validLegs.Count > 0 ? validLegs.Max(l => l.SpeedMps) : 0,
            ElevationGainM: gain,
            ElevationLossM: loss,
            Calories: Calories(validLegs, options),
            Splits: SplitCalculator.Calculate(validLegs, smoothed),
            Route: Route(kept));
    }

    // calories = Σ MET(leg speed) × weightKg × leg hours, only while moving
    private static double Calories(IEnumerable<TrackLeg> validLegs, MetricsOptions options) =>
        validLegs
            .Where(l => l.SpeedMps >= options.MovingSpeedMps)
            .Sum(l => MetTable.MetFor(l.SpeedMps * _kmhPerMps, options.IsWalk) * options.WeightKg * (l.DurationS / _secondsPerHour));

    // Each segment is simplified on its own and the segments are joined in order (D-37)
    private static IReadOnlyList<GeoPoint> Route(IReadOnlyList<TrackPoint> kept) =>
        [.. kept.GroupBy(p => p.Segment)
            .OrderBy(g => g.Key)
            .SelectMany(segment => RouteSimplifier.Simplify([.. segment.Select(p => new GeoPoint(p.Lat, p.Lon))]))];
}
