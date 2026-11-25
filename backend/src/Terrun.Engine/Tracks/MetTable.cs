namespace Terrun.Engine.Tracks;

// LLD §4.1: metabolic equivalents by speed (Compendium of Physical Activities), interpolated linearly between rows. A speed
// outside the table takes the nearest row (D-37). These are physiology constants, not game rules
public static class MetTable
{
    private static readonly (double SpeedKmh, double Met)[] _running =
        [(8.0, 8.3), (9.7, 9.8), (11.3, 11.0), (12.9, 11.8), (14.5, 12.8), (16.1, 14.5)];

    private static readonly (double SpeedKmh, double Met)[] _walking = [(5.0, 3.5), (6.4, 5.0)];

    public static double MetFor(double speedKmh, bool isWalk)
    {
        var table = isWalk ? _walking : _running;
        if (speedKmh <= table[0].SpeedKmh)
        {
            return table[0].Met;
        }

        if (speedKmh >= table[^1].SpeedKmh)
        {
            return table[^1].Met;
        }

        var upper = Array.FindIndex(table, row => row.SpeedKmh >= speedKmh);
        var (low, high) = (table[upper - 1], table[upper]);
        return low.Met + ((high.Met - low.Met) * (speedKmh - low.SpeedKmh) / (high.SpeedKmh - low.SpeedKmh));
    }
}
