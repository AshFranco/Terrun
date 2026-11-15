using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Terrun.Infrastructure.Persistence;

// Used only by the dotnet-ef tool. Adding a migration does not connect; updating a database reads ConnectionStrings__Db
internal sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<TerrunDbContext>
{
    private const string _localWithoutPassword = "Host=localhost;Port=5432;Database=terrun;Username=terrun";

    public TerrunDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Db") ?? _localWithoutPassword;
        var options = new DbContextOptionsBuilder<TerrunDbContext>();
        TerrunDbContextOptions.Configure(options, connectionString);

        return new TerrunDbContext(options.Options);
    }
}
