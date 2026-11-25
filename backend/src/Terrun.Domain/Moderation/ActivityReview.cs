namespace Terrun.Domain.Moderation;

// LLD §2.3 (M12): an activity the anti-cheat sent to human review (§4.1 step 3). Moderators resolve it from /admin/reviews
public sealed class ActivityReview
{
    public Guid Id { get; set; }

    public Guid ActivityId { get; set; }

    // JSON array of { rule, value, limit }, with the stable rule names of the engine (AntiCheatJudge)
    public required string ViolatedRules { get; set; }

    public ReviewStatus Status { get; set; } = ReviewStatus.Pending;

    public Guid? ReviewerId { get; set; }

    public string? Note { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? ResolvedAt { get; set; }
}
