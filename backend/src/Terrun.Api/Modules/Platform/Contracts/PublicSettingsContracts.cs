namespace Terrun.Api.Modules.Platform.Contracts;

// LLD §2.4 and §3.2 (M13): public game parameters read by the app so no game rule is ever hardcoded in TypeScript

public sealed record PublicSettingsResponse(
    TerritoryPublicSettings Territory,
    ActivityPublicSettings Activity,
    XpPublicSettings Xp,
    PrivacyPublicSettings Privacy);

public sealed record TerritoryPublicSettings(int ZoneResolution, int ConquestThreshold);

public sealed record ActivityPublicSettings(int MaxPoints, int MaxDurationH, double AutoPauseSpeedMps);

public sealed record XpPublicSettings(LevelCurvePublicSettings LevelCurve);

public sealed record LevelCurvePublicSettings(int Base, double Exponent);

public sealed record PrivacyPublicSettings(int RouteTrimM);
