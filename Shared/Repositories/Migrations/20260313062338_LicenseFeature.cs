using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class LicenseFeature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "customer_name",
                table: "mst_application",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "enabled_features",
                table: "mst_application",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "license_machine_id",
                table: "mst_application",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "license_tier",
                table: "mst_application",
                type: "nvarchar(255)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "max_beacons",
                table: "mst_application",
                type: "int",
                nullable: false,
                defaultValue: 20);

            migrationBuilder.AddColumn<int>(
                name: "max_readers",
                table: "mst_application",
                type: "int",
                nullable: false,
                defaultValue: 5);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "customer_name",
                table: "mst_application");

            migrationBuilder.DropColumn(
                name: "enabled_features",
                table: "mst_application");

            migrationBuilder.DropColumn(
                name: "license_machine_id",
                table: "mst_application");

            migrationBuilder.DropColumn(
                name: "license_tier",
                table: "mst_application");

            migrationBuilder.DropColumn(
                name: "max_beacons",
                table: "mst_application");

            migrationBuilder.DropColumn(
                name: "max_readers",
                table: "mst_application");
        }
    }
}
