using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddPermissionFlags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_head",
                table: "user_group",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "can_alarm_action",
                table: "user",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "can_approve_patrol",
                table: "user",
                type: "bit",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_head",
                table: "user_group");

            migrationBuilder.DropColumn(
                name: "can_alarm_action",
                table: "user");

            migrationBuilder.DropColumn(
                name: "can_approve_patrol",
                table: "user");
        }
    }
}
