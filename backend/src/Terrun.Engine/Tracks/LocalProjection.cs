namespace Terrun.Engine.Tracks;

// Equirectangular projection to meters around a reference point: accurate to centimeters over the few kilometers of a run,
// and what the 5 m tolerance of the route simplification is measured in
internal readonly record struct LocalProjection(double Lat0, double Lon0)
{
    private double MetersPerDegreeLat => Geo.EarthRadiusM * Math.PI / 180;

    private double MetersPerDegreeLon => MetersPerDegreeLat * Math.Cos(Lat0 * Math.PI / 180);

    public (double X, double Y) ToMeters(GeoPoint point) =>
        ((point.Lon - Lon0) * MetersPerDegreeLon, (point.Lat - Lat0) * MetersPerDegreeLat);
}
