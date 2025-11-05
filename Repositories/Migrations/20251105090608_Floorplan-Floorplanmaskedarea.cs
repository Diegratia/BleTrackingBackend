using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class FloorplanFloorplanmaskedarea : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_floorplan_masked_area_mst_floorplan_MstFloorplanId",
                table: "floorplan_masked_area");

            migrationBuilder.DropIndex(
                name: "IX_floorplan_masked_area_MstFloorplanId",
                table: "floorplan_masked_area");

            migrationBuilder.DropColumn(
                name: "MstFloorplanId",
                table: "floorplan_masked_area");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "MstFloorplanId",
                table: "floorplan_masked_area",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_floorplan_masked_area_MstFloorplanId",
                table: "floorplan_masked_area",
                column: "MstFloorplanId");

            migrationBuilder.AddForeignKey(
                name: "FK_floorplan_masked_area_mst_floorplan_MstFloorplanId",
                table: "floorplan_masked_area",
                column: "MstFloorplanId",
                principalTable: "mst_floorplan",
                principalColumn: "id");
        }
    }
}
