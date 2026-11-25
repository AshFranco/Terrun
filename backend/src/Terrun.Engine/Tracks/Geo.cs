namespace Terrun.Engine.Tracks;

public static class Geo
{
    // Mean Earth radius (IUGG), in meters
    public const double EarthRadiusM = 6_371_008.8;

    // LLD §4.1: great-circle distance between two WGS84 points (MT-01)
    public static double HaversineM(double lat1, double lon1, double lat2, double lon2)
    {
        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);
        var a = (Math.Sin(dLat / 2) * Math.Sin(dLat / 2))
            + (Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) * Math.Sin(dLon / 2) * Math.Sin(dLon / 2));
        return 2 * EarthRadiusM * Math.Asin(Math.Min(1, Math.Sqrt(a)));
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180;
}
