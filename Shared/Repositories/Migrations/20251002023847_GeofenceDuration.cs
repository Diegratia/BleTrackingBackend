using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class GeofenceDuration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "max_duration",
                table: "stay_on_area",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "max_capacity",
                table: "overpopulating",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "max_duration",
                table: "stay_on_area");

            migrationBuilder.DropColumn(
                name: "max_capacity",
                table: "overpopulating");
        }
    }
}
