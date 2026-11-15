using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Terrun.Infrastructure.Persistence;

// LLD §2.1: every mutable table has created_at and every editable one updated_at. The database default only covers
// the INSERT, so this keeps updated_at current on every change, using the injectable clock (the LLD IClock)
internal sealed class TimestampsInterceptor(TimeProvider clock) : SaveChangesInterceptor
{
    private const string _createdAt = "CreatedAt";
    private const string _updatedAt = "UpdatedAt";

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        Stamp(eventData.Context);
        return result;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        Stamp(eventData.Context);
        return ValueTask.FromResult(result);
    }

    private void Stamp(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        var now = clock.GetUtcNow();
        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Added)
            {
                SetIfMapped(entry, _createdAt, now);
                SetIfMapped(entry, _updatedAt, now);
            }
            else if (entry.State == EntityState.Modified)
            {
                SetIfMapped(entry, _updatedAt, now);
            }
        }
    }

    private static void SetIfMapped(EntityEntry entry, string propertyName, DateTimeOffset now)
    {
        if (entry.Metadata.FindProperty(propertyName) is not null)
        {
            entry.Property(propertyName).CurrentValue = now;
        }
    }
}
