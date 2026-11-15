using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Terrun.Infrastructure.Persistence;

namespace Terrun.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddDbContext<TerrunDbContext>(options => TerrunDbContextOptions.Configure(options, connectionString));

        return services;
    }
}
