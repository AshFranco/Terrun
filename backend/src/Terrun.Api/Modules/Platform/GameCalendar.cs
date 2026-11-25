namespace Terrun.Api.Modules.Platform;

// The days, weeks and months of the game follow the time of the Dominican Republic (AST, UTC-4, no daylight saving):
// the scheduled jobs (§6) and the period summaries (/me/summary) use the same calendar
public static class GameCalendar
{
    public const string TimeZoneId = "America/Santo_Domingo";

    public static TimeZoneInfo TimeZone => TimeZoneInfo.FindSystemTimeZoneById(TimeZoneId);
}
