using Microsoft.EntityFrameworkCore.Migrations;
using Terrun.Domain.Identity;

#nullable disable

namespace Terrun.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddConsentEvidenceAndVerificationAttempts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_verification_token_user_id",
                table: "verification_token");

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

            migrationBuilder.AddColumn<short>(
                name: "attempt_count",
                table: "verification_token",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<string>(
                name: "app_version",
                table: "consent",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<ConsentMethod>(
                name: "method",
                table: "consent",
                type: "consent_method",
                nullable: false,
                defaultValue: ConsentMethod.Registration);

            migrationBuilder.CreateIndex(
                name: "ix_verification_token_user_id_type",
                table: "verification_token",
                columns: new[] { "user_id", "type" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_verification_token_user_id_type",
                table: "verification_token");

            migrationBuilder.DropColumn(
                name: "attempt_count",
                table: "verification_token");

            migrationBuilder.DropColumn(
                name: "app_version",
                table: "consent");

            migrationBuilder.DropColumn(
                name: "method",
                table: "consent");

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

            migrationBuilder.CreateIndex(
                name: "ix_verification_token_user_id",
                table: "verification_token",
                column: "user_id");
        }
    }
}
