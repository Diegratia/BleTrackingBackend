using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddCycleIndexToCheckpointLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "max_dwell_time",
                table: "patrol_assignment");

            migrationBuilder.DropColumn(
                name: "min_dwell_time",
                table: "patrol_assignment");

            migrationBuilder.AddColumn<int>(
                name: "cycle_index",
                table: "patrol_checkpoint_log",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "cycle_index",
                table: "patrol_checkpoint_log");

            migrationBuilder.AddColumn<int>(
                name: "max_dwell_time",
                table: "patrol_assignment",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "min_dwell_time",
                table: "patrol_assignment",
                type: "int",
                nullable: true);
        }
    }
}
