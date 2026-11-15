using System;
using System.Net;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Terrun.Domain.Common;
using Terrun.Domain.Identity;
using Terrun.Domain.Profiles;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Terrun.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddIdentityProfileAndSettingsTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:legal_document_type", "privacy,terms")
                .Annotation("Npgsql:Enum:login_provider", "apple,google")
                .Annotation("Npgsql:Enum:sex", "female,male,other,undisclosed")
                .Annotation("Npgsql:Enum:token_type", "email_verification,password_reset")
                .Annotation("Npgsql:Enum:unit_system", "imperial,metric")
                .Annotation("Npgsql:Enum:user_role", "admin,moderator,runner")
                .Annotation("Npgsql:Enum:user_status", "active,deleted,suspended")
                .Annotation("Npgsql:Enum:visibility", "followers,private,public")
                .Annotation("Npgsql:PostgresExtension:citext", ",,")
                .Annotation("Npgsql:PostgresExtension:pgcrypto", ",,")
                .Annotation("Npgsql:PostgresExtension:postgis", ",,");

            migrationBuilder.CreateTable(
                name: "app_user",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    email = table.Column<string>(type: "citext", nullable: false),
                    is_email_verified = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    password_hash = table.Column<string>(type: "text", nullable: true),
                    role = table.Column<UserRole>(type: "user_role", nullable: false, defaultValue: UserRole.Runner),
                    status = table.Column<UserStatus>(type: "user_status", nullable: false, defaultValue: UserStatus.Active),
                    security_version = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    last_login_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_app_user", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "legal_document",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    type = table.Column<LegalDocumentType>(type: "legal_document_type", nullable: false),
                    version = table.Column<string>(type: "text", nullable: false),
                    url = table.Column<string>(type: "text", nullable: false),
                    effective_from = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_legal_document", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "audit_log",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    action = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    entity = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    entity_id = table.Column<string>(type: "text", nullable: true),
                    data = table.Column<string>(type: "jsonb", nullable: true),
                    ip = table.Column<IPAddress>(type: "inet", nullable: true),
                    occurred_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_log", x => x.id);
                    table.ForeignKey(
                        name: "fk_audit_log_app_user_user_id",
                        column: x => x.user_id,
                        principalTable: "app_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "external_login",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider = table.Column<LoginProvider>(type: "login_provider", nullable: false),
                    subject = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "citext", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_external_login", x => x.id);
                    table.ForeignKey(
                        name: "fk_external_login_app_user_user_id",
                        column: x => x.user_id,
                        principalTable: "app_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "privacy_zone",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    center = table.Column<Point>(type: "geography (point, 4326)", nullable: false),
                    radius_m = table.Column<short>(type: "smallint", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_privacy_zone", x => x.id);
                    table.CheckConstraint("ck_privacy_zone_radius_m", "radius_m BETWEEN 100 AND 1000");
                    table.ForeignKey(
                        name: "fk_privacy_zone_app_user_user_id",
                        column: x => x.user_id,
                        principalTable: "app_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "profile",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    display_name = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    handle = table.Column<string>(type: "citext", nullable: false),
                    photo_url = table.Column<string>(type: "text", nullable: true),
                    bio = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    city = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    country = table.Column<string>(type: "char(2)", nullable: true),
                    birth_date = table.Column<DateOnly>(type: "date", nullable: true),
                    sex = table.Column<Sex>(type: "sex", nullable: false),
                    weight_kg = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    height_cm = table.Column<short>(type: "smallint", nullable: true),
                    is_private = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_profile", x => x.user_id);
                    table.CheckConstraint("ck_profile_weight_kg", "weight_kg BETWEEN 25 AND 300");
                    table.ForeignKey(
                        name: "fk_profile_app_user_user_id",
                        column: x => x.user_id,
                        principalTable: "app_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "refresh_token",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    token_hash = table.Column<byte[]>(type: "bytea", nullable: false),
                    family_id = table.Column<Guid>(type: "uuid", nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    revoked_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    replaced_by = table.Column<Guid>(type: "uuid", nullable: true),
                    ip = table.Column<IPAddress>(type: "inet", nullable: true),
                    device = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_refresh_token", x => x.id);
                    table.ForeignKey(
                        name: "fk_refresh_token_app_user_user_id",
                        column: x => x.user_id,
                        principalTable: "app_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "system_setting",
                columns: table => new
                {
                    key = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    value = table.Column<string>(type: "jsonb", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    is_public = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_system_setting", x => x.key);
                    table.ForeignKey(
                        name: "fk_system_setting_app_user_updated_by",
                        column: x => x.updated_by,
                        principalTable: "app_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "user_preference",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    units = table.Column<UnitSystem>(type: "unit_system", nullable: false, defaultValue: UnitSystem.Metric),
                    default_visibility = table.Column<Visibility>(type: "visibility", nullable: false, defaultValue: Visibility.Followers),
                    is_auto_pause_enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    is_voice_cues_enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    voice_cue_interval_km = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: false, defaultValue: 1m),
                    notify_zone_stolen = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    notify_kudos = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    notify_comments = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    notify_followers = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    notify_challenges = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_preference", x => x.user_id);
                    table.ForeignKey(
                        name: "fk_user_preference_app_user_user_id",
                        column: x => x.user_id,
                        principalTable: "app_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_stats",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    total_distance_m = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    total_time_s = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    total_activities = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    zones_conquered = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    zones_owned = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    total_steals = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    total_xp = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    level = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)1),
                    current_streak_weeks = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    longest_streak_weeks = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    last_activity_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_stats", x => x.user_id);
                    table.ForeignKey(
                        name: "fk_user_stats_app_user_user_id",
                        column: x => x.user_id,
                        principalTable: "app_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "verification_token",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<TokenType>(type: "token_type", nullable: false),
                    token_hash = table.Column<byte[]>(type: "bytea", nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    used_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_verification_token", x => x.id);
                    table.ForeignKey(
                        name: "fk_verification_token_app_user_user_id",
                        column: x => x.user_id,
                        principalTable: "app_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "consent",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    legal_document_id = table.Column<Guid>(type: "uuid", nullable: false),
                    accepted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    ip = table.Column<IPAddress>(type: "inet", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_consent", x => x.id);
                    table.ForeignKey(
                        name: "fk_consent_app_user_user_id",
                        column: x => x.user_id,
                        principalTable: "app_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_consent_legal_document_legal_document_id",
                        column: x => x.legal_document_id,
                        principalTable: "legal_document",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "legal_document",
                columns: new[] { "id", "effective_from", "type", "url", "version" },
                values: new object[,]
                {
                    { new Guid("6f1c2d0e-7a4b-4c5d-9e8f-0a1b2c3d4e01"), new DateTimeOffset(new DateTime(2026, 11, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), LegalDocumentType.Terms, "https://github.com/AshFranco/Terrun/blob/main/docs/producto/legal/terminos-v1.0.md", "1.0" },
                    { new Guid("6f1c2d0e-7a4b-4c5d-9e8f-0a1b2c3d4e02"), new DateTimeOffset(new DateTime(2026, 11, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), LegalDocumentType.Privacy, "https://github.com/AshFranco/Terrun/blob/main/docs/producto/legal/privacidad-v1.0.md", "1.0" }
                });

            migrationBuilder.InsertData(
                table: "system_setting",
                columns: new[] { "key", "description", "is_public", "updated_at", "updated_by", "value" },
                values: new object[,]
                {
                    { "activity.auto_pause_speed_mps", "Auto-pause speed threshold used by the app, in m/s", true, new DateTimeOffset(new DateTime(2026, 11, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "0.5" },
                    { "activity.max_points", "Maximum points per activity", true, new DateTimeOffset(new DateTime(2026, 11, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "30000" }
                });

            migrationBuilder.InsertData(
                table: "system_setting",
                columns: new[] { "key", "description", "updated_at", "updated_by", "value" },
                values: new object[,]
                {
                    { "anticheat.max_accuracy_m", "Maximum accepted GPS accuracy, in meters", new DateTimeOffset(new DateTime(2026, 11, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "30" },
                    { "anticheat.max_jump_m", "Maximum jump in less than 5 seconds, in meters", new DateTimeOffset(new DateTime(2026, 11, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "200" },
                    { "anticheat.max_speed_kmh", "Maximum speed of a segment, in km/h", new DateTimeOffset(new DateTime(2026, 11, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "25" },
                    { "anticheat.min_duration_s", "Minimum duration to add territory, in seconds", new DateTimeOffset(new DateTime(2026, 11, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "180" },
                    { "anticheat.min_pace_s_per_km", "Suspicious sustained pace (2:30 min/km), in seconds per km", new DateTimeOffset(new DateTime(2026, 11, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "150" },
                    { "calories.default_weight_kg", "Weight used when the profile has none, in kg", new DateTimeOffset(new DateTime(2026, 11, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "70" },
                    { "map.max_zones_per_bbox", "Maximum zones per map query", new DateTimeOffset(new DateTime(2026, 11, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "2000" }
                });

            migrationBuilder.InsertData(
                table: "system_setting",
                columns: new[] { "key", "description", "is_public", "updated_at", "updated_by", "value" },
                values: new object[] { "territory.conquest_threshold", "Sub-cells (out of 7) needed to conquer a zone", true, new DateTimeOffset(new DateTime(2026, 11, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "5" });

            migrationBuilder.InsertData(
                table: "system_setting",
                columns: new[] { "key", "description", "updated_at", "updated_by", "value" },
                values: new object[,]
                {
                    { "territory.densify_step_m", "Interpolation step of a route, in meters", new DateTimeOffset(new DateTime(2026, 11, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "10" },
                    { "territory.min_ownership_points", "Minimum points to own a zone", new DateTimeOffset(new DateTime(2026, 11, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "3" },
                    { "territory.min_zones_for_ruler", "Minimum owned zones to become ruler of a neighborhood", new DateTimeOffset(new DateTime(2026, 11, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "3" },
                    { "territory.ownership_window_days", "Window of the ownership points, in days", new DateTimeOffset(new DateTime(2026, 11, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "30" }
                });

            migrationBuilder.InsertData(
                table: "system_setting",
                columns: new[] { "key", "description", "is_public", "updated_at", "updated_by", "value" },
                values: new object[] { "territory.zone_resolution", "H3 resolution of a zone", true, new DateTimeOffset(new DateTime(2026, 11, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "9" });

            migrationBuilder.InsertData(
                table: "system_setting",
                columns: new[] { "key", "description", "updated_at", "updated_by", "value" },
                values: new object[] { "xp.conquest", "XP per conquered zone", new DateTimeOffset(new DateTime(2026, 11, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "50" });

            migrationBuilder.InsertData(
                table: "system_setting",
                columns: new[] { "key", "description", "is_public", "updated_at", "updated_by", "value" },
                values: new object[] { "xp.level_curve", "XP for level n = base * n^exponent", true, new DateTimeOffset(new DateTime(2026, 11, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "{\"base\":100,\"exponent\":1.5}" });

            migrationBuilder.InsertData(
                table: "system_setting",
                columns: new[] { "key", "description", "updated_at", "updated_by", "value" },
                values: new object[,]
                {
                    { "xp.ownership", "XP for gaining the ownership of a zone", new DateTimeOffset(new DateTime(2026, 11, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "30" },
                    { "xp.per_km", "XP per kilometer", new DateTimeOffset(new DateTime(2026, 11, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "10" },
                    { "xp.steal", "XP for stealing a zone", new DateTimeOffset(new DateTime(2026, 11, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "40" }
                });

            migrationBuilder.CreateIndex(
                name: "uk_app_user_email",
                table: "app_user",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_audit_log_user_id_occurred_at",
                table: "audit_log",
                columns: new[] { "user_id", "occurred_at" });

            migrationBuilder.CreateIndex(
                name: "ix_consent_legal_document_id",
                table: "consent",
                column: "legal_document_id");

            migrationBuilder.CreateIndex(
                name: "uk_consent_user_id_legal_document_id",
                table: "consent",
                columns: new[] { "user_id", "legal_document_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_external_login_user_id",
                table: "external_login",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "uk_external_login_provider_subject",
                table: "external_login",
                columns: new[] { "provider", "subject" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uk_legal_document_type_version",
                table: "legal_document",
                columns: new[] { "type", "version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_privacy_zone_user_id",
                table: "privacy_zone",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "uk_profile_handle",
                table: "profile",
                column: "handle",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_refresh_token_family_id",
                table: "refresh_token",
                column: "family_id");

            migrationBuilder.CreateIndex(
                name: "ix_refresh_token_user_id_revoked_at",
                table: "refresh_token",
                columns: new[] { "user_id", "revoked_at" });

            migrationBuilder.CreateIndex(
                name: "uk_refresh_token_token_hash",
                table: "refresh_token",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_system_setting_updated_by",
                table: "system_setting",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "ix_verification_token_user_id",
                table: "verification_token",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "uk_verification_token_token_hash",
                table: "verification_token",
                column: "token_hash",
                unique: true);

            // Hand-written: LLD §2.3 (M12) audit_log is insert-only. Keep this block if the migration is regenerated
            migrationBuilder.Sql("""
                CREATE FUNCTION reject_audit_log_change() RETURNS trigger LANGUAGE plpgsql AS $$
                BEGIN
                    RAISE EXCEPTION 'audit_log is insert-only' USING ERRCODE = 'insufficient_privilege';
                END;
                $$;

                CREATE TRIGGER trg_audit_log_insert_only
                    BEFORE UPDATE OR DELETE ON audit_log
                    FOR EACH ROW EXECUTE FUNCTION reject_audit_log_change();

                CREATE TRIGGER trg_audit_log_no_truncate
                    BEFORE TRUNCATE ON audit_log
                    FOR EACH STATEMENT EXECUTE FUNCTION reject_audit_log_change();
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP TRIGGER trg_audit_log_no_truncate ON audit_log;
                DROP TRIGGER trg_audit_log_insert_only ON audit_log;
                DROP FUNCTION reject_audit_log_change();
                """);

            migrationBuilder.DropTable(
                name: "audit_log");

            migrationBuilder.DropTable(
                name: "consent");

            migrationBuilder.DropTable(
                name: "external_login");

            migrationBuilder.DropTable(
                name: "privacy_zone");

            migrationBuilder.DropTable(
                name: "profile");

            migrationBuilder.DropTable(
                name: "refresh_token");

            migrationBuilder.DropTable(
                name: "system_setting");

            migrationBuilder.DropTable(
                name: "user_preference");

            migrationBuilder.DropTable(
                name: "user_stats");

            migrationBuilder.DropTable(
                name: "verification_token");

            migrationBuilder.DropTable(
                name: "legal_document");

            migrationBuilder.DropTable(
                name: "app_user");
        }
    }
}
