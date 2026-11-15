using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Terrun.Infrastructure.Persistence;

namespace Terrun.Api.Tests.Infrastructure;

public sealed class TerrunApiFactory(string connectionString, bool migrateOnStartup = false) : WebApplicationFactory<Program>
{
    // Port 1 is closed on every machine, so the database check fails without waiting for a timeout
    public const string UnreachableDatabase = "Host=localhost;Port=1;Database=terrun;Username=terrun;Password=unused";

    public string EnvironmentName { get; init; } = "Development";

    public Dictionary<string, string?> Settings { get; init; } = [];

    public Action<IServiceCollection>? TestServices { get; init; }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(EnvironmentName);
        builder.UseSetting("ConnectionStrings:Db", connectionString);
        builder.UseSetting(DatabaseMigrator.MigrateOnStartupKey, migrateOnStartup.ToString());
        foreach (var (key, value) in Settings)
        {
            builder.UseSetting(key, value);
        }

        if (TestServices is not null)
        {
            builder.ConfigureTestServices(TestServices);
        }
    }
}
