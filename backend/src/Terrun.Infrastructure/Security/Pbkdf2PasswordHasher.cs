using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Terrun.Infrastructure.Security;

// LLD §7.1: PBKDF2-HMAC-SHA512, 210,000 iterations, 16-byte salt. The stored value keeps the algorithm and the
// iteration count ("pbkdf2-sha512$210000$salt$hash"), so the cost can be raised later without breaking old hashes
internal sealed class Pbkdf2PasswordHasher : IPasswordHasher
{
    private const string _algorithm = "pbkdf2-sha512";
    private const int _iterations = 210_000;
    private const int _saltBytes = 16;
    private const int _hashBytes = 64;
    private const char _separator = '$';

    private readonly Lazy<string> _decoyHash;

    public Pbkdf2PasswordHasher() => _decoyHash = new(() => Hash(Convert.ToBase64String(RandomNumberGenerator.GetBytes(_saltBytes))));

    public string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(_saltBytes);
        var hash = Derive(password, salt, _iterations);

        return string.Join(
            _separator,
            _algorithm,
            _iterations.ToString(CultureInfo.InvariantCulture),
            Convert.ToBase64String(salt),
            Convert.ToBase64String(hash));
    }

    public bool Verify(string storedHash, string password)
    {
        var parts = storedHash.Split(_separator);
        if (parts.Length != 4 || parts[0] != _algorithm
            || !int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out var iterations))
        {
            return false;
        }

        var expected = Convert.FromBase64String(parts[3]);
        var actual = Derive(password, Convert.FromBase64String(parts[2]), iterations);

        return CryptographicOperations.FixedTimeEquals(expected, actual);
    }

    public void VerifyAgainstDecoy(string password) => Verify(_decoyHash.Value, password);

    private static byte[] Derive(string password, byte[] salt, int iterations) =>
        Rfc2898DeriveBytes.Pbkdf2(Encoding.UTF8.GetBytes(password), salt, iterations, HashAlgorithmName.SHA512, _hashBytes);
}
