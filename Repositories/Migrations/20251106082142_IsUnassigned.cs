using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class IsUnassigned : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_assigned",
                table: "mst_ble_reader",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_assigned",
                table: "mst_access_control",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_assigned",
                table: "mst_access_cctv",
                type: "bit",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_assigned",
                table: "mst_ble_reader");

            migrationBuilder.DropColumn(
                name: "is_assigned",
                table: "mst_access_control");

            migrationBuilder.DropColumn(
                name: "is_assigned",
                table: "mst_access_cctv");
        }
    }
}
