using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Terrun.Api.Modules.Platform.Services;
using Terrun.Domain.Activities;
using Terrun.Domain.Moderation;
using Terrun.Engine.Tracks;

namespace Terrun.Api.Modules.Activities.Processing;

// LLD §4.1 steps 1 and 3 (CTZ-31a): cleans the GPS signal with the engine, marks the discarded points and, when an
// anti-cheat rule is violated, leaves the activity in review with an activity_review for the moderators (MT-04).
// Registered together with the metrics stage of CTZ-31b, so no activity is ever processed without its metrics (D-36)
public sealed class CleaningStep(SystemSettingsReader settings) : IActivityProcessingStep
{
    public const string InReviewReason = "anticheat";

    private static readonly string[] _ruleKeys =
    [
        SystemSettingsReader.AntiCheatMaxAccuracyM,
        SystemSettingsReader.AntiCheatMaxJumpM,
        SystemSettingsReader.AntiCheatJumpWindowS,
        SystemSettingsReader.AntiCheatMaxSpeedKmh,
        SystemSettingsReader.ActivityAutoPauseSpeedMps,
        SystemSettingsReader.AntiCheatMinPaceSPerKm,
        SystemSettingsReader.AntiCheatMaxInvalidDistancePct,
    ];

    public int Order => 100;

    public async Task RunAsync(ActivityProcessingContext context, CancellationToken cancellationToken)
    {
        var rules = await LoadRulesAsync(cancellationToken);
        var activity = context.Activity;
        var points = await context.Db.GpsPoints.AsNoTracking()
            .Where(p => p.ActivityId == activity.Id)
            .Select(p => new TrackPoint(p.Seq, p.Segment, p.RecordedAt, p.Lat, p.Lon, p.AccuracyM, p.AltitudeM))
            .ToListAsync(cancellationToken);

        context.Points = points;
        var cleaning = TrackCleaner.Clean(points, rules);
        context.Cleaning = cleaning;
        await MarkDiscardedAsync(context, cleaning.Discarded, cancellationToken);

        var violations = AntiCheatJudge.Judge(cleaning, rules);
        if (violations.Count > 0)
        {
            activity.Status = ActivityStatus.InReview;
            activity.StatusReason = InReviewReason;
            context.IsEligibleForTerritory = false;
            context.Db.ActivityReviews.Add(new ActivityReview
            {
                Id = Guid.CreateVersion7(),
                ActivityId = activity.Id,
                ViolatedRules = JsonSerializer.Serialize(violations, JsonSerializerOptions.Web),
            });
        }
    }

    private async Task<CleaningRules> LoadRulesAsync(CancellationToken cancellationToken)
    {
        var values = await settings.GetNumbersAsync(_ruleKeys, cancellationToken);
        return new CleaningRules(
            MaxAccuracyM: values[SystemSettingsReader.AntiCheatMaxAccuracyM],
            MaxJumpM: values[SystemSettingsReader.AntiCheatMaxJumpM],
            JumpWindowS: values[SystemSettingsReader.AntiCheatJumpWindowS],
            MaxSpeedKmh: values[SystemSettingsReader.AntiCheatMaxSpeedKmh],
            MovingSpeedMps: values[SystemSettingsReader.ActivityAutoPauseSpeedMps],
            MinPaceSPerKm: values[SystemSettingsReader.AntiCheatMinPaceSPerKm],
            MaxInvalidDistancePct: values[SystemSettingsReader.AntiCheatMaxInvalidDistancePct]);
    }

    // One UPDATE per reason, inside the processing transaction; reprocessing an activity first clears the old marks
    private static async Task MarkDiscardedAsync(ActivityProcessingContext context, IReadOnlyList<DiscardedPoint> discarded, CancellationToken cancellationToken)
    {
        var activityId = context.Activity.Id;
        await context.Db.GpsPoints.Where(p => p.ActivityId == activityId && p.IsDiscarded)
            .ExecuteUpdateAsync(set => set.SetProperty(p => p.IsDiscarded, false).SetProperty(p => p.DiscardReason, (DiscardReason?)null), cancellationToken);

        foreach (var group in discarded.GroupBy(d => d.Reason))
        {
            var seqs = group.Select(d => d.Seq).ToList();
            var reason = ToDomain(group.Key);
            await context.Db.GpsPoints.Where(p => p.ActivityId == activityId && seqs.Contains(p.Seq))
                .ExecuteUpdateAsync(set => set.SetProperty(p => p.IsDiscarded, true).SetProperty(p => p.DiscardReason, reason), cancellationToken);
        }
    }

    private static DiscardReason ToDomain(PointDiscardReason reason) => reason switch
    {
        PointDiscardReason.Accuracy => DiscardReason.Accuracy,
        PointDiscardReason.Timestamp => DiscardReason.Timestamp,
        PointDiscardReason.Jump => DiscardReason.Jump,
        _ => throw new ArgumentOutOfRangeException(nameof(reason), reason, null),
    };
}
