using System.Globalization;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Terrun.Domain.Identity;

namespace Terrun.Api.Modules.Identity.Services;

public sealed class AccessTokenIssuer(IOptions<JwtOptions> options, TimeProvider clock)
{
    public const string SecurityVersionClaim = "sv";
    public const string RoleClaim = "role";

    private readonly JsonWebTokenHandler _handler = new();

    // LLD §7.1: claims sub, role and sv; sv changes with the password or the role and invalidates older tokens
    public (string Token, DateTimeOffset ExpiresAt) Issue(AppUser user)
    {
        var settings = options.Value;
        var now = clock.GetUtcNow();
        var expiresAt = now.AddMinutes(settings.AccessTokenMinutes);

        var token = _handler.CreateToken(new SecurityTokenDescriptor
        {
            Issuer = settings.Issuer,
            Audience = settings.Audience,
            IssuedAt = now.UtcDateTime,
            NotBefore = now.UtcDateTime,
            Expires = expiresAt.UtcDateTime,
            Subject = new ClaimsIdentity(
            [
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(RoleClaim, JsonNamingPolicy.SnakeCaseLower.ConvertName(user.Role.ToString())),
                new Claim(SecurityVersionClaim, user.SecurityVersion.ToString(CultureInfo.InvariantCulture)),
            ]),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(settings.KeyBytes), SecurityAlgorithms.HmacSha256),
        });

        return (token, expiresAt);
    }
}
