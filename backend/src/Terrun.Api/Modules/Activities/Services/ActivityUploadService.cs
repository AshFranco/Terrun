using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgsqlTypes;
using Terrun.Api.Modules.Activities.Contracts;
using Terrun.Api.Modules.Activities.Processing;
using Terrun.Api.Modules.Platform.Services;
using Terrun.Domain.Activities;
using Terrun.Domain.Errors;
using Terrun.Infrastructure.Persistence;

namespace Terrun.Api.Modules.Activities.Services;

// RF-16 (CTZ-30): the phone uploads a finished activity. It is stored as pending with its raw points and the processor
// (CTZ-31a/31b, queued by CTZ-104) computes the official results. Every check is scoped to the caller's own activities
public sealed class ActivityUploadService(TerrunDbContext db, SystemSettingsReader settings, ActivityQueue queue)
{
    private const string _copyGpsPoints =
        "COPY gps_point (activity_id, seq, segment, recorded_at, lat, lon, altitude_m, accuracy_m, speed_mps) FROM STDIN (FORMAT BINARY)";

    public async Task<(ActivityUploadResponse Activity, bool IsNew)> UploadAsync(
        Guid userId, CreateActivityRequest request, CancellationToken cancellationToken)
    {
        // MT-09: a retry of the same activity answers with what was stored, before any other check (it overlaps itself)
        if (await FindExistingAsync(userId, request.ClientId, cancellationToken) is { } existing)
        {
            return (existing, false);
        }

        await EnsureAcceptableAsync(userId, request, cancellationToken);

        try
        {
            var result = await InsertAsync(userId, request, cancellationToken);
            if (result.IsNew)
            {
                // LLD §6: processed right away by ActivityQueueWorker; the API is awake because the upload is incoming traffic
                queue.Enqueue(result.Activity.Id);
            }

            return result;
        }
        catch (DbUpdateException exception) when (IsViolationOf(exception.InnerException, DatabaseConstraints.ActivityClientIdUnique))
        {
            // Two retries of the same activity raced; the unique index kept one and this one returns it
            return ((await FindExistingAsync(userId, request.ClientId, cancellationToken))!, false);
        }
        catch (Exception exception) when (IsViolationOf(exception as PostgresException ?? exception.InnerException, DatabaseConstraints.ActivityNoOverlap))
        {
            throw Overlap();
        }
    }

    private Task<ActivityUploadResponse?> FindExistingAsync(Guid userId, Guid clientId, CancellationToken cancellationToken) =>
        db.Activities.AsNoTracking()
            .Where(a => a.UserId == userId && a.ClientId == clientId)
            .Select(a => new ActivityUploadResponse(a.Id, a.Status))
            .SingleOrDefaultAsync(cancellationToken);

    private async Task EnsureAcceptableAsync(Guid userId, CreateActivityRequest request, CancellationToken cancellationToken)
    {
        var maxPoints = await settings.GetAsync<int>(SystemSettingsReader.ActivityMaxPoints, cancellationToken);
        if (request.Points!.Count > maxPoints)
        {
            throw new PayloadTooLargeException($"The activity has more than {maxPoints} points (activity.max_points).");
        }

        if (request.ShoeId is { } shoeId && !await db.Shoes.AnyAsync(s => s.Id == shoeId && s.UserId == userId, cancellationToken))
        {
            throw new FieldValidationException("shoeId", "The shoe does not exist or is not yours.");
        }

        // Fast path: most overlaps are answered here, without opening a transaction
        if (await OverlapsAsync(userId, request, cancellationToken))
        {
            throw Overlap();
        }
    }

    private Task<bool> OverlapsAsync(Guid userId, CreateActivityRequest request, CancellationToken cancellationToken)
    {
        var startedAt = request.StartedAt.ToUniversalTime();
        var endedAt = request.EndedAt.ToUniversalTime();
        // The same clientId is the same activity (a retry racing this one), never an overlap: the locked path returns it
        return db.Activities.AnyAsync(
            a => a.UserId == userId
                && a.ClientId != request.ClientId
                && a.Status != ActivityStatus.Rejected
                && a.Status != ActivityStatus.Failed
                && a.StartedAt < endedAt
                && a.EndedAt > startedAt,
            cancellationToken);
    }

