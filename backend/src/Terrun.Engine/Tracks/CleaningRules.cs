namespace Terrun.Engine.Tracks;

// LLD §2.4 and §4.1: every threshold comes from system_setting; the engine never holds a game number of its own
public sealed record CleaningRules(
    double MaxAccuracyM,
    double MaxJumpM,
    double JumpWindowS,
    double MaxSpeedKmh,
    double MovingSpeedMps,
    double MinPaceSPerKm,
    double MaxInvalidDistancePct);
