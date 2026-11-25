namespace Terrun.Infrastructure.Persistence;

// Constraint names the API maps to an error code when the database rejects a write (a race between two requests)
public static class DatabaseConstraints
{
    public const string ActivityClientIdUnique = "uk_activity_user_id_client_id";

    // D-34: created with SQL in the migration, because EF cannot express an EXCLUDE constraint
    public const string ActivityNoOverlap = "ex_activity_user_id_time";
}