    // The activity and its points commit together; the points go through binary COPY because an upload can carry up to
    // activity.max_points rows and one INSERT per point would take seconds
    private async Task<(ActivityUploadResponse Activity, bool IsNew)> InsertAsync(Guid userId, CreateActivityRequest request, CancellationToken cancellationToken)
    {
        var visibility = await db.UserPreferences.AsNoTracking()
            .Where(p => p.UserId == userId)
            .Select(p => p.DefaultVisibility)
            .SingleAsync(cancellationToken);
        var activity = new Activity
        {
            Id = Guid.CreateVersion7(),
            UserId = userId,
            ClientId = request.ClientId,
            Type = request.Type!.Value,
            Visibility = visibility,
            StartedAt = request.StartedAt.ToUniversalTime(),
            EndedAt = request.EndedAt.ToUniversalTime(),
            ShoeId = request.ShoeId,
            Device = request.Device,
            AppVersion = request.AppVersion,
        };

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        // D-34: uploads of the same runner go one at a time. Concurrent overlapping inserts that wait on an EXCLUDE
        // constraint can deadlock each other (PostgreSQL), which would answer 500 instead of 409. Inside the lock the
        // retry and overlap checks are exact; the unique index and the constraint stay as the last guarantee
        await db.Database.ExecuteSqlAsync($"SELECT pg_advisory_xact_lock(hashtextextended({userId.ToString()}, 0))", cancellationToken);
        if (await FindExistingAsync(userId, request.ClientId, cancellationToken) is { } existing)
        {
            return (existing, false);
        }

        if (await OverlapsAsync(userId, request, cancellationToken))
        {
            throw Overlap();
        }

        db.Activities.Add(activity);
        await db.SaveChangesAsync(cancellationToken);
        await CopyPointsAsync(activity.Id, request.Points!, cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return (new ActivityUploadResponse(activity.Id, activity.Status), true);
    }

    private async Task CopyPointsAsync(Guid activityId, IReadOnlyList<GpsPointRequest> points, CancellationToken cancellationToken)
    {
        var connection = (NpgsqlConnection)db.Database.GetDbConnection();
        await using var importer = await connection.BeginBinaryImportAsync(_copyGpsPoints, cancellationToken);
        foreach (var point in points)
        {
            await importer.StartRowAsync(cancellationToken);
            await importer.WriteAsync(activityId, NpgsqlDbType.Uuid, cancellationToken);
            await importer.WriteAsync(point.Seq, NpgsqlDbType.Integer, cancellationToken);
            await importer.WriteAsync(point.Segment, NpgsqlDbType.Smallint, cancellationToken);
            await importer.WriteAsync(point.RecordedAt.ToUniversalTime(), NpgsqlDbType.TimestampTz, cancellationToken);
            await importer.WriteAsync(point.Lat!.Value, NpgsqlDbType.Double, cancellationToken);
            await importer.WriteAsync(point.Lon!.Value, NpgsqlDbType.Double, cancellationToken);
            await WriteNullableAsync(importer, point.AltitudeM, cancellationToken);
            await WriteNullableAsync(importer, point.AccuracyM, cancellationToken);
            await WriteNullableAsync(importer, point.SpeedMps, cancellationToken);
        }

        await importer.CompleteAsync(cancellationToken);
    }

    private static Task WriteNullableAsync(NpgsqlBinaryImporter importer, float? value, CancellationToken cancellationToken) =>
        value is { } number
            ? importer.WriteAsync(number, NpgsqlDbType.Real, cancellationToken)
            : importer.WriteNullAsync(cancellationToken);

    private static bool IsViolationOf(Exception? exception, string constraint) =>
        exception is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation or PostgresErrorCodes.ExclusionViolation } postgres
        && postgres.ConstraintName == constraint;

    private static ConflictException Overlap() =>
        new(ErrorCodes.ActivityOverlap, "The activity overlaps in time with another of your activities.");
}
