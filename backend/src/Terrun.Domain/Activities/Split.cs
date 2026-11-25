namespace Terrun.Domain.Activities;

// LLD §2.3 (M3): one row per kilometer; the last one can be partial
public sealed class Split
{
    public Guid ActivityId { get; set; }

    public short Number { get; set; }

    public decimal DistanceM { get; set; }

    public int DurationS { get; set; }

    public int PaceSPerKm { get; set; }

    public decimal? ElevationM { get; set; }
}
