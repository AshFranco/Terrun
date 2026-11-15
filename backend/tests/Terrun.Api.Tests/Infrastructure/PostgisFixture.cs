using DotNet.Testcontainers.Images;
using Testcontainers.PostgreSql;
using ImagePlatform = DotNet.Testcontainers.Images.Platform;

namespace Terrun.Api.Tests.Infrastructure;

// Integration tests run against a real PostgreSQL + PostGIS, never an in-memory database
public sealed class PostgisFixture : IAsyncLifetime
{
    // The official PostGIS image is published only for amd64; Apple Silicon runs it under emulation
    private static readonly DockerImage _postgisImage = new("postgis/postgis:16-3.5", new ImagePlatform("linux/amd64"));

    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder(_postgisImage).Build();

    public string ConnectionString => _container.GetConnectionString();

    public async ValueTask InitializeAsync() => await _container.StartAsync();

    public async ValueTask DisposeAsync() => await _container.DisposeAsync();
}
