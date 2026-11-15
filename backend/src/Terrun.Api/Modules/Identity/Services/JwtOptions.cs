using System.Text;

namespace Terrun.Api.Modules.Identity.Services;

// LLD §7.1 and §8: HS256 with a key of at least 256 bits from the environment (Jwt__Key), 15-minute access tokens and
// 30-day refresh tokens. Only the key is secret; issuer and audience live in appsettings.json
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";
    public const int MinimumKeyBytes = 32;

    public string Key { get; init; } = string.Empty;

    public string Issuer { get; init; } = string.Empty;

    public string Audience { get; init; } = string.Empty;

    public int AccessTokenMinutes { get; init; } = 15;

    public int RefreshTokenDays { get; init; } = 30;

    public byte[] KeyBytes => Encoding.UTF8.GetBytes(Key);

    public bool IsValid => KeyBytes.Length >= MinimumKeyBytes && Issuer.Length > 0 && Audience.Length > 0;
}
