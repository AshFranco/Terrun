namespace Terrun.Infrastructure.Security;

public interface IPasswordHasher
{
    string Hash(string password);

    bool Verify(string storedHash, string password);

    // Spends the same time as a real check, for logins whose email does not exist (no timing hint)
    void VerifyAgainstDecoy(string password);
}
