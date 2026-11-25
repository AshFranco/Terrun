namespace Terrun.Api.Jobs;

// LLD §6 and §8: Jobs__Key is a secret shared with the GitHub Actions workflow. Without it the trigger endpoint refuses
// every call instead of running jobs for anyone
public sealed class JobsOptions
{
    public const string SectionName = "Jobs";
    public const int MinimumKeyLength = 32;

    public string? Key { get; set; }

    // The game days and weeks (nightly ownership, streaks) follow the time of the Dominican Republic
    public string TimeZone { get; set; } = Modules.Platform.GameCalendar.TimeZoneId;

    public bool HasKey => Key is { Length: >= MinimumKeyLength };
}
