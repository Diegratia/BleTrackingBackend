using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class removeEngineIdAtFloorplan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "engine_id",
                table: "stay_on_area");

            migrationBuilder.DropColumn(
                name: "engine_id",
                table: "overpopulating");

            migrationBuilder.DropColumn(
                name: "engine_id",
                table: "mst_floorplan");

            migrationBuilder.DropColumn(
                name: "engine_reader_id",
                table: "mst_ble_reader");

            migrationBuilder.DropColumn(
                name: "engine_id",
                table: "geofence");

            migrationBuilder.DropColumn(
                name: "engine_area_id",
                table: "floorplan_masked_area");

            migrationBuilder.DropColumn(
                name: "engine_id",
                table: "boundary");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "engine_id",
                table: "stay_on_area",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "engine_id",
                table: "overpopulating",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "engine_id",
                table: "mst_floorplan",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "engine_reader_id",
                table: "mst_ble_reader",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "engine_id",
                table: "geofence",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "engine_area_id",
                table: "floorplan_masked_area",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "engine_id",
                table: "boundary",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
