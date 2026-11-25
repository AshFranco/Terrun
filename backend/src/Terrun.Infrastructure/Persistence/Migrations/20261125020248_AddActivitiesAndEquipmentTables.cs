using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;
using Terrun.Domain.Activities;
using Terrun.Domain.Common;

#nullable disable

namespace Terrun.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddActivitiesAndEquipmentTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:activity_status", "failed,in_review,pending,processed,processing,rejected")
                .Annotation("Npgsql:Enum:activity_type", "jog,run,walk")
                .Annotation("Npgsql:Enum:consent_method", "registration,settings")
                .Annotation("Npgsql:Enum:discard_reason", "accuracy,jump,timestamp")
                .Annotation("Npgsql:Enum:legal_document_type", "privacy,terms")
                .Annotation("Npgsql:Enum:login_provider", "apple,google")
                .Annotation("Npgsql:Enum:record_type", "half_marathon,km1,km10,km5,longest,marathon")
                .Annotation("Npgsql:Enum:sex", "female,male,other,undisclosed")
                .Annotation("Npgsql:Enum:token_type", "email_verification,password_reset")
                .Annotation("Npgsql:Enum:unit_system", "imperial,metric")
                .Annotation("Npgsql:Enum:user_role", "admin,moderator,runner")
                .Annotation("Npgsql:Enum:user_status", "active,deleted,suspended")
                .Annotation("Npgsql:Enum:visibility", "followers,private,public")
                .Annotation("Npgsql:PostgresExtension:btree_gist", ",,")
                .Annotation("Npgsql:PostgresExtension:citext", ",,")
                .Annotation("Npgsql:PostgresExtension:pgcrypto", ",,")
                .Annotation("Npgsql:PostgresExtension:postgis", ",,")
                .OldAnnotation("Npgsql:Enum:consent_method", "registration,settings")
                .OldAnnotation("Npgsql:Enum:legal_document_type", "privacy,terms")
                .OldAnnotation("Npgsql:Enum:login_provider", "apple,google")
                .OldAnnotation("Npgsql:Enum:sex", "female,male,other,undisclosed")
                .OldAnnotation("Npgsql:Enum:token_type", "email_verification,password_reset")
                .OldAnnotation("Npgsql:Enum:unit_system", "imperial,metric")
                .OldAnnotation("Npgsql:Enum:user_role", "admin,moderator,runner")
                .OldAnnotation("Npgsql:Enum:user_status", "active,deleted,suspended")
                .OldAnnotation("Npgsql:Enum:visibility", "followers,private,public")
                .OldAnnotation("Npgsql:PostgresExtension:citext", ",,")
                .OldAnnotation("Npgsql:PostgresExtension:pgcrypto", ",,")
                .OldAnnotation("Npgsql:PostgresExtension:postgis", ",,");

            migrationBuilder.CreateTable(
                name: "shoe",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    brand = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    model = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    nickname = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    initial_distance_m = table.Column<int>(type: "integer", nullable: false),
                    total_distance_m = table.Column<long>(type: "bigint", nullable: false),
                    retirement_km = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)700),
                    is_default = table.Column<bool>(type: "boolean", nullable: false),
                    retired_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_shoe", x => x.id);
                    table.ForeignKey(
                        name: "fk_shoe_app_user_user_id",
                        column: x => x.user_id,
                        principalTable: "app_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "activity",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    client_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<ActivityType>(type: "activity_type", nullable: false),
                    title = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    visibility = table.Column<Visibility>(type: "visibility", nullable: false),
                    status = table.Column<ActivityStatus>(type: "activity_status", nullable: false, defaultValue: ActivityStatus.Pending),
                    status_reason = table.Column<string>(type: "text", nullable: true),
                    started_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ended_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    elapsed_time_s = table.Column<int>(type: "integer", nullable: true),
                    moving_time_s = table.Column<int>(type: "integer", nullable: true),
                    distance_m = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    avg_pace_s_per_km = table.Column<int>(type: "integer", nullable: true),
                    max_speed_mps = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    elevation_gain_m = table.Column<decimal>(type: "numeric(7,1)", precision: 7, scale: 1, nullable: true),
                    elevation_loss_m = table.Column<decimal>(type: "numeric(7,1)", precision: 7, scale: 1, nullable: true),
                    calories = table.Column<int>(type: "integer", nullable: true),
                    route = table.Column<LineString>(type: "geography (linestring, 4326)", nullable: true),
                    shoe_id = table.Column<Guid>(type: "uuid", nullable: true),
                    new_zones = table.Column<short>(type: "smallint", nullable: false),
                    xp_earned = table.Column<int>(type: "integer", nullable: false),
                    attempts = table.Column<short>(type: "smallint", nullable: false),
                    device = table.Column<string>(type: "text", nullable: true),
                    app_version = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    processed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_activity", x => x.id);
                    table.CheckConstraint("ck_activity_ended_after_started", "ended_at > started_at");
                    table.ForeignKey(
                        name: "fk_activity_shoes_shoe_id",
                        column: x => x.shoe_id,
                        principalTable: "shoe",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_activity_users_user_id",
                        column: x => x.user_id,
                        principalTable: "app_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "gps_point",
                columns: table => new
                {
                    activity_id = table.Column<Guid>(type: "uuid", nullable: false),
                    seq = table.Column<int>(type: "integer", nullable: false),
                    segment = table.Column<short>(type: "smallint", nullable: false),
                    recorded_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    lat = table.Column<double>(type: "double precision", nullable: false),
                    lon = table.Column<double>(type: "double precision", nullable: false),
                    altitude_m = table.Column<float>(type: "real", nullable: true),
                    accuracy_m = table.Column<float>(type: "real", nullable: true),
                    speed_mps = table.Column<float>(type: "real", nullable: true),
                    is_discarded = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    discard_reason = table.Column<DiscardReason>(type: "discard_reason", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_gps_point", x => new { x.activity_id, x.seq });
                    table.ForeignKey(
                        name: "fk_gps_point_activity_activity_id",
                        column: x => x.activity_id,
                        principalTable: "activity",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "personal_record",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<RecordType>(type: "record_type", nullable: false),
                    value = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    activity_id = table.Column<Guid>(type: "uuid", nullable: false),
                    achieved_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_personal_record", x => x.id);
                    table.ForeignKey(
                        name: "fk_personal_record_activity_activity_id",
                        column: x => x.activity_id,
                        principalTable: "activity",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_personal_record_app_user_user_id",
                        column: x => x.user_id,
                        principalTable: "app_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "split",
                columns: table => new
                {
                    activity_id = table.Column<Guid>(type: "uuid", nullable: false),
                    number = table.Column<short>(type: "smallint", nullable: false),
                    distance_m = table.Column<decimal>(type: "numeric(7,2)", precision: 7, scale: 2, nullable: false),
                    duration_s = table.Column<int>(type: "integer", nullable: false),
                    pace_s_per_km = table.Column<int>(type: "integer", nullable: false),
                    elevation_m = table.Column<decimal>(type: "numeric(6,1)", precision: 6, scale: 1, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_split", x => new { x.activity_id, x.number });
                    table.ForeignKey(
                        name: "fk_split_activity_activity_id",
                        column: x => x.activity_id,
                        principalTable: "activity",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "system_setting",
                columns: new[] { "key", "description", "is_public", "updated_at", "updated_by", "value" },
                values: new object[] { "activity.max_duration_h", "Maximum duration of an activity, in hours (D-34)", true, new DateTimeOffset(new DateTime(2026, 11, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "24" });

            migrationBuilder.CreateIndex(
                name: "ix_activity_pending_created_at",
                table: "activity",
                column: "created_at",
                filter: "status = 'pending'");

            migrationBuilder.CreateIndex(
                name: "ix_activity_route",
                table: "activity",
                column: "route")
                .Annotation("Npgsql:IndexMethod", "gist");

            migrationBuilder.CreateIndex(
                name: "ix_activity_shoe_id",
                table: "activity",
                column: "shoe_id");

            migrationBuilder.CreateIndex(
                name: "ix_activity_user_id_started_at",
                table: "activity",
                columns: new[] { "user_id", "started_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "uk_activity_user_id_client_id",
                table: "activity",
                columns: new[] { "user_id", "client_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_personal_record_activity_id",
                table: "personal_record",
                column: "activity_id");

            migrationBuilder.CreateIndex(
                name: "uk_personal_record_user_id_type",
                table: "personal_record",
                columns: new[] { "user_id", "type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_shoe_user_id",
                table: "shoe",
                column: "user_id",
                unique: true,
                filter: "is_default");

            // D-34: two activities of the same runner cannot overlap in time (409 ACTIVITY_OVERLAP); rejected and failed
            // ones do not count. The range is [started_at, ended_at), so an activity may start exactly when another ends
            migrationBuilder.Sql(
                """
                ALTER TABLE activity ADD CONSTRAINT ex_activity_user_id_time
                    EXCLUDE USING gist (user_id WITH =, tstzrange(started_at, ended_at) WITH &&)
                    WHERE (status NOT IN ('rejected', 'failed'));
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "gps_point");

            migrationBuilder.DropTable(
                name: "personal_record");

            migrationBuilder.DropTable(
                name: "split");

            migrationBuilder.DropTable(
                name: "activity");

            migrationBuilder.DropTable(
                name: "shoe");

            migrationBuilder.DeleteData(
                table: "system_setting",
                keyColumn: "key",
                keyValue: "activity.max_duration_h");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:consent_method", "registration,settings")
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
                .Annotation("Npgsql:PostgresExtension:postgis", ",,")
                .OldAnnotation("Npgsql:Enum:activity_status", "failed,in_review,pending,processed,processing,rejected")
                .OldAnnotation("Npgsql:Enum:activity_type", "jog,run,walk")
                .OldAnnotation("Npgsql:Enum:consent_method", "registration,settings")
                .OldAnnotation("Npgsql:Enum:discard_reason", "accuracy,jump,timestamp")
                .OldAnnotation("Npgsql:Enum:legal_document_type", "privacy,terms")
                .OldAnnotation("Npgsql:Enum:login_provider", "apple,google")
                .OldAnnotation("Npgsql:Enum:record_type", "half_marathon,km1,km10,km5,longest,marathon")
                .OldAnnotation("Npgsql:Enum:sex", "female,male,other,undisclosed")
                .OldAnnotation("Npgsql:Enum:token_type", "email_verification,password_reset")
                .OldAnnotation("Npgsql:Enum:unit_system", "imperial,metric")
                .OldAnnotation("Npgsql:Enum:user_role", "admin,moderator,runner")
                .OldAnnotation("Npgsql:Enum:user_status", "active,deleted,suspended")
                .OldAnnotation("Npgsql:Enum:visibility", "followers,private,public")
                .OldAnnotation("Npgsql:PostgresExtension:btree_gist", ",,")
                .OldAnnotation("Npgsql:PostgresExtension:citext", ",,")
                .OldAnnotation("Npgsql:PostgresExtension:pgcrypto", ",,")
                .OldAnnotation("Npgsql:PostgresExtension:postgis", ",,");
        }
    }
}
