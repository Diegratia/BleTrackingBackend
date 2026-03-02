using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddMonitoringConfigPermissionFlags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "can_create_monitoring_config",
                table: "user",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "can_update_monitoring_config",
                table: "user",
                type: "bit",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "can_create_monitoring_config",
                table: "user");

            migrationBuilder.DropColumn(
                name: "can_update_monitoring_config",
                table: "user");
        }
    }
}
