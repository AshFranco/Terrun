using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Terrun.Infrastructure.Persistence;

namespace Terrun.Api.Modules.Platform.Services;

// LLD §2.4 and standards §3.1: game rules and limits are read from system_setting, never written as literals. A missing
// key is a broken deployment (the seed creates every key), so it fails loudly instead of guessing a value
public sealed class SystemSettingsReader(TerrunDbContext db)
{
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
    public const string CaloriesDefaultWeightKg = "calories.default_weight_kg";
    public const string PrivacyRouteTrimM = "privacy.route_trim_m";

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

    private static T Parse<T>(string key, string value) =>
        JsonSerializer.Deserialize<T>(value) ?? throw new InvalidOperationException($"The system setting '{key}' has no value.");
}
