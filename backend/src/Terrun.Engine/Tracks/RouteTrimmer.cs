namespace Terrun.Engine.Tracks;

// LLD §7.2 and D-29 (MT-21): what other runners see of a route. The first and the last trimM meters, measured along the
// whole route, are hidden, cutting the route at the exact interpolated points; a route of 2 × trimM or less shows nothing.
// The owner always gets the complete route, so this is applied on read and a change of privacy.route_trim_m applies to
// every activity
public static class RouteTrimmer
{
    public static IReadOnlyList<GeoPoint> TrimForOthers(IReadOnlyList<GeoPoint> route, double trimM)
    {
        if (route.Count < 2)
        {
            return [];
        }

        var cumulative = new double[route.Count];
        for (var i = 1; i < route.Count; i++)
        {
            cumulative[i] = cumulative[i - 1] + Geo.HaversineM(route[i - 1].Lat, route[i - 1].Lon, route[i].Lat, route[i].Lon);
        }

        var total = cumulative[^1];
        if (total <= 2 * trimM)
        {
            return [];
        }

        var (from, to) = (trimM, total - trimM);
        var trimmed = new List<GeoPoint> { PointAt(route, cumulative, from) };
        trimmed.AddRange(route.Where((_, i) => cumulative[i] > from && cumulative[i] < to));
        trimmed.Add(PointAt(route, cumulative, to));
        return trimmed;
    }

    private static GeoPoint PointAt(IReadOnlyList<GeoPoint> route, double[] cumulative, double distanceM)
    {
        var i = Array.FindIndex(cumulative, d => d >= distanceM);
        if (i <= 0)
        {
            return route[Math.Max(i, 0)];
        }

        var fraction = (distanceM - cumulative[i - 1]) / (cumulative[i] - cumulative[i - 1]);
        var (a, b) = (route[i - 1], route[i]);
        return new GeoPoint(a.Lat + ((b.Lat - a.Lat) * fraction), a.Lon + ((b.Lon - a.Lon) * fraction));
    }
}
