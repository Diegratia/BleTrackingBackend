using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddFilterPresets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "card_status",
                table: "card",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "tracking_report_presets",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    time_range = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    custom_from_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    custom_to_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    status = table.Column<int>(type: "int", nullable: false),
                    application_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    created_by = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_by = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tracking_report_presets", x => x.id);
                    table.ForeignKey(
                        name: "FK_tracking_report_presets_mst_application_application_id",
                        column: x => x.application_id,
                        principalTable: "mst_application",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_tracking_report_presets_application_id",
                table: "tracking_report_presets",
                column: "application_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tracking_report_presets");

            migrationBuilder.DropColumn(
                name: "card_status",
                table: "card");
        }
    }
}
