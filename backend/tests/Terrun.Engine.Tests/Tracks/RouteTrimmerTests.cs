using Terrun.Engine.Tracks;

namespace Terrun.Engine.Tests.Tracks;

// CTZ-31b (LLD §7.2, D-29, §9): what other runners see of a route
public sealed class RouteTrimmerTests
{
    private const double _trimM = 200;

    // 3 km straight north, one point every 100 m: others see from 200 m to 2,800 m, cut at the exact points
    [Fact]
    [Trait("Case", "MT-21")]
    public void TrimForOthers_RouteOf3Km_HidesTheFirstAndLast200Meters()
    {
        var route = Line(lengthM: 3000, stepM: 100);

        var trimmed = RouteTrimmer.TrimForOthers(route, _trimM);

        Assert.Equal(Track.LatAtMetersNorth(200), trimmed[0].Lat, 1e-9);
        Assert.Equal(Track.LatAtMetersNorth(2800), trimmed[^1].Lat, 1e-9);
        Assert.Equal(2600, Length(trimmed), 1e-3);
        Assert.Equal(31, route.Count);
    }

    [Fact]
    [Trait("Case", "MT-21")]
    public void TrimForOthers_RouteOf350Meters_ShowsNothing()
    {
        Assert.Empty(RouteTrimmer.TrimForOthers(Line(lengthM: 350, stepM: 50), _trimM));
    }

    [Fact]
    public void TrimForOthers_RouteOfExactly400Meters_ShowsNothing()
    {
        Assert.Empty(RouteTrimmer.TrimForOthers(Line(lengthM: 400, stepM: 50), _trimM));
    }

    // The cut falls in the middle of a leg: it is interpolated, not rounded to a recorded point
    [Fact]
    public void TrimForOthers_CutInsideALeg_InterpolatesThePoint()
    {
        var route = Line(lengthM: 1500, stepM: 300);

        var trimmed = RouteTrimmer.TrimForOthers(route, _trimM);

        Assert.Equal([200, 300, 600, 900, 1200, 1300], trimmed.Select(p => Math.Round(MetersNorth(p), 3)));
    }

    private static List<GeoPoint> Line(double lengthM, double stepM) =>
        [.. Enumerable.Range(0, (int)(lengthM / stepM) + 1).Select(i => new GeoPoint(Track.LatAtMetersNorth(i * stepM), Track.OriginLon))];

    private static double MetersNorth(GeoPoint point) => (point.Lat - Track.OriginLat) * Math.PI / 180 * Geo.EarthRadiusM;

    private static double Length(IReadOnlyList<GeoPoint> route) =>
        route.Zip(route.Skip(1), (a, b) => Geo.HaversineM(a.Lat, a.Lon, b.Lat, b.Lon)).Sum();
}
