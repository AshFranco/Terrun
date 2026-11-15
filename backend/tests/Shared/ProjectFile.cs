using System.Xml.Linq;

namespace Terrun.Tests.Shared;

// Reads a .csproj from the source tree so the dependency rules of LLD §1.2 can be checked
internal sealed class ProjectFile
{
    private readonly XDocument _document;

    private ProjectFile(XDocument document)
    {
        _document = document;
    }

    public IReadOnlyList<string> ProjectReferences => Includes("ProjectReference")
        .Select(path => Path.GetFileNameWithoutExtension(path.Replace('\\', '/')))
        .ToList();

    public IReadOnlyList<string> PackageReferences => Includes("PackageReference").ToList();

    public static ProjectFile Load(string projectName)
    {
        var path = Path.Combine(FindBackendRoot(), "src", projectName, $"{projectName}.csproj");
        return new ProjectFile(XDocument.Load(path));
    }

    private IEnumerable<string> Includes(string elementName) => _document
        .Descendants(elementName)
        .Select(element => (string?)element.Attribute("Include"))
        .OfType<string>();

    private static string FindBackendRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Terrun.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new DirectoryNotFoundException("Terrun.slnx was not found above the test output folder.");
    }
}
