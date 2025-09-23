using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class geofenceRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "engine_id",
                table: "geofence",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "floor_id",
                table: "geofence",
                type: "uniqueidentifier",
                maxLength: 36,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "floorplan_id",
                table: "geofence",
                type: "uniqueidentifier",
                maxLength: 36,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "is_active",
                table: "geofence",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_geofence_floor_id",
                table: "geofence",
                column: "floor_id");

            migrationBuilder.CreateIndex(
                name: "IX_geofence_floorplan_id",
                table: "geofence",
                column: "floorplan_id");

            migrationBuilder.AddForeignKey(
                name: "FK_geofence_mst_floor_floor_id",
                table: "geofence",
                column: "floor_id",
                principalTable: "mst_floor",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_geofence_mst_floorplan_floorplan_id",
                table: "geofence",
                column: "floorplan_id",
                principalTable: "mst_floorplan",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_geofence_mst_floor_floor_id",
                table: "geofence");

            migrationBuilder.DropForeignKey(
                name: "FK_geofence_mst_floorplan_floorplan_id",
                table: "geofence");

            migrationBuilder.DropIndex(
                name: "IX_geofence_floor_id",
                table: "geofence");

            migrationBuilder.DropIndex(
                name: "IX_geofence_floorplan_id",
                table: "geofence");

            migrationBuilder.DropColumn(
                name: "engine_id",
                table: "geofence");

            migrationBuilder.DropColumn(
                name: "floor_id",
                table: "geofence");

            migrationBuilder.DropColumn(
                name: "floorplan_id",
                table: "geofence");

            migrationBuilder.DropColumn(
                name: "is_active",
                table: "geofence");
        }
    }
}
