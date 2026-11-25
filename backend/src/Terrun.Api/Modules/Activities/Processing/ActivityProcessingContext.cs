using Terrun.Domain.Activities;
using Terrun.Engine.Tracks;
using Terrun.Infrastructure.Persistence;

namespace Terrun.Api.Modules.Activities.Processing;

// What the stages share while one activity is processed: the locked activity (tracked, saved at the end) and the context
// that owns the transaction
public sealed class ActivityProcessingContext(Activity activity, TerrunDbContext db)
{
    public Activity Activity { get; } = activity;

    public TerrunDbContext Db { get; } = db;

    // Loaded once by the cleaning stage (CTZ-31a) and read by the later ones
    public IReadOnlyList<TrackPoint> Points { get; set; } = [];

    // Set by the cleaning stage and read by the later ones: metrics (CTZ-31b) and territory use only valid legs
    public CleaningResult? Cleaning { get; set; }

    // LLD §4.1 step 3: an activity in review or shorter than anticheat.min_duration_s adds no territory (MT-05)
    public bool IsEligibleForTerritory { get; set; } = true;
}
