namespace Terrun.Domain.Platform;

// LLD §2.4: every game rule lives here, never as a literal in the code
public sealed class SystemSetting
{
    public required string Key { get; set; }

    // Raw JSON stored in a jsonb column
    public required string Value { get; set; }

    public required string Description { get; set; }

    public bool IsPublic { get; set; }

    public Guid? UpdatedBy { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
