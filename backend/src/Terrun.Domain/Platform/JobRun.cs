namespace Terrun.Domain.Platform;

// LLD §6 (D-25): one row per scheduled job. A job is due when its cron has an occurrence after the last success, so a
// run that GitHub delays or skips is recovered by the next one. last_finished_at tells a running job from one that ended
public sealed class JobRun
{
    public required string Name { get; set; }

    public DateTimeOffset? LastStartedAt { get; set; }

    public DateTimeOffset? LastFinishedAt { get; set; }

    public DateTimeOffset? LastSucceededAt { get; set; }

    public string? LastError { get; set; }
}
