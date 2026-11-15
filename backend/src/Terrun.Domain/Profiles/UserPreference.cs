using Terrun.Domain.Common;

namespace Terrun.Domain.Profiles;

// LLD §2.3: a new runner starts with auto-pause, voice cues and every notification turned on
public sealed class UserPreference
{
    public Guid UserId { get; set; }

    public UnitSystem Units { get; set; } = UnitSystem.Metric;

    public Visibility DefaultVisibility { get; set; } = Visibility.Followers;

    public bool IsAutoPauseEnabled { get; set; } = true;

    public bool IsVoiceCuesEnabled { get; set; } = true;

    public decimal VoiceCueIntervalKm { get; set; } = 1m;

    public bool NotifyZoneStolen { get; set; } = true;

    public bool NotifyKudos { get; set; } = true;

    public bool NotifyComments { get; set; } = true;

    public bool NotifyFollowers { get; set; } = true;

    public bool NotifyChallenges { get; set; } = true;

    public DateTimeOffset UpdatedAt { get; set; }
}
