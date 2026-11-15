using Microsoft.EntityFrameworkCore;
using Terrun.Domain.Common;
using Terrun.Domain.Profiles;

namespace Terrun.Api.Tests.Persistence;

// CTZ-13 acceptance: a runner starts with auto-pause and every notice turned on (LLD §2.3, M2)
[Collection(MigratedDatabaseCollection.Name)]
public sealed class UserPreferenceDefaultsTests(MigratedDatabaseFixture database)
{
    [Fact]
    public async Task InsertPreference_OnlyUserId_StartsWithEverythingTurnedOn()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var scope = database.CreateScope();
        var db = MigratedDatabaseFixture.Db(scope);
        var user = MigratedDatabaseFixture.NewUser();
        db.Users.Add(user);
        await db.SaveChangesAsync(ct);

        await db.Database.ExecuteSqlAsync($"INSERT INTO user_preference (user_id) VALUES ({user.Id})", ct);
        var preference = await db.UserPreferences.AsNoTracking().SingleAsync(p => p.UserId == user.Id, ct);

        Assert.Equal(UnitSystem.Metric, preference.Units);
        Assert.Equal(Visibility.Followers, preference.DefaultVisibility);
        Assert.True(preference.IsAutoPauseEnabled);
        Assert.True(preference.IsVoiceCuesEnabled);
        Assert.Equal(1m, preference.VoiceCueIntervalKm);
        Assert.True(preference.NotifyZoneStolen && preference.NotifyKudos && preference.NotifyComments);
        Assert.True(preference.NotifyFollowers && preference.NotifyChallenges);
    }

    [Fact]
    public async Task SavePreference_RunnerTurnsThingsOff_KeepsTheChoice()
    {
        var ct = TestContext.Current.CancellationToken;
        var user = MigratedDatabaseFixture.NewUser();
        await using (var writeScope = database.CreateScope())
        {
            var db = MigratedDatabaseFixture.Db(writeScope);
            db.Users.Add(user);
            db.UserPreferences.Add(new UserPreference
            {
                UserId = user.Id,
                DefaultVisibility = Visibility.Private,
                IsAutoPauseEnabled = false,
                VoiceCueIntervalKm = 2m,
                NotifyKudos = false,
            });
            await db.SaveChangesAsync(ct);
        }

        await using var readScope = database.CreateScope();
        var saved = await MigratedDatabaseFixture.Db(readScope).UserPreferences.AsNoTracking().SingleAsync(p => p.UserId == user.Id, ct);

        Assert.Equal(Visibility.Private, saved.DefaultVisibility);
        Assert.False(saved.IsAutoPauseEnabled);
        Assert.Equal(2m, saved.VoiceCueIntervalKm);
        Assert.False(saved.NotifyKudos);
        Assert.True(saved.NotifyComments);
    }
}
