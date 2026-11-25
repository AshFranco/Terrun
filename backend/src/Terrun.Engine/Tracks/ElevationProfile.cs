namespace Terrun.Engine.Tracks;

// LLD §4.1: GPS altitude is noisy, so it is smoothed with a centered moving average of 5 points and only changes larger
// than the 3 m hysteresis count as gain or loss. Each segment is measured on its own: a climb during a pause does not count
public static class ElevationProfile
{
    public const int SmoothingWindow = 5;
    public const double HysteresisM = 3;

    // Smoothed altitude of every kept point that has one, by seq
    public static IReadOnlyDictionary<int, double> Smooth(IReadOnlyList<TrackPoint> keptPoints)
    {
        var smoothed = new Dictionary<int, double>();
        foreach (var segment in keptPoints.Where(p => p.AltitudeM is not null).GroupBy(p => p.Segment))
        {
            var points = segment.OrderBy(p => p.Seq).ToList();
            for (var i = 0; i < points.Count; i++)
            {
                var from = Math.Max(0, i - (SmoothingWindow / 2));
                var to = Math.Min(points.Count - 1, i + (SmoothingWindow / 2));
                smoothed[points[i].Seq] = points.Skip(from).Take(to - from + 1).Average(p => p.AltitudeM!.Value);
            }
        }

        return smoothed;
    }

    public static (double GainM, double LossM) GainAndLoss(IReadOnlyList<TrackPoint> keptPoints, IReadOnlyDictionary<int, double> smoothed)
    {
        var (gain, loss) = (0.0, 0.0);
        foreach (var segment in keptPoints.Where(p => smoothed.ContainsKey(p.Seq)).GroupBy(p => p.Segment))
        {
            double? reference = null;
            foreach (var altitude in segment.OrderBy(p => p.Seq).Select(p => smoothed[p.Seq]))
            {
                reference ??= altitude;
                if (altitude - reference >= HysteresisM)
                {
                    gain += altitude - reference.Value;
                    reference = altitude;
                }
                else if (reference - altitude >= HysteresisM)
                {
                    loss += reference.Value - altitude;
                    reference = altitude;
                }
            }
        }

        return (gain, loss);
    }
}
