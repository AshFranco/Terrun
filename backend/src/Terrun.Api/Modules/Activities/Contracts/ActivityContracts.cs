using Terrun.Domain.Activities;

namespace Terrun.Api.Modules.Activities.Contracts;

// LLD §3.3 (POST /activities). Type, latitude and longitude are nullable only so that a missing value is a validation
// error instead of silently becoming "run" or the point (0, 0)

public sealed record CreateActivityRequest(
    Guid ClientId,
    ActivityType? Type,
    DateTimeOffset StartedAt,
    DateTimeOffset EndedAt,
    IReadOnlyList<GpsPointRequest>? Points,
    Guid? ShoeId = null,
    string? Device = null,
    string? AppVersion = null);

public sealed record GpsPointRequest(
    int Seq,
    short Segment,
    DateTimeOffset RecordedAt,
    double? Lat,
    double? Lon,
    float? AltitudeM = null,
    float? AccuracyM = null,
    float? SpeedMps = null);

// 202 for a new upload and 200 with the same body when the clientId already exists (MT-09). The app polls
// GET /activities/{id} with this id until the status leaves pending (CTZ-34)
public sealed record ActivityUploadResponse(Guid Id, ActivityStatus Status);
