using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddFilterPresetsAtt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "area_id",
                table: "tracking_report_presets",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "building_id",
                table: "tracking_report_presets",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "floor_id",
                table: "tracking_report_presets",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "floorplan_id",
                table: "tracking_report_presets",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "member_id",
                table: "tracking_report_presets",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "visitor_id",
                table: "tracking_report_presets",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "area_id",
                table: "tracking_report_presets");

            migrationBuilder.DropColumn(
                name: "building_id",
                table: "tracking_report_presets");

            migrationBuilder.DropColumn(
                name: "floor_id",
                table: "tracking_report_presets");

            migrationBuilder.DropColumn(
                name: "floorplan_id",
                table: "tracking_report_presets");

            migrationBuilder.DropColumn(
                name: "member_id",
                table: "tracking_report_presets");

            migrationBuilder.DropColumn(
                name: "visitor_id",
                table: "tracking_report_presets");
        }
    }
}
