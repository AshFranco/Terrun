using Terrun.Api.Modules.Profiles.Services;

namespace Terrun.Api.Modules.Profiles;

public static class ProfilesModule
{
    public static IServiceCollection AddProfilesModule(this IServiceCollection services)
    {
        services.AddScoped<ProfileService>();

        return services;
    }
}
