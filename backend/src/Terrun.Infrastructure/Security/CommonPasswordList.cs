namespace Terrun.Infrastructure.Security;

public interface ICommonPasswordList
{
    bool Contains(string password);
}

// LLD §7.1: the 10,000 most common passwords are rejected (SecLists, MIT license: common-passwords.LICENSE.txt)
internal sealed class CommonPasswordList : ICommonPasswordList
{
    private const string _resourceName = "Terrun.Infrastructure.Security.common-passwords.txt";

    private readonly Lazy<HashSet<string>> _passwords = new(Load);

    public bool Contains(string password) => _passwords.Value.Contains(password.Trim());

    private static HashSet<string> Load()
    {
        using var stream = typeof(CommonPasswordList).Assembly.GetManifestResourceStream(_resourceName)
            ?? throw new InvalidOperationException($"Embedded resource '{_resourceName}' is missing.");
        using var reader = new StreamReader(stream);

        var passwords = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        while (reader.ReadLine() is { } line)
        {
            if (line.Length > 0)
            {
                passwords.Add(line);
            }
        }

        return passwords;
    }
}
