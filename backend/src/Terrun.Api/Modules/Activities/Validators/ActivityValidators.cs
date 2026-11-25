using FluentValidation;
using Terrun.Api.Modules.Activities.Contracts;
using Terrun.Api.Modules.Identity.Validators;
using Terrun.Api.Modules.Platform.Services;

namespace Terrun.Api.Modules.Activities.Validators;

internal static class ActivityRules
{
    // A route needs at least two points
    public const int MinPoints = 2;

    // Tolerance for a phone clock slightly ahead of the server; technical, not a game rule
    public static readonly TimeSpan ClockSkew = TimeSpan.FromMinutes(5);

    public const double MaxLatitude = 90;
    public const double MaxLongitude = 180;
}

// LLD §3.3 and D-34. The point count against activity.max_points is checked by the service, because it answers 413,
// not 400. The points are checked in one pass that reports the first wrong point, so a bad upload of 30,000 points
// does not produce 30,000 messages
internal sealed class CreateActivityRequestValidator : AbstractValidator<CreateActivityRequest>
{
    public CreateActivityRequestValidator(SystemSettingsReader settings, TimeProvider clock)
    {
        RuleFor(r => r.ClientId).NotEmpty();
        RuleFor(r => r.Type).NotNull().IsInEnum();
        RuleFor(r => r.Device).MaximumLength(AuthRules.DeviceMaxLength);
        RuleFor(r => r.AppVersion).MaximumLength(AuthRules.AppVersionMaxLength);

        RuleFor(r => r.EndedAt)
            .Cascade(CascadeMode.Stop)
            .GreaterThan(r => r.StartedAt).WithMessage("The activity must end after it starts.")
            .Must(endedAt => endedAt <= clock.GetUtcNow() + ActivityRules.ClockSkew).WithMessage("The activity cannot end in the future.")
            .MustAsync(async (request, endedAt, cancellationToken) =>
                endedAt - request.StartedAt <= TimeSpan.FromHours(
                    await settings.GetAsync<double>(SystemSettingsReader.ActivityMaxDurationHours, cancellationToken)))
            .WithMessage("The activity is longer than activity.max_duration_h.");

        RuleFor(r => r.Points)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .Must(points => points!.Count >= ActivityRules.MinPoints).WithMessage($"An activity needs at least {ActivityRules.MinPoints} points.")
            .Custom((points, context) =>
            {
                if (FirstInvalidPoint(points!, context.InstanceToValidate) is { } error)
                {
                    context.AddFailure(error);
                }
            });
    }

    private static string? FirstInvalidPoint(IReadOnlyList<GpsPointRequest> points, CreateActivityRequest request)
    {
        for (var i = 0; i < points.Count; i++)
        {
            var point = points[i];
            var previous = i > 0 ? points[i - 1] : null;
            var problem = PointProblem(point, previous, request);
            if (problem is not null)
            {
                return $"Point {i} (seq {point.Seq}): {problem}";
            }
        }

        return null;
    }

    // seq orders the points and is the primary key with the activity, so it must grow; the segment only grows because a
    // pause opens a new one (LLD §5.1)
    private static string? PointProblem(GpsPointRequest point, GpsPointRequest? previous, CreateActivityRequest request) => point switch
    {
        { Lat: null } or { Lon: null } => "lat and lon are required.",
        { Lat: < -ActivityRules.MaxLatitude or > ActivityRules.MaxLatitude } => "lat must be between -90 and 90.",
        { Lon: < -ActivityRules.MaxLongitude or > ActivityRules.MaxLongitude } => "lon must be between -180 and 180.",
        { Seq: < 0 } => "seq cannot be negative.",
        { Segment: < 0 } => "segment cannot be negative.",
        { AccuracyM: < 0 } => "accuracyM cannot be negative.",
        { SpeedMps: < 0 } => "speedMps cannot be negative.",
        _ when previous is not null && point.Seq <= previous.Seq => "seq must increase.",
        _ when previous is not null && point.Segment < previous.Segment => "segment cannot go back.",
        _ when point.RecordedAt < request.StartedAt || point.RecordedAt > request.EndedAt => "recordedAt must be between startedAt and endedAt.",
        _ => null,
    };
}
