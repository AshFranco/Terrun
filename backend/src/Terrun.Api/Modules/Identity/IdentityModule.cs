using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Terrun.Api.Modules.Identity.Services;
using Terrun.Domain.Identity;
using Terrun.Infrastructure.Persistence;

namespace Terrun.Api.Modules.Identity;

public static class IdentityModule
{
    // Small tolerance for clocks that drift between the phone, Render and the token issuer
    private static readonly TimeSpan _clockSkew = TimeSpan.FromSeconds(30);

    public static WebApplicationBuilder AddIdentityModule(this WebApplicationBuilder builder)
    {
        var services = builder.Services;
        services.AddOptions<JwtOptions>()
            .Bind(builder.Configuration.GetSection(JwtOptions.SectionName))
            .Validate(jwt => jwt.IsValid, $"Jwt:Key must have at least {JwtOptions.MinimumKeyBytes} bytes and Jwt:Issuer/Audience are required.")
            .ValidateOnStart();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtOptions>>((bearer, jwt) => ConfigureBearer(bearer, jwt.Value));

        services.AddAuthorization();

        services.AddSingleton<AccessTokenIssuer>();
        services.AddScoped<RefreshTokenService>();
        services.AddScoped<EmailVerificationService>();
        services.AddScoped<LegalConsentService>();
        services.AddScoped<AuthService>();

        return builder;
    }

    private static void ConfigureBearer(JwtBearerOptions bearer, JwtOptions jwt)
    {
        bearer.MapInboundClaims = false;
        bearer.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(jwt.KeyBytes),
            ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
            ClockSkew = _clockSkew,
            NameClaimType = JwtRegisteredClaimNames.Sub,
            RoleClaimType = AccessTokenIssuer.RoleClaim,
        };
        bearer.Events = new JwtBearerEvents { OnTokenValidated = RejectStaleTokenAsync };
    }

    // LLD §7.1: a token stops working as soon as the password or role changes (sv) or the account is suspended or deleted,
    // without waiting for its 15 minutes to run out
    private static async Task RejectStaleTokenAsync(TokenValidatedContext context)
    {
        var principal = context.Principal;
        if (!Guid.TryParse(principal?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out var userId)
            || !int.TryParse(principal?.FindFirst(AccessTokenIssuer.SecurityVersionClaim)?.Value, out var securityVersion))
        {
            context.Fail("The token has no valid subject or security version.");
            return;
        }

        var db = context.HttpContext.RequestServices.GetRequiredService<TerrunDbContext>();
        var isCurrent = await db.Users.AsNoTracking().AnyAsync(
            u => u.Id == userId && u.SecurityVersion == securityVersion && u.Status == UserStatus.Active,
            context.HttpContext.RequestAborted);
        if (!isCurrent)
        {
            context.Fail("The token is no longer valid for this account.");
        }
    }
}
