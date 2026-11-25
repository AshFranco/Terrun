using NetTopologySuite.Geometries;
using Terrun.Domain.Common;

namespace Terrun.Domain.Activities;

// LLD §2.3 (M3). The upload fills the identity, the times and the points; the processor (CTZ-31a/31b) fills the
// official metrics, the route and the results, so those stay null until then
public sealed class Activity
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    // Generated on the phone: sending the same activity again never duplicates it (MT-09)
    public Guid ClientId { get; set; }

    public ActivityType Type { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public Visibility Visibility { get; set; }

    public ActivityStatus Status { get; set; } = ActivityStatus.Pending;

    public string? StatusReason { get; set; }

    public DateTimeOffset StartedAt { get; set; }

    public DateTimeOffset EndedAt { get; set; }

    public int? ElapsedTimeS { get; set; }

    public int? MovingTimeS { get; set; }

    public decimal? DistanceM { get; set; }

    public int? AvgPaceSPerKm { get; set; }

    public decimal? MaxSpeedMps { get; set; }

    public decimal? ElevationGainM { get; set; }

    public decimal? ElevationLossM { get; set; }

    public int? Calories { get; set; }

    public LineString? Route { get; set; }

    public Guid? ShoeId { get; set; }

    public short NewZones { get; set; }

    public int XpEarned { get; set; }

    public short Attempts { get; set; }

    public string? Device { get; set; }

    public string? AppVersion { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? ProcessedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
