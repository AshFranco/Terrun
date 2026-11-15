using Microsoft.EntityFrameworkCore;
using Terrun.Domain.Profiles;

namespace Terrun.Api.Tests.Persistence;

[Collection(MigratedDatabaseCollection.Name)]
public sealed class ConstraintTests(MigratedDatabaseFixture database)
{
    [Fact]
    public async Task SaveUser_EmailDiffersOnlyInCase_IsRejected()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var scope = database.CreateScope();
        var db = MigratedDatabaseFixture.Db(scope);
        var first = MigratedDatabaseFixture.NewUser();
        var second = MigratedDatabaseFixture.NewUser();
        second.Email = first.Email.ToUpperInvariant();
        db.Users.Add(first);
        await db.SaveChangesAsync(ct);

        db.Users.Add(second);

        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync(ct));
    }

    [Fact]
    public async Task SaveProfile_WeightBelowTwentyFiveKg_IsRejected()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var scope = database.CreateScope();
        var db = MigratedDatabaseFixture.Db(scope);
        var user = MigratedDatabaseFixture.NewUser();
        db.Users.Add(user);
        await db.SaveChangesAsync(ct);

        db.Profiles.Add(new Profile { UserId = user.Id, DisplayName = "Runner", Handle = $"runner{Guid.NewGuid():N}", WeightKg = 20m });

        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync(ct));
    }
}
