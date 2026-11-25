using System.Linq.Expressions;
using Terrun.Domain.Activities;
using Terrun.Domain.Common;

namespace Terrun.Api.Modules.Activities.Services;

// LLD §7.2: the owner sees every activity of their own, in any state. Other runners only see processed public
// activities; "followers" is owner-only until the follow table exists (P1, D-38), and blocked users will be excluded
// there too. The rule goes inside the SQL query, never after it, so nothing leaks (IDOR)
internal static class ActivityVisibility
{
    public static Expression<Func<Activity, bool>> VisibleTo(Guid viewerId) =>
        a => a.UserId == viewerId || (a.Visibility == Visibility.Public && a.Status == ActivityStatus.Processed);
}
