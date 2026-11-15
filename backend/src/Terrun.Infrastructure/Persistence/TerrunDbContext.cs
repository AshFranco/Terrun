using Microsoft.EntityFrameworkCore;

namespace Terrun.Infrastructure.Persistence;

public sealed class TerrunDbContext(DbContextOptions<TerrunDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("postgis");
    }
}
