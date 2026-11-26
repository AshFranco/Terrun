using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Terrun.Api.Modules.Platform.Contracts;
using Terrun.Infrastructure.Persistence;

namespace Terrun.Api.Modules.Platform.Services;

// LLD §2.4 and standards §3.1: game rules and limits are read from system_setting, never written as literals. A missing
// key is a broken deployment (the seed creates every key), so it fails loudly instead of guessing a value
public sealed class SystemSettingsReader(TerrunDbContext db)
{
    public const string TerritoryZoneResolution = "territory.zone_resolution";
    public const string TerritoryConquestThreshold = "territory.conquest_threshold";
    public const string ActivityMaxPoints = "activity.max_points";
    public const string ActivityMaxDurationHours = "activity.max_duration_h";
    public const string ActivityAutoPauseSpeedMps = "activity.auto_pause_speed_mps";
    public const string AntiCheatMaxAccuracyM = "anticheat.max_accuracy_m";
    public const string AntiCheatMaxJumpM = "anticheat.max_jump_m";
    public const string AntiCheatJumpWindowS = "anticheat.jump_window_s";
    public const string AntiCheatMaxSpeedKmh = "anticheat.max_speed_kmh";
    public const string AntiCheatMinPaceSPerKm = "anticheat.min_pace_s_per_km";
    public const string AntiCheatMaxInvalidDistancePct = "anticheat.max_invalid_distance_pct";
    public const string AntiCheatMinDurationS = "anticheat.min_duration_s";
    public const string XpLevelCurve = "xp.level_curve";
    public const string CaloriesDefaultWeightKg = "calories.default_weight_kg";
    public const string PrivacyRouteTrimM = "privacy.route_trim_m";

    public static IReadOnlyList<string> PublicKeys { get; } =
    [
        TerritoryZoneResolution,
        TerritoryConquestThreshold,
        ActivityMaxPoints,
        ActivityMaxDurationHours,
        ActivityAutoPauseSpeedMps,
        XpLevelCurve,
        PrivacyRouteTrimM,
    ];

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public async Task<T> GetAsync<T>(string key, CancellationToken cancellationToken)
    {
        var value = await db.SystemSettings.AsNoTracking()
            .Where(s => s.Key == key)
            .Select(s => s.Value)
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException($"The system setting '{key}' does not exist.");

        return Parse<T>(key, value);
    }

    // Several settings in one query, for code that needs a whole group (the anti-cheat limits of one activity)
    public async Task<IReadOnlyDictionary<string, double>> GetNumbersAsync(IReadOnlyCollection<string> keys, CancellationToken cancellationToken)
    {
        var values = await db.SystemSettings.AsNoTracking()
            .Where(s => keys.Contains(s.Key))
            .ToDictionaryAsync(s => s.Key, s => s.Value, cancellationToken);
        var missing = keys.Where(key => !values.ContainsKey(key)).ToList();
        if (missing.Count > 0)
        {
            throw new InvalidOperationException($"Missing system settings: {string.Join(", ", missing)}.");
        }

        return values.ToDictionary(v => v.Key, v => Parse<double>(v.Key, v.Value));
    }

    // CTZ-17 (LLD §3.2): reads only rows marked is_public = true so private anti-cheat limits are never exposed
    public async Task<PublicSettingsResponse> GetPublicAsync(CancellationToken cancellationToken)
    {
        var values = await db.SystemSettings.AsNoTracking()
            .Where(s => s.IsPublic)
            .ToDictionaryAsync(s => s.Key, s => s.Value, cancellationToken);
        var missing = PublicKeys.Where(key => !values.ContainsKey(key)).ToList();
        if (missing.Count > 0)
        {
            throw new InvalidOperationException($"Missing public system settings: {string.Join(", ", missing)}.");
        }

        return new PublicSettingsResponse(
            Territory: new TerritoryPublicSettings(
                ZoneResolution: Parse<int>(TerritoryZoneResolution, values[TerritoryZoneResolution]),
                ConquestThreshold: Parse<int>(TerritoryConquestThreshold, values[TerritoryConquestThreshold])),
            Activity: new ActivityPublicSettings(
                MaxPoints: Parse<int>(ActivityMaxPoints, values[ActivityMaxPoints]),
                MaxDurationH: Parse<int>(ActivityMaxDurationHours, values[ActivityMaxDurationHours]),
                AutoPauseSpeedMps: Parse<double>(ActivityAutoPauseSpeedMps, values[ActivityAutoPauseSpeedMps])),
            Xp: new XpPublicSettings(
                LevelCurve: Parse<LevelCurvePublicSettings>(XpLevelCurve, values[XpLevelCurve])),
            Privacy: new PrivacyPublicSettings(
                RouteTrimM: Parse<int>(PrivacyRouteTrimM, values[PrivacyRouteTrimM])));
    }

    private static T Parse<T>(string key, string value) =>
        JsonSerializer.Deserialize<T>(value, _jsonOptions) ?? throw new InvalidOperationException($"The system setting '{key}' has no value.");
}
