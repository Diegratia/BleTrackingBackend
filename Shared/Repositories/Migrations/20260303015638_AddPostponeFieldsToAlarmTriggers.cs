using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddPostponeFieldsToAlarmTriggers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "postpone_reason",
                table: "alarm_triggers",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "postponed_at",
                table: "alarm_triggers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "postponed_by",
                table: "alarm_triggers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "postponed_until_date",
                table: "alarm_triggers",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "postpone_reason",
                table: "alarm_triggers");

            migrationBuilder.DropColumn(
                name: "postponed_at",
                table: "alarm_triggers");

            migrationBuilder.DropColumn(
                name: "postponed_by",
                table: "alarm_triggers");

            migrationBuilder.DropColumn(
                name: "postponed_until_date",
                table: "alarm_triggers");
        }
    }
}
