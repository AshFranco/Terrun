using Terrun.Domain.Platform;

namespace Terrun.Infrastructure.Persistence.Seed;

// LLD §2.4. Public settings are the ones the app needs (GET /settings/public); anti-cheat limits stay private
internal static class SystemSettingSeed
{
    private static readonly DateTimeOffset _seededAt = new(2026, 11, 15, 0, 0, 0, TimeSpan.Zero);

    public static readonly SystemSetting[] All =
    [
        Setting("territory.zone_resolution", "9", "H3 resolution of a zone", isPublic: true),
        Setting("territory.conquest_threshold", "5", "Sub-cells (out of 7) needed to conquer a zone", isPublic: true),
        Setting("territory.ownership_window_days", "30", "Window of the ownership points, in days"),
        Setting("territory.min_ownership_points", "3", "Minimum points to own a zone"),
        Setting("territory.densify_step_m", "10", "Interpolation step of a route, in meters"),
        Setting("territory.min_zones_for_ruler", "3", "Minimum owned zones to become ruler of a neighborhood"),
        Setting("anticheat.max_accuracy_m", "30", "Maximum accepted GPS accuracy, in meters"),
        Setting("anticheat.max_speed_kmh", "25", "Maximum speed of a segment, in km/h"),
        Setting("anticheat.min_pace_s_per_km", "150", "Suspicious sustained pace (2:30 min/km), in seconds per km"),
        Setting("anticheat.min_duration_s", "180", "Minimum duration to add territory, in seconds"),
        Setting("anticheat.max_jump_m", "200", "Maximum jump in less than 5 seconds, in meters"),
        Setting("activity.max_points", "30000", "Maximum points per activity", isPublic: true),
        Setting("activity.auto_pause_speed_mps", "0.5", "Auto-pause speed threshold used by the app, in m/s", isPublic: true),
        Setting("xp.per_km", "10", "XP per kilometer"),
        Setting("xp.conquest", "50", "XP per conquered zone"),
        Setting("xp.ownership", "30", "XP for gaining the ownership of a zone"),
        Setting("xp.steal", "40", "XP for stealing a zone"),
        Setting("xp.level_curve", """{"base":100,"exponent":1.5}""", "XP for level n = base * n^exponent", isPublic: true),
        Setting("calories.default_weight_kg", "70", "Weight used when the profile has none, in kg"),
        Setting("map.max_zones_per_bbox", "2000", "Maximum zones per map query"),
    ];

    private static SystemSetting Setting(string key, string value, string description, bool isPublic = false) => new()
    {
        Key = key,
        Value = value,
        Description = description,
        IsPublic = isPublic,
        UpdatedAt = _seededAt,
    };
}
