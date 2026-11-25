using Microsoft.EntityFrameworkCore;
using Terrun.Api.Modules.Activities.Contracts;
using Terrun.Api.Modules.Activities.Processing;
using Terrun.Domain.Activities;
using Terrun.Domain.Errors;
using Terrun.Engine.Tracks;
using Terrun.Infrastructure.Persistence;

namespace Terrun.Api.Modules.Activities.Services;

// RF-18 (CTZ-32, P-12): only the owner edits or deletes an activity. Someone who can see it but is not the owner gets 403;
// someone who cannot even see it gets 404, the same as a missing activity (LLD §3.4, §7.2)
public sealed class ActivityEditService(TerrunDbContext db, ProcessingSettings processing, ActivityQueryService queries)
{
    public async Task<ActivityResponse> UpdateAsync(Guid viewerId, Guid activityId, UpdateActivityRequest request, CancellationToken cancellationToken)
    {
        var activity = await LoadOwnedAsync(viewerId, activityId, cancellationToken);

        if (request.Has(nameof(UpdateActivityRequest.Title)))
        {
            activity.Title = string.IsNullOrWhiteSpace(request.Title) ? null : request.Title.Trim();
        }

        if (request.Has(nameof(UpdateActivityRequest.Description)))
        {
            activity.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        }

        if (request.Has(nameof(UpdateActivityRequest.Visibility)))
        {
            activity.Visibility = request.Visibility!.Value;
        }

        if (request.Has(nameof(UpdateActivityRequest.ShoeId)))
        {
            await EnsureOwnShoeAsync(viewerId, request.ShoeId, cancellationToken);
            activity.ShoeId = request.ShoeId;
        }

        if (request.Has(nameof(UpdateActivityRequest.Type)) && request.Type != activity.Type)
        {
            activity.Type = request.Type!.Value;
            await RecalculateCaloriesAsync(activity, cancellationToken);
        }

        await db.SaveChangesAsync(cancellationToken);
        return await queries.GetAsync(viewerId, activityId, cancellationToken);
    }

    // LLD §4.5: the activity goes with its points, splits and review; a processed one leaves the runner's totals. The row
    // is locked first, so an activity being processed right now is deleted after its totals were added, never before
    public async Task DeleteAsync(Guid viewerId, Guid activityId, CancellationToken cancellationToken)
    {
        await LoadOwnedAsync(viewerId, activityId, cancellationToken);
        db.ChangeTracker.Clear();

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        var activity = await db.Activities
            .FromSql($"SELECT * FROM activity WHERE id = {activityId} FOR UPDATE")
            .AsNoTracking()
            .SingleAsync(cancellationToken);
        if (activity.Status == ActivityStatus.Processed)
        {
            await SubtractFromTotalsAsync(activity, cancellationToken);
        }

        await db.Activities.Where(a => a.Id == activityId).ExecuteDeleteAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    private async Task<Activity> LoadOwnedAsync(Guid viewerId, Guid activityId, CancellationToken cancellationToken)
    {
        var activity = await db.Activities
            .Where(ActivityVisibility.VisibleTo(viewerId))
            .SingleOrDefaultAsync(a => a.Id == activityId, cancellationToken)
            ?? throw new NotFoundException("The activity does not exist or is not visible to you.");
        return activity.UserId == viewerId
            ? activity
            : throw new ForbiddenException(ErrorCodes.Forbidden, "Only the owner can change this activity.");
    }

    private async Task EnsureOwnShoeAsync(Guid userId, Guid? shoeId, CancellationToken cancellationToken)
    {
        if (shoeId is { } id && !await db.Shoes.AnyAsync(s => s.Id == id && s.UserId == userId, cancellationToken))
        {
            throw new FieldValidationException("shoeId", "The shoe does not exist or is not yours.");
        }
    }

    // D-38: calories depend on the type (walking uses its own MET table); the rest of the metrics do not, so only they are
    // recalculated. An activity not processed yet gets them from the processor with the new type
    private async Task RecalculateCaloriesAsync(Activity activity, CancellationToken cancellationToken)
    {
        if (activity.Calories is null)
        {
            return;
        }

        var points = await processing.PointsAsync(activity.Id, cancellationToken);
        var cleaning = TrackCleaner.Clean(points, await processing.CleaningRulesAsync(cancellationToken));
        var (options, _) = await processing.MetricsOptionsAsync(activity, cancellationToken);
        var metrics = MetricsCalculator.Calculate(points, cleaning, activity.StartedAt, activity.EndedAt, options);
        activity.Calories = (int)Math.Round(metrics.Calories);
    }

    private Task SubtractFromTotalsAsync(Activity activity, CancellationToken cancellationToken)
    {
        var (userId, activityId) = (activity.UserId, activity.Id);
        var distanceM = (long)Math.Round(activity.DistanceM ?? 0);
        var movingTimeS = (long)(activity.MovingTimeS ?? 0);
        return db.UserStats.Where(s => s.UserId == userId).ExecuteUpdateAsync(
            set => set
                .SetProperty(s => s.TotalDistanceM, s => s.TotalDistanceM - distanceM)
                .SetProperty(s => s.TotalTimeS, s => s.TotalTimeS - movingTimeS)
                .SetProperty(s => s.TotalActivities, s => s.TotalActivities - 1)
                .SetProperty(s => s.LastActivityAt, s => db.Activities
                    .Where(a => a.UserId == userId && a.Id != activityId && a.Status == ActivityStatus.Processed)
                    .Max(a => (DateTimeOffset?)a.EndedAt)),
            cancellationToken);
    }
}
