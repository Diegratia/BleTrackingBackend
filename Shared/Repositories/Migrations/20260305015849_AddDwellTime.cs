using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddDwellTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "max_dwell_time",
                table: "patrol_route_areas",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "min_dwell_time",
                table: "patrol_route_areas",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "max_dwell_time",
                table: "patrol_checkpoint_log",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "min_dwell_time",
                table: "patrol_checkpoint_log",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "max_dwell_time",
                table: "patrol_route_areas");

            migrationBuilder.DropColumn(
                name: "min_dwell_time",
                table: "patrol_route_areas");

            migrationBuilder.DropColumn(
                name: "max_dwell_time",
                table: "patrol_checkpoint_log");

            migrationBuilder.DropColumn(
                name: "min_dwell_time",
                table: "patrol_checkpoint_log");
        }
    }
}
