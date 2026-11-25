namespace Terrun.Engine.Tracks;

// One kilometer of the activity; the last one can be partial (MT-15)
public sealed record SplitResult(int Number, double DistanceM, double DurationS, double? ElevationM)
{
    public double PaceSPerKm => DistanceM > 0 ? DurationS / (DistanceM / 1000) : 0;
}

// LLD §4.1: cuts the valid legs every 1000 m of cumulative distance, interpolating inside the leg that crosses the mark.
// A split lasts the time of its valid legs, so a pause between segments does not slow it down (D-37). Its elevation is
// the net change of the smoothed altitude, or null when the phone gave no altitude
public static class SplitCalculator
{
    public const double MetersPerKilometer = 1000;

    // Rounding of the summed legs must not leave a split of 999.9999999 m or a partial one of 0.0000001 m
    private const double _epsilonM = 1e-6;

    public static IReadOnlyList<SplitResult> Calculate(IReadOnlyList<TrackLeg> validLegs, IReadOnlyDictionary<int, double> smoothedAltitude)
    {
        var splits = new List<SplitResult>();
        var (distance, duration) = (0.0, 0.0);
        double? startAltitude = validLegs.Count > 0 ? AltitudeOf(smoothedAltitude, validLegs[0].FromSeq) : null;

        foreach (var leg in validLegs)
        {
            var consumed = 0.0;
            while (distance + (leg.DistanceM - consumed) >= MetersPerKilometer - _epsilonM && leg.DistanceM > 0)
            {
                var needed = Math.Min(Math.Max(0, MetersPerKilometer - distance), leg.DistanceM - consumed);
                var fraction = (consumed + needed) / leg.DistanceM;
                duration += leg.DurationS * needed / leg.DistanceM;
                var endAltitude = Interpolate(smoothedAltitude, leg, fraction);
                splits.Add(new SplitResult(splits.Count + 1, MetersPerKilometer, duration, endAltitude - startAltitude));
                (distance, duration, startAltitude) = (0, 0, endAltitude);
                consumed += needed;
            }

            var rest = Math.Max(0, leg.DistanceM - consumed);
            distance += rest;
            duration += leg.DistanceM > 0 ? leg.DurationS * rest / leg.DistanceM : leg.DurationS;
        }

        if (distance > _epsilonM)
        {
            var endAltitude = validLegs.Count > 0 ? AltitudeOf(smoothedAltitude, validLegs[^1].ToSeq) : null;
            splits.Add(new SplitResult(splits.Count + 1, distance, duration, endAltitude - startAltitude));
        }

        return splits;
    }

    private static double? AltitudeOf(IReadOnlyDictionary<int, double> smoothed, int seq) =>
        smoothed.TryGetValue(seq, out var altitude) ? altitude : null;

    private static double? Interpolate(IReadOnlyDictionary<int, double> smoothed, TrackLeg leg, double fraction) =>
        AltitudeOf(smoothed, leg.FromSeq) is { } from && AltitudeOf(smoothed, leg.ToSeq) is { } to
            ? from + ((to - from) * fraction)
            : null;
}
