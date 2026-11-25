using Microsoft.EntityFrameworkCore;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using Terrun.Api.Modules.Platform.Services;
using Terrun.Domain.Activities;
using Terrun.Engine.Tracks;

namespace Terrun.Api.Modules.Activities.Processing;

// LLD §4.1 step 2 (CTZ-31b): stores the official metrics, the splits and the simplified route computed by the engine
// from what the cleaning kept. The route is stored complete; the privacy trim for other runners is applied on read
// (§7.2, MT-21). An activity shorter than anticheat.min_duration_s is processed without territory (MT-05)
public sealed class MetricsStep(SystemSettingsReader settings) : IActivityProcessingStep
{
    public const string TooShortReason = "too_short";
    private const int _wgs84Srid = 4326;

    private static readonly GeometryFactory _geometry = NtsGeometryServices.Instance.CreateGeometryFactory(_wgs84Srid);

    private static readonly string[] _settingKeys =
    [
        SystemSettingsReader.ActivityAutoPauseSpeedMps,
        SystemSettingsReader.AntiCheatMinDurationS,
        SystemSettingsReader.CaloriesDefaultWeightKg,
    ];

    public int Order => 200;

    public async Task RunAsync(ActivityProcessingContext context, CancellationToken cancellationToken)
    {
        var cleaning = context.Cleaning
            ?? throw new InvalidOperationException("The metrics stage needs the result of the cleaning stage.");
        var activity = context.Activity;
        var values = await settings.GetNumbersAsync(_settingKeys, cancellationToken);
        var weightKg = await context.Db.Profiles.AsNoTracking()
            .Where(p => p.UserId == activity.UserId)
            .Select(p => p.WeightKg)
            .SingleOrDefaultAsync(cancellationToken);
        var options = new MetricsOptions(
            IsWalk: activity.Type == ActivityType.Walk,
            WeightKg: (double?)weightKg ?? values[SystemSettingsReader.CaloriesDefaultWeightKg],
            MovingSpeedMps: values[SystemSettingsReader.ActivityAutoPauseSpeedMps]);

        var metrics = MetricsCalculator.Calculate(context.Points, cleaning, activity.StartedAt, activity.EndedAt, options);
        Apply(activity, metrics);
        await ReplaceSplitsAsync(context, metrics.Splits, cancellationToken);

        if (metrics.ElapsedTimeS < values[SystemSettingsReader.AntiCheatMinDurationS])
        {
            context.IsEligibleForTerritory = false;
            if (activity.Status == ActivityStatus.Pending)
            {
                activity.StatusReason = TooShortReason;
            }
        }
    }

    private static void Apply(Activity activity, ActivityMetrics metrics)
    {
        activity.ElapsedTimeS = (int)Math.Round(metrics.ElapsedTimeS);
        activity.MovingTimeS = (int)Math.Round(metrics.MovingTimeS);
        activity.DistanceM = Math.Round((decimal)metrics.DistanceM, 2);
        activity.AvgPaceSPerKm = metrics.AvgPaceSPerKm is { } pace ? (int)Math.Round(pace) : null;
        activity.MaxSpeedMps = Math.Round((decimal)metrics.MaxSpeedMps, 2);
        activity.ElevationGainM = Math.Round((decimal)metrics.ElevationGainM, 1);
        activity.ElevationLossM = Math.Round((decimal)metrics.ElevationLossM, 1);
        activity.Calories = (int)Math.Round(metrics.Calories);
        activity.Route = metrics.Route.Count >= 2
            ? _geometry.CreateLineString([.. metrics.Route.Select(p => new Coordinate(p.Lon, p.Lat))])
            : null;
    }

    private static async Task ReplaceSplitsAsync(ActivityProcessingContext context, IReadOnlyList<SplitResult> splits, CancellationToken cancellationToken)
    {
        var activityId = context.Activity.Id;
        await context.Db.Splits.Where(s => s.ActivityId == activityId).ExecuteDeleteAsync(cancellationToken);
        context.Db.Splits.AddRange(splits.Select(split => new Split
        {
            ActivityId = activityId,
            Number = (short)split.Number,
            DistanceM = Math.Round((decimal)split.DistanceM, 2),
            DurationS = (int)Math.Round(split.DurationS),
            PaceSPerKm = (int)Math.Round(split.PaceSPerKm),
            ElevationM = split.ElevationM is { } elevation ? Math.Round((decimal)elevation, 1) : null,
        }));
    }
}
