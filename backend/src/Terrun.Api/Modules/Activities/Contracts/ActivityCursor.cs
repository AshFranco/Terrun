using System.Buffers.Text;
using System.Globalization;
using System.Text;

namespace Terrun.Api.Modules.Activities.Contracts;

// LLD §3.1: an opaque keyset cursor (startedAt, id) of the last item of a page; stable when activities are added
public readonly record struct ActivityCursor(DateTimeOffset StartedAt, Guid Id)
{
    public string Encode() =>
        Base64Url.EncodeToString(Encoding.UTF8.GetBytes($"{StartedAt.UtcTicks.ToString(CultureInfo.InvariantCulture)}:{Id:N}"));

    public static bool TryParse(string value, out ActivityCursor cursor)
    {
        cursor = default;
        try
        {
            var parts = Encoding.UTF8.GetString(Base64Url.DecodeFromChars(value)).Split(':');
            if (parts.Length == 2
                && long.TryParse(parts[0], NumberStyles.None, CultureInfo.InvariantCulture, out var ticks)
                && ticks >= 0 && ticks <= DateTime.MaxValue.Ticks
                && Guid.TryParseExact(parts[1], "N", out var id))
            {
                cursor = new ActivityCursor(new DateTimeOffset(ticks, TimeSpan.Zero), id);
                return true;
            }
        }
        catch (FormatException)
        {
            return false;
        }

        return false;
    }
}
