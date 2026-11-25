namespace Terrun.Api.Tests.Activities;

// Test data factory for POST /activities: a straight line of points on a made-up grid (standards §6: never the
// coordinates of anyone's home), with a pause halfway that opens segment 1
public sealed record ActivityUpload(Guid ClientId, DateTimeOffset StartedAt, DateTimeOffset EndedAt, IReadOnlyList<ActivityUpload.Point> Points)
{
    private const double _originLat = 10.0;
    private const double _originLon = -60.0;
    private const double _stepDegrees = 0.00005;

    public static ActivityUpload At(DateTimeOffset startedAt, int points, int everySeconds = 2)
    {
        var list = Enumerable.Range(0, points)
            .Select(i => new Point(
                Seq: i,
                Segment: (short)(i < points / 2 ? 0 : 1),
                RecordedAt: startedAt.AddSeconds(i * everySeconds),
                Lat: _originLat + (i * _stepDegrees),
                Lon: _originLon))
            .ToList();
        return new ActivityUpload(Guid.NewGuid(), startedAt, list[^1].RecordedAt.AddSeconds(1), list);
    }

    // A mutable JSON body, so a test can break one field at a time
    public Dictionary<string, object?> Body(Guid? shoeId = null) => new()
    {
        ["clientId"] = ClientId,
        ["type"] = "run",
        ["startedAt"] = StartedAt,
        ["endedAt"] = EndedAt,
        ["shoeId"] = shoeId,
        ["device"] = "Test phone",
        ["appVersion"] = "0.1.0",
        ["points"] = Points.Select(p => new Dictionary<string, object?>
        {
            ["seq"] = p.Seq,
            ["segment"] = p.Segment,
            ["recordedAt"] = p.RecordedAt,
            ["lat"] = p.Lat,
            ["lon"] = p.Lon,
            ["altitudeM"] = 20.5f,
            ["accuracyM"] = 5f,
            ["speedMps"] = 2.8f,
        }).ToList(),
    };

    public sealed record Point(int Seq, short Segment, DateTimeOffset RecordedAt, double Lat, double Lon);
}
