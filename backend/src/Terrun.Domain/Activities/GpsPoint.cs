namespace Terrun.Domain.Activities;

// LLD §2.3 (M3): the raw points as the phone recorded them. The segment changes on every pause, so the processor never
// adds distance between the end of one segment and the start of the next (MT-14)
public sealed class GpsPoint
{
    public Guid ActivityId { get; set; }

    public int Seq { get; set; }

    public short Segment { get; set; }

    public DateTimeOffset RecordedAt { get; set; }

    public double Lat { get; set; }

    public double Lon { get; set; }

    public float? AltitudeM { get; set; }

    public float? AccuracyM { get; set; }

    public float? SpeedMps { get; set; }

    public bool IsDiscarded { get; set; }

    public DiscardReason? DiscardReason { get; set; }
}
