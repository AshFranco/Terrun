using Microsoft.Extensions.DependencyInjection;
using Terrun.Api.Tests.Infrastructure;
using Terrun.Domain.Identity;
using Terrun.Infrastructure.Persistence;

namespace Terrun.Api.Tests.Persistence;

// One empty PostGIS per test run, migrated by the API itself on startup (the same path QA uses)
public sealed class MigratedDatabaseFixture : IAsyncLifetime
{
    private readonly PostgisFixture _postgis = new();
    private TerrunApiFactory? _factory;

    public async ValueTask InitializeAsync()
    {
        await _postgis.InitializeAsync();
        _factory = new TerrunApiFactory(_postgis.ConnectionString, migrateOnStartup: true);
        _ = _factory.Services;
    }

    public AsyncServiceScope CreateScope() =>
        (_factory ?? throw new InvalidOperationException("The fixture is not initialized.")).Services.CreateAsyncScope();

    public static TerrunDbContext Db(AsyncServiceScope scope) => scope.ServiceProvider.GetRequiredService<TerrunDbContext>();

    public static AppUser NewUser() => new()
    {
        Id = Guid.CreateVersion7(),
        Email = $"runner-{Guid.NewGuid():N}@example.test",
    };

    public async ValueTask DisposeAsync()
    {
        if (_factory is not null)
        {
            await _factory.DisposeAsync();
        }

        await _postgis.DisposeAsync();
    }
}

[CollectionDefinition(Name)]
public sealed class MigratedDatabaseCollection : ICollectionFixture<MigratedDatabaseFixture>
{
    public const string Name = "Migrated database";
}
