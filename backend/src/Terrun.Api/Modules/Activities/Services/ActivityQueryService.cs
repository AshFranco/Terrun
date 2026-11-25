using Microsoft.EntityFrameworkCore;
using Terrun.Api.Modules.Activities.Contracts;
using Terrun.Api.Modules.Platform;
using Terrun.Api.Modules.Platform.Services;
using Terrun.Domain.Activities;
using Terrun.Domain.Errors;
using Terrun.Engine.Tracks;
using Terrun.Infrastructure.Persistence;

namespace Terrun.Api.Modules.Activities.Services;

// RF-18 (CTZ-32): what P-09, P-10, P-11 and P-15 read. A missing activity and one the caller cannot see answer the same
// 404, so an id reveals nothing (LLD §3.4)
public sealed class ActivityQueryService(TerrunDbContext db, SystemSettingsReader settings, TimeProvider clock)
{
    public async Task<ActivityResponse> GetAsync(Guid viewerId, Guid activityId, CancellationToken cancellationToken)
    {
        var activity = await (
            from a in db.Activities.AsNoTracking().Where(ActivityVisibility.VisibleTo(viewerId))
            join p in db.Profiles.AsNoTracking() on a.UserId equals p.UserId
            where a.Id == activityId
            select new ActivityResponse(
                a.Id,
                new ActivityOwnerResponse(a.UserId, p.DisplayName, p.Handle),
                a.UserId == viewerId,
                a.Type,
                a.Title,
                a.Description,
                a.Visibility,
                a.Status,
                a.StatusReason,
                a.StartedAt,
                a.EndedAt,
                a.ElapsedTimeS,
                a.MovingTimeS,
                a.DistanceM,
                a.AvgPaceSPerKm,
                a.MaxSpeedMps,
                a.ElevationGainM,
                a.ElevationLossM,
                a.Calories,
                a.NewZones,
                a.XpEarned,
                a.UserId == viewerId ? a.ShoeId : null,
                a.UserId == viewerId ? a.Device : null,
                a.UserId == viewerId ? a.AppVersion : null,
                a.CreatedAt,
                a.ProcessedAt))
            .SingleOrDefaultAsync(cancellationToken);
        return activity ?? throw NotFound();
    }

    // LLD §7.2 and D-29 (MT-21): the owner gets the complete route; anyone else gets it without the first and the last
    // privacy.route_trim_m meters, or no geometry when it is too short
    public async Task<RouteResponse> GetRouteAsync(Guid viewerId, Guid activityId, CancellationToken cancellationToken)
    {
        var activity = await db.Activities.AsNoTracking()
            .Where(ActivityVisibility.VisibleTo(viewerId))
            .Where(a => a.Id == activityId)
            .Select(a => new { a.UserId, a.Route })
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw NotFound();
        var trimM = await settings.GetAsync<int>(SystemSettingsReader.PrivacyRouteTrimM, cancellationToken);
        if (activity.Route is null)
        {
            return new RouteResponse(RouteResponse.FeatureType, null, new RouteProperties(IsTrimmed: false, trimM));
        }

        IReadOnlyList<GeoPoint> points = [.. activity.Route.Coordinates.Select(c => new GeoPoint(c.Y, c.X))];
        var isOwner = activity.UserId == viewerId;
        if (!isOwner)
        {
            points = RouteTrimmer.TrimForOthers(points, trimM);
        }

        var geometry = points.Count >= 2 ? new RouteGeometry(RouteGeometry.LineStringType, [.. points.Select(p => new[] { p.Lon, p.Lat })]) : null;
        return new RouteResponse(RouteResponse.FeatureType, geometry, new RouteProperties(IsTrimmed: !isOwner, trimM));
    }

    public async Task<IReadOnlyList<SplitResponse>> GetSplitsAsync(Guid viewerId, Guid activityId, CancellationToken cancellationToken)
    {
        if (!await db.Activities.Where(ActivityVisibility.VisibleTo(viewerId)).AnyAsync(a => a.Id == activityId, cancellationToken))
        {
            throw NotFound();
        }

        return await db.Splits.AsNoTracking()
            .Where(s => s.ActivityId == activityId)
            .OrderBy(s => s.Number)
            .Select(s => new SplitResponse(s.Number, s.DistanceM, s.DurationS, s.PaceSPerKm, s.ElevationM))
            .ToListAsync(cancellationToken);
    }

