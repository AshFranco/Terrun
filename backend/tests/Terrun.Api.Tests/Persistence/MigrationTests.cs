using Microsoft.EntityFrameworkCore;
using Terrun.Domain.Identity;

namespace Terrun.Api.Tests.Persistence;

// CTZ-13: the initial migration applies from scratch and leaves the LLD §2.4 settings in place
[Collection(MigratedDatabaseCollection.Name)]
public sealed class MigrationTests(MigratedDatabaseFixture database)
{
    private static readonly string[] _expectedTables =
    [
        "app_user", "consent", "external_login", "legal_document", "privacy_zone", "profile",
        "refresh_token", "system_setting", "user_preference", "user_stats", "verification_token",
    ];

    private static readonly string[] _publicSettings =
    [
        "activity.auto_pause_speed_mps", "activity.max_points", "territory.conquest_threshold",
        "territory.zone_resolution", "xp.level_curve",
    ];

    [Fact]
    public async Task Migrate_EmptyDatabase_CreatesTheElevenTablesOfM1M2AndM13()
    {
        await using var scope = database.CreateScope();
        var db = MigratedDatabaseFixture.Db(scope);

        var tables = await db.Database.SqlQuery<string>($"""
            SELECT table_name AS "Value" FROM information_schema.tables
            WHERE table_schema = 'public' AND table_type = 'BASE TABLE'
              AND table_name NOT IN ('spatial_ref_sys', '__EFMigrationsHistory')
            ORDER BY table_name
            """).ToListAsync(TestContext.Current.CancellationToken);

        Assert.Equal(_expectedTables, tables);
    }

    [Fact]
    public async Task Migrate_EmptyDatabase_InstallsTheExtensionsOfLld21()
    {
        await using var scope = database.CreateScope();
        var db = MigratedDatabaseFixture.Db(scope);

        var extensions = await db.Database.SqlQuery<string>($"""SELECT extname AS "Value" FROM pg_extension""")
            .ToListAsync(TestContext.Current.CancellationToken);

        Assert.Subset(extensions.ToHashSet(), new HashSet<string> { "postgis", "citext", "pgcrypto" });
    }

    [Fact]
    public async Task Migrate_EmptyDatabase_SeedsTheTwentySystemSettings()
    {
        await using var scope = database.CreateScope();
        var db = MigratedDatabaseFixture.Db(scope);

        var settings = await db.SystemSettings.AsNoTracking().ToListAsync(TestContext.Current.CancellationToken);

        Assert.Equal(20, settings.Count);
        Assert.Equal("5", settings.Single(s => s.Key == "territory.conquest_threshold").Value);
        Assert.Equal("30", settings.Single(s => s.Key == "territory.ownership_window_days").Value);
        Assert.Equal(_publicSettings, settings.Where(s => s.IsPublic).Select(s => s.Key).Order());
    }

    [Fact]
    public async Task Migrate_EmptyDatabase_SeedsVersionOneOfTermsAndPrivacy()
    {
        await using var scope = database.CreateScope();
        var db = MigratedDatabaseFixture.Db(scope);

        var documents = await db.LegalDocuments.AsNoTracking()
            .OrderBy(d => d.Version).ThenBy(d => d.Url)
            .Select(d => new { d.Type, d.Version })
            .ToListAsync(TestContext.Current.CancellationToken);

        Assert.Equal(2, documents.Count);
        Assert.Contains(documents, d => d.Type == LegalDocumentType.Terms && d.Version == "1.0");
        Assert.Contains(documents, d => d.Type == LegalDocumentType.Privacy && d.Version == "1.0");
    }

    [Fact]
    public async Task Model_ComparedWithTheMigrations_HasNoPendingChanges()
    {
        await using var scope = database.CreateScope();
        var db = MigratedDatabaseFixture.Db(scope);

        Assert.False(db.Database.HasPendingModelChanges(), "Run dotnet ef migrations add: the model changed without a migration.");
    }
}
