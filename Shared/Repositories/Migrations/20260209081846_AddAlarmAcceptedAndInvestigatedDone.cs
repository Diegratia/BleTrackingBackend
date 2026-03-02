using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddAlarmAcceptedAndInvestigatedDone : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add columns for security Accept action
            // Other columns (acknowledged_at/by, arrived_at/by, dispatched_at/by, investigated_done_at/by) already exist
            migrationBuilder.AddColumn<DateTime>(
                name: "accepted_at",
                table: "alarm_triggers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "accepted_by",
                table: "alarm_triggers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
            
            migrationBuilder.AddColumn<string>(
                name: "investigated_done_by",
                table: "alarm_triggers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "accepted_at",
                table: "alarm_triggers");

            migrationBuilder.DropColumn(
                name: "accepted_by",
                table: "alarm_triggers");
            
            migrationBuilder.DropColumn(
                name: "investigated_by",
                table: "alarm_triggers");
                
            migrationBuilder.DropColumn(
                name: "investigated_at",
                table: "alarm_triggers");
        }
    }
}
