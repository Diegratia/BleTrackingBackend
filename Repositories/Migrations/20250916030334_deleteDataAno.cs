using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class deleteDataAno : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_floorplan_masked_area_mst_floorplan_floorplan_id",
                table: "floorplan_masked_area");

            migrationBuilder.DropForeignKey(
                name: "FK_mst_building_mst_application_application_id",
                table: "mst_building");

            migrationBuilder.RenameColumn(
                name: "application_id",
                table: "mst_building",
                newName: "ApplicationId");

            migrationBuilder.RenameIndex(
                name: "IX_mst_building_application_id",
                table: "mst_building",
                newName: "IX_mst_building_ApplicationId");

            migrationBuilder.AddColumn<Guid>(
                name: "MstFloorplanId",
                table: "floorplan_masked_area",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "access_number",
                table: "card_accesses",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

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

            migrationBuilder.AddForeignKey(
                name: "FK_floorplan_masked_area_mst_floorplan_floorplan_id",
                table: "floorplan_masked_area",
                column: "floorplan_id",
                principalTable: "mst_floorplan",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_mst_building_mst_application_ApplicationId",
                table: "mst_building",
                column: "ApplicationId",
                principalTable: "mst_application",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_floorplan_masked_area_mst_floorplan_MstFloorplanId",
                table: "floorplan_masked_area");

            migrationBuilder.DropForeignKey(
                name: "FK_floorplan_masked_area_mst_floorplan_floorplan_id",
                table: "floorplan_masked_area");

            migrationBuilder.DropForeignKey(
                name: "FK_mst_building_mst_application_ApplicationId",
                table: "mst_building");

            migrationBuilder.DropIndex(
                name: "IX_floorplan_masked_area_MstFloorplanId",
                table: "floorplan_masked_area");

            migrationBuilder.DropColumn(
                name: "MstFloorplanId",
                table: "floorplan_masked_area");

            migrationBuilder.RenameColumn(
                name: "ApplicationId",
                table: "mst_building",
                newName: "application_id");

            migrationBuilder.RenameIndex(
                name: "IX_mst_building_ApplicationId",
                table: "mst_building",
                newName: "IX_mst_building_application_id");

            migrationBuilder.AlterColumn<int>(
                name: "access_number",
                table: "card_accesses",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_floorplan_masked_area_mst_floorplan_floorplan_id",
                table: "floorplan_masked_area",
                column: "floorplan_id",
                principalTable: "mst_floorplan",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_mst_building_mst_application_application_id",
                table: "mst_building",
                column: "application_id",
                principalTable: "mst_application",
                principalColumn: "id");
        }
    }
}
