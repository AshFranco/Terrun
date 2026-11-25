namespace Terrun.Engine.Tracks;

// LLD §4.1: Douglas-Peucker over the kept points, with the tolerance in meters. Iterative, so a route of 30,000 points
// never runs out of stack
public static class RouteSimplifier
{
    public const double ToleranceM = 5;

    public static IReadOnlyList<GeoPoint> Simplify(IReadOnlyList<GeoPoint> points, double toleranceM = ToleranceM)
    {
        if (points.Count <= 2)
        {
            return points;
        }

        var projection = new LocalProjection(points[0].Lat, points[0].Lon);
        var meters = points.Select(projection.ToMeters).ToArray();
        var keep = new bool[points.Count];
        keep[0] = keep[^1] = true;
        var pending = new Stack<(int First, int Last)>();
        pending.Push((0, points.Count - 1));

        while (pending.Count > 0)
        {
            var (first, last) = pending.Pop();
            var (farthest, distance) = Farthest(meters, first, last);
            if (distance > toleranceM)
            {
                keep[farthest] = true;
                pending.Push((first, farthest));
                pending.Push((farthest, last));
            }
        }

        return [.. points.Where((_, i) => keep[i])];
    }

    private static (int Index, double DistanceM) Farthest((double X, double Y)[] meters, int first, int last)
    {
        var (index, max) = (first, 0.0);
        for (var i = first + 1; i < last; i++)
        {
            var distance = DistanceToSegment(meters[i], meters[first], meters[last]);
            if (distance > max)
            {
                (index, max) = (i, distance);
            }
        }

        return (index, max);
    }

    private static double DistanceToSegment((double X, double Y) p, (double X, double Y) a, (double X, double Y) b)
    {
        var (dx, dy) = (b.X - a.X, b.Y - a.Y);
        var lengthSquared = (dx * dx) + (dy * dy);
        var t = lengthSquared == 0 ? 0 : Math.Clamp((((p.X - a.X) * dx) + ((p.Y - a.Y) * dy)) / lengthSquared, 0, 1);
        var (cx, cy) = (a.X + (t * dx), a.Y + (t * dy));
        return Math.Sqrt(((p.X - cx) * (p.X - cx)) + ((p.Y - cy) * (p.Y - cy)));
    }
}
