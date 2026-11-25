using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Terrun.Domain.Moderation;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Terrun.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddActivityReviewAndAntiCheatSettings : Migration
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
                .Annotation("Npgsql:Enum:review_status", "approved,pending,rejected")
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

            migrationBuilder.CreateTable(
                name: "activity_review",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    activity_id = table.Column<Guid>(type: "uuid", nullable: false),
                    violated_rules = table.Column<string>(type: "jsonb", nullable: false),
                    status = table.Column<ReviewStatus>(type: "review_status", nullable: false, defaultValue: ReviewStatus.Pending),
                    reviewer_id = table.Column<Guid>(type: "uuid", nullable: true),
                    note = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    resolved_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_activity_review", x => x.id);
                    table.ForeignKey(
                        name: "fk_activity_review_activity_activity_id",
                        column: x => x.activity_id,
                        principalTable: "activity",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_activity_review_users_reviewer_id",
                        column: x => x.reviewer_id,
                        principalTable: "app_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.UpdateData(
                table: "system_setting",
                keyColumn: "key",
                keyValue: "anticheat.max_jump_m",
                column: "description",
                value: "Maximum jump within anticheat.jump_window_s, in meters");

            migrationBuilder.InsertData(
                table: "system_setting",
                columns: new[] { "key", "description", "updated_at", "updated_by", "value" },
                values: new object[,]
                {
                    { "anticheat.jump_window_s", "Window of a GPS jump, in seconds (D-36)", new DateTimeOffset(new DateTime(2026, 11, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "5" },
                    { "anticheat.max_invalid_distance_pct", "Share of distance in invalid legs that sends an activity to review, in percent (D-36)", new DateTimeOffset(new DateTime(2026, 11, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "20" }
                });

            migrationBuilder.CreateIndex(
                name: "ix_activity_review_reviewer_id",
                table: "activity_review",
                column: "reviewer_id");

            migrationBuilder.CreateIndex(
                name: "uk_activity_review_activity_id",
                table: "activity_review",
                column: "activity_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "activity_review");

            migrationBuilder.DeleteData(
                table: "system_setting",
                keyColumn: "key",
                keyValue: "anticheat.jump_window_s");

            migrationBuilder.DeleteData(
                table: "system_setting",
                keyColumn: "key",
                keyValue: "anticheat.max_invalid_distance_pct");

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
                .OldAnnotation("Npgsql:Enum:activity_status", "failed,in_review,pending,processed,processing,rejected")
                .OldAnnotation("Npgsql:Enum:activity_type", "jog,run,walk")
                .OldAnnotation("Npgsql:Enum:consent_method", "registration,settings")
                .OldAnnotation("Npgsql:Enum:discard_reason", "accuracy,jump,timestamp")
                .OldAnnotation("Npgsql:Enum:legal_document_type", "privacy,terms")
                .OldAnnotation("Npgsql:Enum:login_provider", "apple,google")
                .OldAnnotation("Npgsql:Enum:record_type", "half_marathon,km1,km10,km5,longest,marathon")
                .OldAnnotation("Npgsql:Enum:review_status", "approved,pending,rejected")
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

            migrationBuilder.UpdateData(
                table: "system_setting",
                keyColumn: "key",
                keyValue: "anticheat.max_jump_m",
                column: "description",
                value: "Maximum jump in less than 5 seconds, in meters");
        }
    }
}
