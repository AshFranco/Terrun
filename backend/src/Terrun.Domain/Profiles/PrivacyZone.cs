using NetTopologySuite.Geometries;

namespace Terrun.Domain.Profiles;

public sealed class PrivacyZone
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public required string Name { get; set; }

    public required Point Center { get; set; }

    public short RadiusM { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
