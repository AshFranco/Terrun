namespace Terrun.Domain.Equipment;

// LLD §2.3 (M4). Its endpoints are P1 (/me/shoes); the table exists from CTZ-30 because an activity can point to a shoe
public sealed class Shoe
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public required string Brand { get; set; }

    public required string Model { get; set; }

    public string? Nickname { get; set; }

    public int InitialDistanceM { get; set; }

    public long TotalDistanceM { get; set; }

    public short RetirementKm { get; set; }

    public bool IsDefault { get; set; }

    public DateTimeOffset? RetiredAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
