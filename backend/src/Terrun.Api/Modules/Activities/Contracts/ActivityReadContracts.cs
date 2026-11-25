using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Mvc;
using Terrun.Domain.Activities;
using Terrun.Domain.Common;

namespace Terrun.Api.Modules.Activities.Contracts;

// LLD §3.2 (M3), CTZ-32. Metrics are null until the activity is processed (CTZ-31b). Device, app version and shoe are only
// returned to the owner

public sealed record ActivityOwnerResponse(Guid Id, string DisplayName, string Handle);

public sealed record ActivityResponse(
    Guid Id,
    ActivityOwnerResponse Owner,
    bool IsOwner,
    ActivityType Type,
    string? Title,
    string? Description,
    Visibility Visibility,
    ActivityStatus Status,
    string? StatusReason,
    DateTimeOffset StartedAt,
    DateTimeOffset EndedAt,
    int? ElapsedTimeS,
    int? MovingTimeS,
    decimal? DistanceM,
    int? AvgPaceSPerKm,
    decimal? MaxSpeedMps,
    decimal? ElevationGainM,
    decimal? ElevationLossM,
    int? Calories,
    short NewZones,
    int XpEarned,
    Guid? ShoeId,
    string? Device,
    string? AppVersion,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ProcessedAt);

public sealed record SplitResponse(short Number, decimal DistanceM, int DurationS, int PaceSPerKm, decimal? ElevationM);

// GeoJSON Feature (RFC 7946) with a LineString of [lon, lat]. Geometry is null while the activity is not processed, or
// for other runners when the route is 2 × privacy.route_trim_m or shorter (MT-21)
public sealed record RouteResponse(string Type, RouteGeometry? Geometry, RouteProperties Properties)
{
    public const string FeatureType = "Feature";
}

public sealed record RouteGeometry(string Type, IReadOnlyList<double[]> Coordinates)
{
    public const string LineStringType = "LineString";
}

public sealed record RouteProperties(bool IsTrimmed, int TrimM);

public sealed record ActivityListItem(
    Guid Id,
    ActivityType Type,
    string? Title,
    Visibility Visibility,
    ActivityStatus Status,
    string? StatusReason,
    DateTimeOffset StartedAt,
    DateTimeOffset EndedAt,
    decimal? DistanceM,
    int? MovingTimeS,
    int? AvgPaceSPerKm,
    decimal? ElevationGainM,
    int? Calories,
    short NewZones);

public sealed record ActivityPage(IReadOnlyList<ActivityListItem> Items, string? NextCursor);

// LLD §3.1: cursor pagination, newest first; from/to filter by startedAt (from inclusive, to exclusive)
public sealed class ActivityListQuery
{
    public const int DefaultLimit = 20;
    public const int MaxLimit = 50;

    // LLD §3.1: query parameters in camelCase, also in the OpenAPI the app types come from
    [FromQuery(Name = "type")]
    public ActivityType? Type { get; set; }

    [FromQuery(Name = "from")]
    public DateTimeOffset? From { get; set; }

    [FromQuery(Name = "to")]
    public DateTimeOffset? To { get; set; }

    [FromQuery(Name = "limit")]
    public int Limit { get; set; } = DefaultLimit;

    [FromQuery(Name = "cursor")]
    public string? Cursor { get; set; }
}

public enum SummaryPeriod
{
    Week,
    Month,
    Year,
}

public sealed record PeriodTotals(
    DateTimeOffset From,
    DateTimeOffset To,
    int Activities,
    decimal DistanceM,
    long MovingTimeS,
    decimal ElevationGainM,
    long Calories,
    int? AvgPaceSPerKm);

public sealed record SummaryResponse(SummaryPeriod Period, PeriodTotals Current, PeriodTotals Previous);

// JSON merge patch (RFC 7396), like PATCH /me/profile (D-32): a field left out stays, null clears title, description or
// shoe; type and visibility cannot be cleared. Changing the type of a processed activity recalculates its calories (D-38)
public sealed class UpdateActivityRequest
{
    private readonly HashSet<string> _sentFields = [];

    public string? Title { get; set => Track(out field, value); }

    public string? Description { get; set => Track(out field, value); }

    public ActivityType? Type { get; set => Track(out field, value); }

    public Visibility? Visibility { get; set => Track(out field, value); }

    public Guid? ShoeId { get; set => Track(out field, value); }

    public bool Has(string fieldName) => _sentFields.Contains(fieldName);

    private void Track<T>(out T field, T value, [CallerMemberName] string fieldName = "")
    {
        field = value;
        _sentFields.Add(fieldName);
    }
}
