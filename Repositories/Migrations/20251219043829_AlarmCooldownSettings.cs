using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AlarmCooldownSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "last_notified_at",
                table: "alarm_triggers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "alarm_cooldown",
                table: "alarm_category_settings",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "last_notified_at",
                table: "alarm_triggers");

            migrationBuilder.DropColumn(
                name: "alarm_cooldown",
                table: "alarm_category_settings");
        }
    }
}
