using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Terrun.Infrastructure.Persistence;

namespace Terrun.Api.Tests.Infrastructure;

public sealed class TerrunApiFactory(string connectionString, bool migrateOnStartup = false) : WebApplicationFactory<Program>
{
    // Port 1 is closed on every machine, so the database check fails without waiting for a timeout
    public const string UnreachableDatabase = "Host=localhost;Port=1;Database=terrun;Username=terrun;Password=unused";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:Db", connectionString);
        builder.UseSetting(DatabaseMigrator.MigrateOnStartupKey, migrateOnStartup.ToString());
    }
}
