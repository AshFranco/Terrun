using Terrun.Engine.Tracks;

namespace Terrun.Engine.Tests.Tracks;

// CTZ-31a (LLD §4.1 step 3, §9)
public sealed class AntiCheatJudgeTests
{
    // 2:10 min/km = 130 s/km = 7.692 m/s = 27.7 km/h: above the 25 km/h limit, so every leg is invalid and the activity
    // goes to review because 100 % of its distance is invalid (the pace rule cannot apply with no valid distance)
    [Fact]
    [Trait("Case", "MT-04")]
    public void Judge_PaceOf2m10sPerKmFor20Minutes_SendsTheActivityToReview()
    {
        var result = TrackCleaner.Clean(Track.Steady(speedMps: 1000.0 / 130, intervalS: 5, points: 241), Track.Rules);

        var violations = AntiCheatJudge.Judge(result, Track.Rules);

        Assert.Equal(0, result.ValidDistanceM);
        Assert.Equal(new RuleViolation(AntiCheatJudge.TooMuchInvalidDistance, 100, 20), Assert.Single(violations));
    }

    // 2:26 min/km = 146 s/km = 6.849 m/s = 24.7 km/h: every leg is valid, but the pace is under the 150 s/km minimum
    [Fact]
    public void Judge_PaceOf2m26sPerKm_ViolatesOnlyTheMinimumPace()
    {
        var result = TrackCleaner.Clean(Track.Steady(speedMps: 1000.0 / 146, intervalS: 5, points: 241), Track.Rules);

        var violations = AntiCheatJudge.Judge(result, Track.Rules);

        Assert.Equal(new RuleViolation(AntiCheatJudge.PaceTooFast, 146, 150), Assert.Single(violations));
    }

    [Fact]
    public void Judge_NormalRunAt5mPerKm_HasNoViolations()
    {
        var result = TrackCleaner.Clean(Track.Steady(speedMps: 1000.0 / 300, intervalS: 5, points: 241), Track.Rules);

        Assert.Empty(AntiCheatJudge.Judge(result, Track.Rules));
        Assert.Equal(300, result.AvgPaceSPerKm!.Value, 1e-6);
    }

    // A normal run with a car ride in the middle: 25 % of the distance is invalid, above the 20 % limit
    [Fact]
    public void Judge_QuarterOfTheDistanceInvalid_SendsTheActivityToReview()
    {
        // 30 legs of 30 m in 10 s (10.8 km/h) = 900 m valid; then 3 legs of 100 m in 5 s (72 km/h) = 300 m invalid
        var points = Enumerable.Range(0, 31).Select(i => Track.At(i, i * 30, i * 10)).ToList();
        points.AddRange(Enumerable.Range(1, 3).Select(i => Track.At(30 + i, 900 + (i * 100), 300 + (i * 5))));

        var result = TrackCleaner.Clean(points, Track.Rules);
        var violations = AntiCheatJudge.Judge(result, Track.Rules);

        Assert.Equal(25, result.InvalidDistancePct, 1e-6);
        Assert.Equal(AntiCheatJudge.TooMuchInvalidDistance, Assert.Single(violations).Rule);
    }

    [Fact]
    public void Judge_ExactlyTwentyPercentInvalid_IsAccepted()
    {
        // 40 legs of 30 m = 1200 m valid; 3 legs of 100 m in 5 s = 300 m invalid → 300 / 1500 = 20 %
        var points = Enumerable.Range(0, 41).Select(i => Track.At(i, i * 30, i * 10)).ToList();
        points.AddRange(Enumerable.Range(1, 3).Select(i => Track.At(40 + i, 1200 + (i * 100), 400 + (i * 5))));

        var result = TrackCleaner.Clean(points, Track.Rules);

        Assert.Equal(20, result.InvalidDistancePct, 1e-6);
        Assert.Empty(AntiCheatJudge.Judge(result, Track.Rules));
    }
}
