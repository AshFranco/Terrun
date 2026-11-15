using Terrun.Tests.Shared;

namespace Terrun.Api.Tests.Architecture;

// LLD §1.2: Api -> Infrastructure -> Domain; Api -> Engine
public sealed class LayerDependencyTests
{
    [Fact]
    public void DomainProject_Always_HasNoProjectReferences()
    {
        var project = ProjectFile.Load("Terrun.Domain");

        Assert.Empty(project.ProjectReferences);
    }

    [Fact]
    public void InfrastructureProject_Always_ReferencesOnlyDomain()
    {
        var project = ProjectFile.Load("Terrun.Infrastructure");

        Assert.Equal(["Terrun.Domain"], project.ProjectReferences);
    }
}
