using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Terrun.Infrastructure.Persistence;

namespace Terrun.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        // Tests replace the clock; nothing reads DateTime.Now (standards §3.9)
        services.TryAddSingleton(TimeProvider.System);
        services.AddSingleton<TimestampsInterceptor>();
        services.AddDbContext<TerrunDbContext>((provider, options) => TerrunDbContextOptions
            .Configure(options, connectionString)
            .AddInterceptors(provider.GetRequiredService<TimestampsInterceptor>()));

        return services;
    }
}
