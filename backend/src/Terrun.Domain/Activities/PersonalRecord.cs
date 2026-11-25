namespace Terrun.Domain.Activities;

// LLD §2.3 (M3): only the current record of each type is kept
public sealed class PersonalRecord
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public RecordType Type { get; set; }

    // Seconds for a distance record, meters for the longest activity
    public decimal Value { get; set; }

    public Guid ActivityId { get; set; }

    public DateTimeOffset AchievedAt { get; set; }
}
