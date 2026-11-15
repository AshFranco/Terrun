using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Terrun.Infrastructure.Persistence;

public static class DatabaseMigrator
{
    public const string MigrateOnStartupKey = "Database:MigrateOnStartup";

    // Applies pending migrations; EF Core takes a database lock, so two instances starting together do not collide
    public static async Task MigrateDatabaseAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<TerrunDbContext>();
        await db.Database.MigrateAsync(cancellationToken);
    }
}
