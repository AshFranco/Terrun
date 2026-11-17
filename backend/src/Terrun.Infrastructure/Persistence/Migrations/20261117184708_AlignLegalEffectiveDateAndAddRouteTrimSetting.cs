using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Terrun.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AlignLegalEffectiveDateAndAddRouteTrimSetting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "legal_document",
                keyColumn: "id",
                keyValue: new Guid("6f1c2d0e-7a4b-4c5d-9e8f-0a1b2c3d4e01"),
                column: "effective_from",
                value: new DateTimeOffset(new DateTime(2026, 11, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "legal_document",
                keyColumn: "id",
                keyValue: new Guid("6f1c2d0e-7a4b-4c5d-9e8f-0a1b2c3d4e02"),
                column: "effective_from",
                value: new DateTimeOffset(new DateTime(2026, 11, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.InsertData(
                table: "system_setting",
                columns: new[] { "key", "description", "is_public", "updated_at", "updated_by", "value" },
                values: new object[] { "privacy.route_trim_m", "Meters hidden at the start and at the end of a route seen by other users", true, new DateTimeOffset(new DateTime(2026, 11, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "200" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "system_setting",
                keyColumn: "key",
                keyValue: "privacy.route_trim_m");

            migrationBuilder.UpdateData(
                table: "legal_document",
                keyColumn: "id",
                keyValue: new Guid("6f1c2d0e-7a4b-4c5d-9e8f-0a1b2c3d4e01"),
                column: "effective_from",
                value: new DateTimeOffset(new DateTime(2026, 11, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "legal_document",
                keyColumn: "id",
                keyValue: new Guid("6f1c2d0e-7a4b-4c5d-9e8f-0a1b2c3d4e02"),
                column: "effective_from",
                value: new DateTimeOffset(new DateTime(2026, 11, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));
        }
    }
}
