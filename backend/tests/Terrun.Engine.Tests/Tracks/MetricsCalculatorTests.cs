using Terrun.Engine.Tracks;

namespace Terrun.Engine.Tests.Tracks;

// CTZ-31b (LLD §4.1 step 2, §9). Each MT case is calculated by hand in docs/qa/04-CASOS/
public sealed class MetricsCalculatorTests
{
    private const double _tolerance = 1e-6;
    private static readonly MetricsOptions _run70Kg = new(IsWalk: false, WeightKg: 70, MovingSpeedMps: 0.5);

    // 5,400 m at 3 m/s with a point every 10 s (30 m per leg): 180 legs, 1,800 s
    [Fact]
    [Trait("Case", "MT-15")]
    public void Calculate_RunOf5Point4Km_HasFiveFullSplitsAndAPartialOf400Meters()
    {
        var metrics = Calculate(Track.Steady(speedMps: 3, intervalS: 10, points: 181));

        Assert.Equal(6, metrics.Splits.Count);
        Assert.All(metrics.Splits.Take(5), split =>
        {
            Assert.Equal(1000, split.DistanceM, _tolerance);
            Assert.Equal(333.333333, split.DurationS, 1e-4);
        });
        Assert.Equal(400, metrics.Splits[5].DistanceM, 1e-4);
        Assert.Equal(133.333333, metrics.Splits[5].DurationS, 1e-4);
        Assert.Equal([1, 2, 3, 4, 5, 6], metrics.Splits.Select(s => s.Number));
        Assert.Equal(333.333333, metrics.Splits[5].PaceSPerKm, 1e-4);
    }

    [Fact]
    public void Calculate_ExactlyFiveKilometers_HasNoEmptyPartialSplit()
    {
        var metrics = Calculate(Track.Steady(speedMps: 2.5, intervalS: 10, points: 201));

        Assert.Equal(5, metrics.Splits.Count);
        Assert.Equal(5000, metrics.DistanceM, 1e-4);
    }

    [Fact]
    public void Calculate_SteadyRun_ReturnsDistanceMovingTimePaceAndMaxSpeed()
    {
        var metrics = Calculate(Track.Steady(speedMps: 3, intervalS: 10, points: 181));

        Assert.Equal(5400, metrics.DistanceM, 1e-4);
        Assert.Equal(1800, metrics.MovingTimeS, _tolerance);
        Assert.Equal(1800, metrics.ElapsedTimeS, _tolerance);
        Assert.Equal(333.333333, metrics.AvgPaceSPerKm!.Value, 1e-4);
        Assert.Equal(3, metrics.MaxSpeedMps, 1e-6);
    }

    // 3 m/s = 10.8 km/h, between 9.7 km/h (9.8 MET) and 11.3 km/h (11.0 MET): MET = 9.8 + 1.2 × 1.1 / 1.6 = 10.625.
    // 10.625 × 70 kg × 0.5 h = 371.875 kcal
    [Fact]
    public void Calculate_HalfHourAt10Point8KmPerHourAnd70Kg_Burns371Point875Kcal()
    {
        var metrics = Calculate(Track.Steady(speedMps: 3, intervalS: 10, points: 181));

        Assert.Equal(371.875, metrics.Calories, 1e-3);
    }

    [Theory]
    [InlineData(5, false, 8.3)]
    [InlineData(20, false, 14.5)]
    [InlineData(12.9, false, 11.8)]
    [InlineData(5.7, true, 4.25)]
    [InlineData(8, true, 5.0)]
    public void MetFor_Speed_InterpolatesOrTakesTheNearestRow(double speedKmh, bool isWalk, double expected)
    {
        Assert.Equal(expected, MetTable.MetFor(speedKmh, isWalk), 1e-9);
    }

    // Altitudes 0, 0, 0, 10, 10, 10, 10, 0, 0, 0. Centered average of 5 points (cut at the edges):
    // 0, 2.5, 4, 6, 8, 8, 6, 4, 2.5, 0. With the 3 m hysteresis from 0: +4 (at 4), +4 (at 8), then -4 (at 4), -4 (at 0)
    [Fact]
    public void Calculate_Hill_CountsGainAndLossOverTheHysteresis()
    {
        double[] altitudes = [0, 0, 0, 10, 10, 10, 10, 0, 0, 0];
        var points = altitudes.Select((alt, i) => Track.At(i, i * 30, i * 10, altitudeM: alt)).ToList();

        var metrics = Calculate(points);

        Assert.Equal(8, metrics.ElevationGainM, 1e-9);
        Assert.Equal(8, metrics.ElevationLossM, 1e-9);
    }

    [Fact]
    public void Calculate_AltitudeNoiseUnder3Meters_CountsNoGainOrLoss()
    {
        double[] altitudes = [10, 12, 10, 12, 10, 12, 10, 12];
        var points = altitudes.Select((alt, i) => Track.At(i, i * 30, i * 10, altitudeM: alt)).ToList();

        var metrics = Calculate(points);

        Assert.Equal(0, metrics.ElevationGainM);
        Assert.Equal(0, metrics.ElevationLossM);
    }

    // A straight line simplifies to its two ends; a corner survives
    [Fact]
    public void Calculate_StraightRouteWithACorner_KeepsTheEndsAndTheCorner()
    {
        var straight = Enumerable.Range(0, 11).Select(i => Track.At(i, i * 30, i * 10)).ToList();
        var corner = straight[^1];
        straight.Add(corner with { Seq = 11, Lon = corner.Lon + 0.001, RecordedAt = corner.RecordedAt.AddSeconds(40) });

        var metrics = Calculate(straight);

        Assert.Equal(3, metrics.Route.Count);
        Assert.Equal(straight[0].Lat, metrics.Route[0].Lat);
        Assert.Equal(corner.Lat, metrics.Route[1].Lat);
    }

    [Fact]
    public void Calculate_DiscardedPoint_IsLeftOutOfTheRoute()
    {
        var points = Enumerable.Range(0, 5).Select(i => Track.At(i, i * 30, i * 10)).ToList();
        points[2] = points[2] with { AccuracyM = 50, Lon = points[2].Lon + 0.01 };

        var metrics = Calculate(points);

        Assert.DoesNotContain(metrics.Route, p => p.Lon == points[2].Lon);
    }

    private static ActivityMetrics Calculate(IReadOnlyList<TrackPoint> points)
    {
        var cleaning = TrackCleaner.Clean(points, Track.Rules);
        var startedAt = points.Min(p => p.RecordedAt);
        var endedAt = points.Max(p => p.RecordedAt);
        return MetricsCalculator.Calculate(points, cleaning, startedAt, endedAt, _run70Kg);
    }
}
