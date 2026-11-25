using Terrun.Engine.Tracks;

namespace Terrun.Engine.Tests.Tracks;

// CTZ-31a (LLD §4.1, §9). Each MT case is calculated by hand in docs/qa/04-CASOS/
public sealed class TrackCleanerTests
{
    private const double _tolerance = 1e-6;

    [Fact]
    [Trait("Case", "MT-01")]
    public void Haversine_PointsOneKilometerApart_Returns1000Meters()
    {
        var north = Geo.HaversineM(Track.OriginLat, Track.OriginLon, Track.LatAtMetersNorth(1000), Track.OriginLon);

        // 1 km along the equator: Δλ = 1000 / R rad
        var east = Geo.HaversineM(0, 0, 0, 1000 / Geo.EarthRadiusM * 180 / Math.PI);

        Assert.InRange(north, 999.5, 1000.5);
        Assert.InRange(east, 999.5, 1000.5);
    }

    [Fact]
    [Trait("Case", "MT-02")]
    public void Clean_PointWithAccuracyOf45Meters_IsDiscardedAndAddsNoDistance()
    {
        TrackPoint[] points =
        [
            Track.At(0, 0, 0),
            Track.At(1, 1000, 10, accuracyM: 45),
            Track.At(2, 50, 20),
        ];

        var result = TrackCleaner.Clean(points, Track.Rules);

        Assert.Equal([new DiscardedPoint(1, PointDiscardReason.Accuracy)], result.Discarded);
        var leg = Assert.Single(result.Legs);
        Assert.Equal((0, 2), (leg.FromSeq, leg.ToSeq));
        Assert.Equal(50, result.ValidDistanceM, _tolerance);
    }

    [Fact]
    [Trait("Case", "MT-03")]
    public void Clean_LegAt40KmPerHour_IsInvalidAndAddsNoDistance()
    {
        TrackPoint[] points =
        [
            Track.At(0, 0, 0),
            Track.At(1, 100, 9),
            Track.At(2, 130, 19),
        ];

        var result = TrackCleaner.Clean(points, Track.Rules);

        Assert.Empty(result.Discarded);
        Assert.Equal([false, true], result.Legs.Select(l => l.IsValid));
        Assert.Equal(40, result.Legs[0].SpeedMps * 3.6, _tolerance);
        Assert.Equal(30, result.ValidDistanceM, _tolerance);
        Assert.Equal(100, result.InvalidDistanceM, _tolerance);
        Assert.Equal(10, result.MovingTimeS, _tolerance);
    }

    [Fact]
    [Trait("Case", "MT-14")]
    public void Clean_PauseOf10MinutesBetweenSegments_AddsNoDistanceAndNoMovingTime()
    {
        // Segment 0: 10 points, 30 m every 10 s. Pause of 600 s while the runner walks 500 m. Segment 1: same pace
        var points = Enumerable.Range(0, 10).Select(i => Track.At(i, i * 30, i * 10, segment: 0))
            .Concat(Enumerable.Range(0, 10).Select(i => Track.At(10 + i, 770 + (i * 30), 690 + (i * 10), segment: 1)))
            .ToList();

        var result = TrackCleaner.Clean(points, Track.Rules);

        Assert.Equal(18, result.Legs.Count);
        Assert.DoesNotContain(result.Legs, l => l.FromSeq == 9 && l.ToSeq == 10);
        Assert.Equal(540, result.ValidDistanceM, _tolerance);
        Assert.Equal(180, result.MovingTimeS, _tolerance);
    }

    [Fact]
    public void Clean_RepeatedOrBackwardsTimestamp_DiscardsThePoint()
    {
        TrackPoint[] points =
        [
            Track.At(0, 0, 0),
            Track.At(1, 20, 0),
            Track.At(2, 30, 10),
            Track.At(3, 40, 5),
            Track.At(4, 60, 20),
        ];

        var result = TrackCleaner.Clean(points, Track.Rules);

        Assert.Equal(
            [new DiscardedPoint(1, PointDiscardReason.Timestamp), new DiscardedPoint(3, PointDiscardReason.Timestamp)],
            result.Discarded);
        Assert.Equal(60, result.ValidDistanceM, _tolerance);
    }

    // More than 200 m in less than 5 s is a GPS jump: the point is dropped. The same distance in 6 s is a real (too fast)
    // movement: the point stays and the leg is invalid
    [Fact]
    public void Clean_JumpOver200MetersInUnder5Seconds_DiscardsThePointButNotInSixSeconds()
    {
        TrackPoint[] points =
        [
            Track.At(0, 0, 0),
            Track.At(1, 250, 4),
            Track.At(2, 20, 10),
            Track.At(3, 270, 16),
        ];

        var result = TrackCleaner.Clean(points, Track.Rules);

        Assert.Equal([new DiscardedPoint(1, PointDiscardReason.Jump)], result.Discarded);
        Assert.Equal([true, false], result.Legs.Select(l => l.IsValid));
    }

    [Fact]
    public void Clean_PointWithoutAccuracy_IsKept()
    {
        TrackPoint[] points = [Track.At(0, 0, 0, accuracyM: null), Track.At(1, 30, 10, accuracyM: null)];

        var result = TrackCleaner.Clean(points, Track.Rules);

        Assert.Empty(result.Discarded);
        Assert.Equal(30, result.ValidDistanceM, _tolerance);
    }

    [Fact]
    public void Clean_StandingStill_AddsNoMovingTime()
    {
        // 0.3 m/s is below the moving threshold of 0.5 m/s
        var result = TrackCleaner.Clean(Track.Steady(speedMps: 0.3, intervalS: 10, points: 7), Track.Rules);

        Assert.Equal(18, result.ValidDistanceM, _tolerance);
        Assert.Equal(0, result.MovingTimeS);
    }

    [Fact]
    public void Clean_PointsOutOfOrder_AreSortedBySeq()
    {
        TrackPoint[] points = [Track.At(2, 60, 20), Track.At(0, 0, 0), Track.At(1, 30, 10)];

        var result = TrackCleaner.Clean(points, Track.Rules);

        Assert.Equal([(0, 1), (1, 2)], result.Legs.Select(l => (l.FromSeq, l.ToSeq)));
    }
}
