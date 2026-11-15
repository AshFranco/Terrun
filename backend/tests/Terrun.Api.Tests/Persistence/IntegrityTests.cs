using Microsoft.EntityFrameworkCore;
using Npgsql;
using Terrun.Domain.Moderation;

namespace Terrun.Api.Tests.Persistence;

// Data integrity rules that later stories rely on: audit trail (LLD §7.3), timestamps (§2.1) and token versioning (§7.1)
[Collection(MigratedDatabaseCollection.Name)]
public sealed class IntegrityTests(MigratedDatabaseFixture database)
{
    [Fact]
    public async Task UpdateAuditLog_ExistingEntry_IsRejected()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var scope = database.CreateScope();
        var db = MigratedDatabaseFixture.Db(scope);
        var entry = await AddAuditEntryAsync(db, ct);

        var update = () => db.Database.ExecuteSqlAsync($"UPDATE audit_log SET action = 'tampered' WHERE id = {entry.Id}", ct);

        var error = await Assert.ThrowsAsync<PostgresException>(update);
        Assert.Contains("insert-only", error.MessageText, StringComparison.Ordinal);
    }

    [Fact]
    public async Task DeleteAuditLog_ExistingEntry_IsRejected()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var scope = database.CreateScope();
        var db = MigratedDatabaseFixture.Db(scope);
        var entry = await AddAuditEntryAsync(db, ct);

        var delete = () => db.Database.ExecuteSqlAsync($"DELETE FROM audit_log WHERE id = {entry.Id}", ct);

        await Assert.ThrowsAsync<PostgresException>(delete);
    }

    [Fact]
    public async Task SaveUser_ChangedLater_KeepsCreatedAtAndAdvancesUpdatedAt()
    {
        var ct = TestContext.Current.CancellationToken;
        var user = MigratedDatabaseFixture.NewUser();
        await using (var createScope = database.CreateScope())
        {
            var db = MigratedDatabaseFixture.Db(createScope);
            db.Users.Add(user);
            await db.SaveChangesAsync(ct);
        }

        var created = user.CreatedAt;
        await using (var updateScope = database.CreateScope())
        {
            var db = MigratedDatabaseFixture.Db(updateScope);
            var stored = await db.Users.SingleAsync(u => u.Id == user.Id, ct);
            stored.IsEmailVerified = true;
            await db.SaveChangesAsync(ct);
        }

        await using var readScope = database.CreateScope();
        var saved = await MigratedDatabaseFixture.Db(readScope).Users.AsNoTracking().SingleAsync(u => u.Id == user.Id, ct);

        Assert.Equal(created, saved.CreatedAt);
        Assert.True(saved.UpdatedAt > saved.CreatedAt, "updated_at must move forward when the row changes.");
        Assert.True(DateTimeOffset.UtcNow - saved.CreatedAt < TimeSpan.FromMinutes(5), "created_at comes from the clock, not a default value.");
    }

    [Fact]
    public async Task SaveUser_New_StartsAtSecurityVersionOne()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var scope = database.CreateScope();
        var db = MigratedDatabaseFixture.Db(scope);
        var user = MigratedDatabaseFixture.NewUser();

        db.Users.Add(user);
        await db.SaveChangesAsync(ct);

        var version = await db.Database.SqlQuery<int>($"""SELECT security_version AS "Value" FROM app_user WHERE id = {user.Id}""")
            .SingleAsync(ct);
        Assert.Equal(1, version);
    }

    private static async Task<AuditLog> AddAuditEntryAsync(Terrun.Infrastructure.Persistence.TerrunDbContext db, CancellationToken ct)
    {
        var user = MigratedDatabaseFixture.NewUser();
        var entry = new AuditLog { UserId = user.Id, Action = "login_succeeded", Entity = "app_user", EntityId = user.Id.ToString() };
        db.Users.Add(user);
        db.AuditLogs.Add(entry);
        await db.SaveChangesAsync(ct);
        return entry;
    }
}
