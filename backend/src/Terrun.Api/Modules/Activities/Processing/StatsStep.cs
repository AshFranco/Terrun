using Microsoft.EntityFrameworkCore;
using Terrun.Domain.Activities;

namespace Terrun.Api.Modules.Activities.Processing;

// LLD §4.1 step 5 (part of the progression, D-37): adds a processed activity to the runner's totals in user_stats, so
// GET /me shows them. An activity in review adds nothing until a moderator approves it. Runs once per activity, inside
// the processing transaction, so the totals are never counted twice. XP, level and streaks stay for CTZ-51
public sealed class StatsStep : IActivityProcessingStep
{
    public int Order => 900;

    public async Task RunAsync(ActivityProcessingContext context, CancellationToken cancellationToken)
    {
        var activity = context.Activity;
        if (activity.Status != ActivityStatus.Pending)
        {
            return;
        }

        var distanceM = (long)Math.Round(activity.DistanceM ?? 0);
        var movingTimeS = (long)(activity.MovingTimeS ?? 0);
        var endedAt = activity.EndedAt;
        await context.Db.UserStats.Where(s => s.UserId == activity.UserId).ExecuteUpdateAsync(
            set => set
                .SetProperty(s => s.TotalDistanceM, s => s.TotalDistanceM + distanceM)
                .SetProperty(s => s.TotalTimeS, s => s.TotalTimeS + movingTimeS)
                .SetProperty(s => s.TotalActivities, s => s.TotalActivities + 1)
                .SetProperty(s => s.LastActivityAt, s => s.LastActivityAt == null || s.LastActivityAt < endedAt ? endedAt : s.LastActivityAt),
            cancellationToken);
    }
}
