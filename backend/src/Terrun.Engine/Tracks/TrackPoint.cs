namespace Terrun.Engine.Tracks;

// A raw GPS point as the phone recorded it (LLD §2.3 gps_point)
public sealed record TrackPoint(int Seq, int Segment, DateTimeOffset RecordedAt, double Lat, double Lon, double? AccuracyM);
