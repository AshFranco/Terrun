using Microsoft.EntityFrameworkCore;
using Terrun.Api.Modules.Platform.Services;
using Terrun.Domain.Activities;
using Terrun.Engine.Tracks;
using Terrun.Infrastructure.Persistence;

namespace Terrun.Api.Modules.Activities.Processing;

// The settings the engine needs, read in one query each (LLD §2.4). Shared by the processing stages and by the edit of an
// activity, which recalculates its calories when the type changes (D-38)
public sealed class ProcessingSettings(SystemSettingsReader settings, TerrunDbContext db)
{
    private static readonly string[] _cleaningKeys =
    [
        SystemSettingsReader.AntiCheatMaxAccuracyM,
        SystemSettingsReader.AntiCheatMaxJumpM,
        SystemSettingsReader.AntiCheatJumpWindowS,
        SystemSettingsReader.AntiCheatMaxSpeedKmh,
        SystemSettingsReader.ActivityAutoPauseSpeedMps,
        SystemSettingsReader.AntiCheatMinPaceSPerKm,
        SystemSettingsReader.AntiCheatMaxInvalidDistancePct,
    ];

    private static readonly string[] _metricsKeys =
    [
        SystemSettingsReader.ActivityAutoPauseSpeedMps,
        SystemSettingsReader.AntiCheatMinDurationS,
        SystemSettingsReader.CaloriesDefaultWeightKg,
    ];

    public async Task<CleaningRules> CleaningRulesAsync(CancellationToken cancellationToken)
    {
        var values = await settings.GetNumbersAsync(_cleaningKeys, cancellationToken);
        return new CleaningRules(
            MaxAccuracyM: values[SystemSettingsReader.AntiCheatMaxAccuracyM],
            MaxJumpM: values[SystemSettingsReader.AntiCheatMaxJumpM],
            JumpWindowS: values[SystemSettingsReader.AntiCheatJumpWindowS],
            MaxSpeedKmh: values[SystemSettingsReader.AntiCheatMaxSpeedKmh],
            MovingSpeedMps: values[SystemSettingsReader.ActivityAutoPauseSpeedMps],
            MinPaceSPerKm: values[SystemSettingsReader.AntiCheatMinPaceSPerKm],
            MaxInvalidDistancePct: values[SystemSettingsReader.AntiCheatMaxInvalidDistancePct]);
    }

    // The weight comes from the profile, or calories.default_weight_kg when the runner has none
    public async Task<(MetricsOptions Options, double MinDurationS)> MetricsOptionsAsync(Activity activity, CancellationToken cancellationToken)
    {
        var values = await settings.GetNumbersAsync(_metricsKeys, cancellationToken);
        var weightKg = await db.Profiles.AsNoTracking()
            .Where(p => p.UserId == activity.UserId)
            .Select(p => p.WeightKg)
            .SingleOrDefaultAsync(cancellationToken);
        var options = new MetricsOptions(
            IsWalk: activity.Type == ActivityType.Walk,
            WeightKg: (double?)weightKg ?? values[SystemSettingsReader.CaloriesDefaultWeightKg],
            MovingSpeedMps: values[SystemSettingsReader.ActivityAutoPauseSpeedMps]);
        return (options, values[SystemSettingsReader.AntiCheatMinDurationS]);
    }

    public Task<List<TrackPoint>> PointsAsync(Guid activityId, CancellationToken cancellationToken) =>
        db.GpsPoints.AsNoTracking()
            .Where(p => p.ActivityId == activityId)
            .Select(p => new TrackPoint(p.Seq, p.Segment, p.RecordedAt, p.Lat, p.Lon, p.AccuracyM, p.AltitudeM))
            .ToListAsync(cancellationToken);
}
