using Microsoft.EntityFrameworkCore;
using Npgsql;
using Terrun.Domain.Common;
using Terrun.Domain.Identity;
using Terrun.Domain.Profiles;

namespace Terrun.Infrastructure.Persistence;

// Shared by the running app and by the design-time factory, so migrations see the same model
internal static class TerrunDbContextOptions
{
    public static DbContextOptionsBuilder Configure(DbContextOptionsBuilder options, string connectionString) => options
        .UseNpgsql(WithoutKerberos(connectionString), npgsql => npgsql
            .UseNetTopologySuite()
            .MapEnum<UserRole>("user_role")
            .MapEnum<UserStatus>("user_status")
            .MapEnum<LoginProvider>("login_provider")
            .MapEnum<TokenType>("token_type")
            .MapEnum<LegalDocumentType>("legal_document_type")
            .MapEnum<Sex>("sex")
            .MapEnum<UnitSystem>("unit_system")
            .MapEnum<Visibility>("visibility"))
        .UseSnakeCaseNamingConvention();

    // No environment uses Kerberos, and the runtime image lacks libgssapi: skip the GSS negotiation on every connection
    private static string WithoutKerberos(string connectionString) =>
        new NpgsqlConnectionStringBuilder(connectionString) { GssEncryptionMode = GssEncryptionMode.Disable }.ConnectionString;
}
