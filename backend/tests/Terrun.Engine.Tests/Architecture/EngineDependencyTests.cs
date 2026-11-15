using Terrun.Tests.Shared;

namespace Terrun.Engine.Tests.Architecture;

public sealed class EngineDependencyTests
{
    private static readonly string[] _forbiddenPackagePrefixes =
    [
        "Microsoft.AspNetCore",
        "Microsoft.EntityFrameworkCore",
        "Npgsql",
        "System.Net.Http",
    ];

    [Fact]
    public void EngineProject_Always_HasNoProjectReferences()
    {
        var project = ProjectFile.Load("Terrun.Engine");

        Assert.Empty(project.ProjectReferences);
    }

    [Fact]
    public void EngineProject_Always_HasNoDatabaseOrHttpPackages()
    {
        var project = ProjectFile.Load("Terrun.Engine");

        var forbidden = project.PackageReferences
            .Where(package => _forbiddenPackagePrefixes.Any(prefix => package.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)));

        Assert.Empty(forbidden);
    }
}
