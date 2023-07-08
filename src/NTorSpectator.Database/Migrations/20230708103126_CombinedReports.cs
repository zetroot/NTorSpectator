using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace NTorSpectator.Database.Migrations
{
    /// <inheritdoc />
    public partial class CombinedReports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ReportId",
                table: "observations",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "reports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "availability_events",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReportId = table.Column<int>(type: "integer", nullable: false),
                    SiteId = table.Column<int>(type: "integer", nullable: false),
                    OccuredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EventType = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_availability_events", x => x.Id);
                    table.ForeignKey(
                        name: "FK_availability_events_reports_ReportId",
                        column: x => x.ReportId,
                        principalTable: "reports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_availability_events_sites_SiteId",
                        column: x => x.SiteId,
                        principalTable: "sites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_observations_ReportId",
                table: "observations",
                column: "ReportId");

            migrationBuilder.CreateIndex(
                name: "IX_availability_events_EventType",
                table: "availability_events",
                column: "EventType");

            migrationBuilder.CreateIndex(
                name: "IX_availability_events_OccuredAt",
                table: "availability_events",
                column: "OccuredAt");

            migrationBuilder.CreateIndex(
                name: "IX_availability_events_ReportId",
                table: "availability_events",
                column: "ReportId");

            migrationBuilder.CreateIndex(
                name: "IX_availability_events_SiteId",
                table: "availability_events",
                column: "SiteId");

            migrationBuilder.AddForeignKey(
                name: "FK_observations_reports_ReportId",
                table: "observations",
                column: "ReportId",
                principalTable: "reports",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_observations_reports_ReportId",
                table: "observations");

            migrationBuilder.DropTable(
                name: "availability_events");

            migrationBuilder.DropTable(
                name: "reports");

            migrationBuilder.DropIndex(
                name: "IX_observations_ReportId",
                table: "observations");

            migrationBuilder.DropColumn(
                name: "ReportId",
                table: "observations");
        }
    }
}
