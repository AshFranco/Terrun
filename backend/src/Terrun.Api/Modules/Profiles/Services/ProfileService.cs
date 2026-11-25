using System.Net;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Terrun.Api.Modules.Identity.Validators;
using Terrun.Api.Modules.Profiles.Contracts;
using Terrun.Api.Modules.Profiles.Validators;
using Terrun.Domain.Errors;
using Terrun.Domain.Profiles;
using Terrun.Infrastructure.Persistence;

namespace Terrun.Api.Modules.Profiles.Services;

// RF-07: the runner reads and edits their own profile. Every query filters by the caller's id, taken from the token
public sealed class ProfileService(TerrunDbContext db)
{
    private const string _handleUniqueConstraint = "uk_profile_handle";

    // One query joins the four rows created at registration (CTZ-14); a user without them is a broken account, so it
    // answers 404 like a missing resource instead of a half-filled body
    public async Task<MeResponse> GetMeAsync(Guid userId, CancellationToken cancellationToken)
    {
        var me = await (
            from user in db.Users.AsNoTracking()
            join profile in db.Profiles.AsNoTracking() on user.Id equals profile.UserId
            join preference in db.UserPreferences.AsNoTracking() on user.Id equals preference.UserId
            join stats in db.UserStats.AsNoTracking() on user.Id equals stats.UserId
            where user.Id == userId
            select new MeResponse(
                new MeUserResponse(user.Id, user.Email, user.IsEmailVerified, user.Role, user.CreatedAt),
                new MeProfileResponse(
                    profile.DisplayName,
                    profile.Handle,
                    profile.PhotoUrl,
                    profile.Bio,
                    profile.City,
                    profile.Country,
                    profile.BirthDate,
                    profile.Sex,
                    profile.WeightKg,
                    profile.HeightCm,
                    profile.IsPrivate,
                    profile.UpdatedAt),
                new MePreferencesResponse(
                    preference.Units,
                    preference.DefaultVisibility,
                    preference.IsAutoPauseEnabled,
                    preference.IsVoiceCuesEnabled,
                    preference.VoiceCueIntervalKm,
                    preference.NotifyZoneStolen,
                    preference.NotifyKudos,
                    preference.NotifyComments,
                    preference.NotifyFollowers,
                    preference.NotifyChallenges),
                new MeStatsResponse(
                    stats.TotalDistanceM,
                    stats.TotalTimeS,
                    stats.TotalActivities,
                    stats.ZonesConquered,
                    stats.ZonesOwned,
                    stats.TotalSteals,
                    stats.TotalXp,
                    stats.Level,
                    stats.CurrentStreakWeeks,
                    stats.LongestStreakWeeks,
                    stats.LastActivityAt)))
            .SingleOrDefaultAsync(cancellationToken);

        return me ?? throw new NotFoundException("The account has no profile.");
    }

    public async Task<MeResponse> UpdateProfileAsync(Guid userId, UpdateProfileRequest request, IPAddress? ip, CancellationToken cancellationToken)
    {
        var profile = await db.Profiles.SingleOrDefaultAsync(p => p.UserId == userId, cancellationToken)
            ?? throw new NotFoundException("The account has no profile.");

        await ApplyHandleAsync(profile, request, userId, ip, cancellationToken);
        Apply(profile, request);
        await SaveAsync(cancellationToken);

        return await GetMeAsync(userId, cancellationToken);
    }

    // D-32: the handle can change at any time while it is free; each change goes to the audit log because other runners
    // see the handle on the map and in the feed
    private async Task ApplyHandleAsync(Profile profile, UpdateProfileRequest request, Guid userId, IPAddress? ip, CancellationToken cancellationToken)
    {
        if (!request.Has(nameof(UpdateProfileRequest.Handle)))
        {
            return;
        }

        var handle = AuthRules.NormalizeHandle(request.Handle!);
        if (handle == profile.Handle)
        {
            return;
        }

        if (await db.Profiles.AnyAsync(p => p.Handle == handle && p.UserId != userId, cancellationToken))
        {
            throw HandleTaken();
        }

        profile.Handle = handle;
        db.Record(AuditTrail.HandleChanged, userId, ip);
    }

    private static void Apply(Profile profile, UpdateProfileRequest request)
    {
        if (request.Has(nameof(UpdateProfileRequest.DisplayName)))
        {
            profile.DisplayName = request.DisplayName!.Trim();
        }

        if (request.Has(nameof(UpdateProfileRequest.Bio)))
        {
            profile.Bio = ProfileRules.NormalizeOptionalText(request.Bio);
        }

        if (request.Has(nameof(UpdateProfileRequest.City)))
        {
            profile.City = ProfileRules.NormalizeOptionalText(request.City);
        }

        if (request.Has(nameof(UpdateProfileRequest.Country)))
        {
            profile.Country = ProfileRules.NormalizeCountry(request.Country);
        }

        ApplyPhysicalData(profile, request);
    }

    // Used to estimate calories (RF-07); clearing them falls back to calories.default_weight_kg
    private static void ApplyPhysicalData(Profile profile, UpdateProfileRequest request)
    {
        if (request.Has(nameof(UpdateProfileRequest.BirthDate)))
        {
            profile.BirthDate = request.BirthDate;
        }

        if (request.Has(nameof(UpdateProfileRequest.Sex)))
        {
            profile.Sex = request.Sex!.Value;
        }

        if (request.Has(nameof(UpdateProfileRequest.WeightKg)))
        {
            profile.WeightKg = request.WeightKg;
        }

        if (request.Has(nameof(UpdateProfileRequest.HeightCm)))
        {
            profile.HeightCm = request.HeightCm;
        }

        if (request.Has(nameof(UpdateProfileRequest.IsPrivate)))
        {
            profile.IsPrivate = request.IsPrivate!.Value;
        }
    }

    // Two runners racing for the same free handle: the unique index decides and the loser gets the same 409
    private async Task SaveAsync(CancellationToken cancellationToken)
    {
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation,
            ConstraintName: _handleUniqueConstraint,
        })
        {
            throw HandleTaken();
        }
    }

    private static ConflictException HandleTaken() => new(ErrorCodes.HandleTaken, "The handle is already taken.");
}
