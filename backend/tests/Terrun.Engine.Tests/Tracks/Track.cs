using Terrun.Engine.Tracks;

namespace Terrun.Engine.Tests.Tracks;

// Builds tracks on a made-up meridian (standards §6: never anyone's real coordinates). Moving north along a meridian,
// the Haversine distance is exactly R × Δφ, which keeps the hand calculations of docs/qa/04-CASOS simple
internal static class Track
{
    public const double OriginLat = 10.0;
    public const double OriginLon = -60.0;
    public static readonly DateTimeOffset Start = new(2026, 11, 21, 10, 0, 0, TimeSpan.Zero);

    // The default anti-cheat settings of the seed (LLD §2.4)
    public static readonly CleaningRules Rules = new(
        MaxAccuracyM: 30,
        MaxJumpM: 200,
        JumpWindowS: 5,
        MaxSpeedKmh: 25,
        MovingSpeedMps: 0.5,
        MinPaceSPerKm: 150,
        MaxInvalidDistancePct: 20);

    public static double LatAtMetersNorth(double meters) => OriginLat + (meters / Geo.EarthRadiusM * 180 / Math.PI);

    public static TrackPoint At(int seq, double metersNorth, double seconds, int segment = 0, double? accuracyM = 5) =>
        new(seq, segment, Start.AddSeconds(seconds), LatAtMetersNorth(metersNorth), OriginLon, accuracyM);

    // A steady run: one point every intervalS seconds at speedMps, starting at the origin
    public static List<TrackPoint> Steady(double speedMps, double intervalS, int points) =>
        [.. Enumerable.Range(0, points).Select(i => At(i, i * speedMps * intervalS, i * intervalS))];
}
