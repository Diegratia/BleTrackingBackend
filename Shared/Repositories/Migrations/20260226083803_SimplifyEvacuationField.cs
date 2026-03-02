using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class SimplifyEvacuationField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_evacuation_assembly_points_floorplan_masked_area_floorplan_masked_area_id",
                table: "evacuation_assembly_points");

            migrationBuilder.DropIndex(
                name: "IX_evacuation_assembly_points_floorplan_masked_area_id",
                table: "evacuation_assembly_points");

            migrationBuilder.DropColumn(
                name: "floorplan_masked_area_id",
                table: "evacuation_assembly_points");

            migrationBuilder.DropColumn(
                name: "total_safe",
                table: "evacuation_alerts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "floorplan_masked_area_id",
                table: "evacuation_assembly_points",
                type: "uniqueidentifier",
                maxLength: 36,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "total_safe",
                table: "evacuation_alerts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_evacuation_assembly_points_floorplan_masked_area_id",
                table: "evacuation_assembly_points",
                column: "floorplan_masked_area_id");

            migrationBuilder.AddForeignKey(
                name: "FK_evacuation_assembly_points_floorplan_masked_area_floorplan_masked_area_id",
                table: "evacuation_assembly_points",
                column: "floorplan_masked_area_id",
                principalTable: "floorplan_masked_area",
                principalColumn: "id");
        }
    }
}
