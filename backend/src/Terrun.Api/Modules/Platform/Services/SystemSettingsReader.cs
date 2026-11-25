using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Terrun.Infrastructure.Persistence;

namespace Terrun.Api.Modules.Platform.Services;

// LLD §2.4 and standards §3.1: game rules and limits are read from system_setting, never written as literals. A missing
// key is a broken deployment (the seed creates every key), so it fails loudly instead of guessing a value
public sealed class SystemSettingsReader(TerrunDbContext db)
{
    public const string ActivityMaxPoints = "activity.max_points";
    public const string ActivityMaxDurationHours = "activity.max_duration_h";

    public async Task<T> GetAsync<T>(string key, CancellationToken cancellationToken)
    {
        var value = await db.SystemSettings.AsNoTracking()
            .Where(s => s.Key == key)
            .Select(s => s.Value)
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException($"The system setting '{key}' does not exist.");

        return JsonSerializer.Deserialize<T>(value)
            ?? throw new InvalidOperationException($"The system setting '{key}' has no value.");
    }
}
