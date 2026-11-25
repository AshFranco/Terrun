using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Terrun.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddJobRunTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "job_run",
                columns: table => new
                {
                    name = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    last_started_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_finished_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_succeeded_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_error = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_job_run", x => x.name);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "job_run");
        }
    }
}
