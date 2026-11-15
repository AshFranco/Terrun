namespace Terrun.Domain.Profiles;

public sealed class Profile
{
    public Guid UserId { get; set; }

    public required string DisplayName { get; set; }

    public required string Handle { get; set; }

    public string? PhotoUrl { get; set; }

    public string? Bio { get; set; }

    public string? City { get; set; }

    public string? Country { get; set; }

    public DateOnly? BirthDate { get; set; }

    public Sex Sex { get; set; } = Sex.Undisclosed;

    public decimal? WeightKg { get; set; }

    public short? HeightCm { get; set; }

    public bool IsPrivate { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
