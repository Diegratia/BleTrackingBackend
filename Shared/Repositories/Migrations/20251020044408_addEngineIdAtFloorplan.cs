using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class addEngineIdAtFloorplan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_mst_engine_engine_id",
                table: "mst_engine");

            migrationBuilder.RenameColumn(
                name: "engine_id",
                table: "mst_engine",
                newName: "engine_tracking_id");

            migrationBuilder.AddColumn<Guid>(
                name: "engine_id",
                table: "mst_floorplan",
                type: "uniqueidentifier",
                maxLength: 36,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_mst_floorplan_engine_id",
                table: "mst_floorplan",
                column: "engine_id");

            migrationBuilder.CreateIndex(
                name: "IX_mst_engine_engine_tracking_id",
                table: "mst_engine",
                column: "engine_tracking_id",
                unique: true,
                filter: "[engine_tracking_id] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_mst_floorplan_mst_engine_engine_id",
                table: "mst_floorplan",
                column: "engine_id",
                principalTable: "mst_engine",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_mst_floorplan_mst_engine_engine_id",
                table: "mst_floorplan");

            migrationBuilder.DropIndex(
                name: "IX_mst_floorplan_engine_id",
                table: "mst_floorplan");

            migrationBuilder.DropIndex(
                name: "IX_mst_engine_engine_tracking_id",
                table: "mst_engine");

            migrationBuilder.DropColumn(
                name: "engine_id",
                table: "mst_floorplan");

            migrationBuilder.RenameColumn(
                name: "engine_tracking_id",
                table: "mst_engine",
                newName: "engine_id");

            migrationBuilder.CreateIndex(
                name: "IX_mst_engine_engine_id",
                table: "mst_engine",
                column: "engine_id",
                unique: true,
                filter: "[engine_id] IS NOT NULL");
        }
    }
}
