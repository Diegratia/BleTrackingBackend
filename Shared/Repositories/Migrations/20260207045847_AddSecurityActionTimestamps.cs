using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddSecurityActionTimestamps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "acknowledged_at",
                table: "alarm_triggers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "acknowledged_by",
                table: "alarm_triggers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "arrived_at",
                table: "alarm_triggers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "arrived_by",
                table: "alarm_triggers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "en_route_at",
                table: "alarm_triggers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "en_route_by",
                table: "alarm_triggers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "acknowledged_at",
                table: "alarm_triggers");

            migrationBuilder.DropColumn(
                name: "acknowledged_by",
                table: "alarm_triggers");

            migrationBuilder.DropColumn(
                name: "arrived_at",
                table: "alarm_triggers");

            migrationBuilder.DropColumn(
                name: "arrived_by",
                table: "alarm_triggers");

            migrationBuilder.DropColumn(
                name: "en_route_at",
                table: "alarm_triggers");

            migrationBuilder.DropColumn(
                name: "en_route_by",
                table: "alarm_triggers");
        }
    }
}