    // P-10: the runner's own history, in any state, newest first
    public async Task<ActivityPage> ListMineAsync(Guid userId, ActivityListQuery query, CancellationToken cancellationToken)
    {
        var activities = db.Activities.AsNoTracking().Where(a => a.UserId == userId);
        if (query.Type is { } type)
        {
            activities = activities.Where(a => a.Type == type);
        }

        if (query.From is { } from)
        {
            activities = activities.Where(a => a.StartedAt >= from);
        }

        if (query.To is { } to)
        {
            activities = activities.Where(a => a.StartedAt < to);
        }

        if (query.Cursor is not null && ActivityCursor.TryParse(query.Cursor, out var cursor))
        {
            activities = activities.Where(a => a.StartedAt < cursor.StartedAt || (a.StartedAt == cursor.StartedAt && a.Id < cursor.Id));
        }

        var page = await activities
            .OrderByDescending(a => a.StartedAt).ThenByDescending(a => a.Id)
            .Take(query.Limit + 1)
            .Select(a => new ActivityListItem(
                a.Id, a.Type, a.Title, a.Visibility, a.Status, a.StatusReason, a.StartedAt, a.EndedAt,
                a.DistanceM, a.MovingTimeS, a.AvgPaceSPerKm, a.ElevationGainM, a.Calories, a.NewZones))
            .ToListAsync(cancellationToken);

        var hasMore = page.Count > query.Limit;
        var items = hasMore ? page[..query.Limit] : page;
        return new ActivityPage(items, hasMore ? new ActivityCursor(items[^1].StartedAt, items[^1].Id).Encode() : null);
    }

    // P-15 and P-29: totals of the processed activities of the current period and of the one before, in the calendar of
    // the game (AST). In review activities do not count, the same as in user_stats (D-37)
    public async Task<SummaryResponse> SummaryAsync(Guid userId, SummaryPeriod period, CancellationToken cancellationToken)
    {
        var (currentFrom, currentTo, previousFrom) = Bounds(period, clock.GetUtcNow());
        var current = await TotalsAsync(userId, currentFrom, currentTo, cancellationToken);
        var previous = await TotalsAsync(userId, previousFrom, currentFrom, cancellationToken);
        return new SummaryResponse(period, current, previous);
    }

    private async Task<PeriodTotals> TotalsAsync(Guid userId, DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken)
    {
        var totals = await db.Activities.AsNoTracking()
            .Where(a => a.UserId == userId && a.Status == ActivityStatus.Processed && a.StartedAt >= from && a.StartedAt < to)
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Count = g.Count(),
                Distance = g.Sum(a => a.DistanceM ?? 0),
                Moving = g.Sum(a => (long)(a.MovingTimeS ?? 0)),
                Gain = g.Sum(a => a.ElevationGainM ?? 0),
                Calories = g.Sum(a => (long)(a.Calories ?? 0)),
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (totals is null)
        {
            return new PeriodTotals(from, to, 0, 0, 0, 0, 0, null);
        }

        int? pace = totals.Distance > 0 ? (int)Math.Round(totals.Moving / (totals.Distance / 1000m)) : null;
        return new PeriodTotals(from, to, totals.Count, totals.Distance, totals.Moving, totals.Gain, totals.Calories, pace);
    }

    // Weeks start on Monday at 00:00 AST; the previous period has the same kind and ends where the current one starts
    public static (DateTimeOffset CurrentFrom, DateTimeOffset CurrentTo, DateTimeOffset PreviousFrom) Bounds(SummaryPeriod period, DateTimeOffset now)
    {
        var zone = GameCalendar.TimeZone;
        var today = TimeZoneInfo.ConvertTime(now, zone).Date;
        var start = period switch
        {
            SummaryPeriod.Week => today.AddDays(-(((int)today.DayOfWeek + 6) % 7)),
            SummaryPeriod.Month => new DateTime(today.Year, today.Month, 1),
            _ => new DateTime(today.Year, 1, 1),
        };
        var (next, previous) = period switch
        {
            SummaryPeriod.Week => (start.AddDays(7), start.AddDays(-7)),
            SummaryPeriod.Month => (start.AddMonths(1), start.AddMonths(-1)),
            _ => (start.AddYears(1), start.AddYears(-1)),
        };
        return (AtZone(start, zone), AtZone(next, zone), AtZone(previous, zone));
    }

    private static DateTimeOffset AtZone(DateTime local, TimeZoneInfo zone) =>
        new DateTimeOffset(local, zone.GetUtcOffset(local)).ToUniversalTime();

    private static NotFoundException NotFound() => new("The activity does not exist or is not visible to you.");
}
