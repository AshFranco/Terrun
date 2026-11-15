using Microsoft.EntityFrameworkCore;
using Terrun.Domain.Identity;
using Terrun.Domain.Moderation;
using Terrun.Domain.Platform;
using Terrun.Domain.Profiles;

namespace Terrun.Infrastructure.Persistence;

public sealed class TerrunDbContext(DbContextOptions<TerrunDbContext> options) : DbContext(options)
{
    public DbSet<AppUser> Users => Set<AppUser>();

    public DbSet<ExternalLogin> ExternalLogins => Set<ExternalLogin>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<VerificationToken> VerificationTokens => Set<VerificationToken>();

    public DbSet<LegalDocument> LegalDocuments => Set<LegalDocument>();

    public DbSet<Consent> Consents => Set<Consent>();

    public DbSet<Profile> Profiles => Set<Profile>();

    public DbSet<UserPreference> UserPreferences => Set<UserPreference>();

    public DbSet<PrivacyZone> PrivacyZones => Set<PrivacyZone>();

    public DbSet<UserStats> UserStats => Set<UserStats>();

    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // LLD §2.1
        modelBuilder.HasPostgresExtension("postgis");
        modelBuilder.HasPostgresExtension("citext");
        modelBuilder.HasPostgresExtension("pgcrypto");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TerrunDbContext).Assembly);
    }
}
