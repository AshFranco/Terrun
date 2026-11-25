using System.Runtime.CompilerServices;
using Terrun.Domain.Common;
using Terrun.Domain.Identity;
using Terrun.Domain.Profiles;

namespace Terrun.Api.Modules.Profiles.Contracts;

// LLD §3.2 (M2): GET /me returns everything P-15 and P-17 need in one call; PATCH /me/profile answers with the same shape
// so the app refreshes its cache without a second request

public sealed record MeResponse(
    MeUserResponse User,
    MeProfileResponse Profile,
    MePreferencesResponse Preferences,
    MeStatsResponse Stats);

public sealed record MeUserResponse(Guid Id, string Email, bool IsEmailVerified, UserRole Role, DateTimeOffset CreatedAt);

public sealed record MeProfileResponse(
    string DisplayName,
    string Handle,
    string? PhotoUrl,
    string? Bio,
    string? City,
    string? Country,
    DateOnly? BirthDate,
    Sex Sex,
    decimal? WeightKg,
    short? HeightCm,
    bool IsPrivate,
    DateTimeOffset UpdatedAt);

public sealed record MePreferencesResponse(
    UnitSystem Units,
    Visibility DefaultVisibility,
    bool IsAutoPauseEnabled,
    bool IsVoiceCuesEnabled,
    decimal VoiceCueIntervalKm,
    bool NotifyZoneStolen,
    bool NotifyKudos,
    bool NotifyComments,
    bool NotifyFollowers,
    bool NotifyChallenges);

public sealed record MeStatsResponse(
    long TotalDistanceM,
    long TotalTimeS,
    int TotalActivities,
    int ZonesConquered,
    int ZonesOwned,
    int TotalSteals,
    int TotalXp,
    short Level,
    short CurrentStreakWeeks,
    short LongestStreakWeeks,
    DateTimeOffset? LastActivityAt);

// JSON merge patch (RFC 7396): a field left out stays as it is, and an explicit null clears an optional field. The
// serializer only calls the setter of the fields the body carries, so each setter records that the field was sent.
// The photo is not here: it arrives through /me/photo/upload-url (P1, D-32)
public sealed class UpdateProfileRequest
{
    private readonly HashSet<string> _sentFields = [];

    public string? DisplayName { get; set => Track(out field, value); }

    public string? Handle { get; set => Track(out field, value); }

    public string? Bio { get; set => Track(out field, value); }

    public string? City { get; set => Track(out field, value); }

    // ISO 3166-1 alpha-2, such as "DO"
    public string? Country { get; set => Track(out field, value); }

    public DateOnly? BirthDate { get; set => Track(out field, value); }

    public Sex? Sex { get; set => Track(out field, value); }

    public decimal? WeightKg { get; set => Track(out field, value); }

    public short? HeightCm { get; set => Track(out field, value); }

    public bool? IsPrivate { get; set => Track(out field, value); }

    public bool Has(string fieldName) => _sentFields.Contains(fieldName);

    private void Track<T>(out T field, T value, [CallerMemberName] string fieldName = "")
    {
        field = value;
        _sentFields.Add(fieldName);
    }
}
