using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class ChangeFloorField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "engine_floor_id",
                table: "mst_floor");

            migrationBuilder.DropColumn(
                name: "floor_image",
                table: "mst_floor");

            migrationBuilder.DropColumn(
                name: "floor_x",
                table: "mst_floor");

            migrationBuilder.DropColumn(
                name: "floor_y",
                table: "mst_floor");

            migrationBuilder.DropColumn(
                name: "meter_per_px",
                table: "mst_floor");

            migrationBuilder.DropColumn(
                name: "pixel_x",
                table: "mst_floor");

            migrationBuilder.DropColumn(
                name: "pixel_y",
                table: "mst_floor");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "monitoring_config",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "monitoring_config",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "Config",
                table: "monitoring_config",
                newName: "config");

            migrationBuilder.AddColumn<long>(
                name: "engine_id",
                table: "mst_floorplan",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<float>(
                name: "floor_x",
                table: "mst_floorplan",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "floor_y",
                table: "mst_floorplan",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<string>(
                name: "floorplan_image",
                table: "mst_floorplan",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "meter_per_px",
                table: "mst_floorplan",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "pixel_x",
                table: "mst_floorplan",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "pixel_y",
                table: "mst_floorplan",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<string>(
                name: "remarks",
                table: "geofence",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "remarks",
                table: "alarm_category_settings",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "engine_id",
                table: "mst_floorplan");

            migrationBuilder.DropColumn(
                name: "floor_x",
                table: "mst_floorplan");

            migrationBuilder.DropColumn(
                name: "floor_y",
                table: "mst_floorplan");

            migrationBuilder.DropColumn(
                name: "floorplan_image",
                table: "mst_floorplan");

            migrationBuilder.DropColumn(
                name: "meter_per_px",
                table: "mst_floorplan");

            migrationBuilder.DropColumn(
                name: "pixel_x",
                table: "mst_floorplan");

            migrationBuilder.DropColumn(
                name: "pixel_y",
                table: "mst_floorplan");

            migrationBuilder.DropColumn(
                name: "remarks",
                table: "geofence");

            migrationBuilder.DropColumn(
                name: "remarks",
                table: "alarm_category_settings");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "monitoring_config",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "monitoring_config",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "config",
                table: "monitoring_config",
                newName: "Config");

            migrationBuilder.AddColumn<long>(
                name: "engine_floor_id",
                table: "mst_floor",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "floor_image",
                table: "mst_floor",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<float>(
                name: "floor_x",
                table: "mst_floor",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "floor_y",
                table: "mst_floor",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "meter_per_px",
                table: "mst_floor",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "pixel_x",
                table: "mst_floor",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "pixel_y",
                table: "mst_floor",
                type: "real",
                nullable: false,
                defaultValue: 0f);
        }
    }
}
