using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Terrun.Infrastructure.Email;
using Terrun.Infrastructure.Persistence;
using Terrun.Infrastructure.Security;

namespace Terrun.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString, IConfiguration configuration)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        // Tests replace the clock; nothing reads DateTime.Now (standards §3.9)
        services.TryAddSingleton(TimeProvider.System);
        services.AddSingleton<TimestampsInterceptor>();
        services.AddDbContext<TerrunDbContext>((provider, options) => TerrunDbContextOptions
            .Configure(options, connectionString)
            .AddInterceptors(provider.GetRequiredService<TimestampsInterceptor>()));

        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddSingleton<ICommonPasswordList, CommonPasswordList>();

        var emailSection = configuration.GetSection(EmailOptions.SectionName);
        services.Configure<EmailOptions>(emailSection);
        if (emailSection.Get<EmailOptions>() is { IsSmtpConfigured: true })
        {
            services.AddScoped<IEmailSender, SmtpEmailSender>();
        }
        else
        {
            services.AddScoped<IEmailSender, LogEmailSender>();
        }

        return services;
    }
}
