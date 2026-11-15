namespace Terrun.Domain.Profiles;

// Denormalized totals kept up to date by the activity processor (LLD §2.3)
public sealed class UserStats
{
    public Guid UserId { get; set; }

    public long TotalDistanceM { get; set; }

    public long TotalTimeS { get; set; }

    public int TotalActivities { get; set; }

    public int ZonesConquered { get; set; }

    public int ZonesOwned { get; set; }

    public int TotalSteals { get; set; }

    public int TotalXp { get; set; }

    public short Level { get; set; } = 1;

    public short CurrentStreakWeeks { get; set; }

    public short LongestStreakWeeks { get; set; }

    public DateTimeOffset? LastActivityAt { get; set; }
}
