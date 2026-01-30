using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class PatrolSessionSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "distance_from_prev",
                table: "patrol_checkpoint_log");

            migrationBuilder.AddColumn<string>(
                name: "patrol_assignment_name_snap",
                table: "patrol_session",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "patrol_route_name_snap",
                table: "patrol_session",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "security_card_number_snap",
                table: "patrol_session",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "security_identity_id_snap",
                table: "patrol_session",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "security_name_snap",
                table: "patrol_session",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "time_group_id",
                table: "patrol_session",
                type: "uniqueidentifier",
                maxLength: 36,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "time_group_name_snap",
                table: "patrol_session",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "area_name_snapshot",
                table: "patrol_checkpoint_log",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "distance_from_prev_meters",
                table: "patrol_checkpoint_log",
                type: "float",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_patrol_session_time_group_id",
                table: "patrol_session",
                column: "time_group_id");

            migrationBuilder.AddForeignKey(
                name: "FK_patrol_session_time_group_time_group_id",
                table: "patrol_session",
                column: "time_group_id",
                principalTable: "time_group",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_patrol_session_time_group_time_group_id",
                table: "patrol_session");

            migrationBuilder.DropIndex(
                name: "IX_patrol_session_time_group_id",
                table: "patrol_session");

            migrationBuilder.DropColumn(
                name: "patrol_assignment_name_snap",
                table: "patrol_session");

            migrationBuilder.DropColumn(
                name: "patrol_route_name_snap",
                table: "patrol_session");

            migrationBuilder.DropColumn(
                name: "security_card_number_snap",
                table: "patrol_session");

            migrationBuilder.DropColumn(
                name: "security_identity_id_snap",
                table: "patrol_session");

            migrationBuilder.DropColumn(
                name: "security_name_snap",
                table: "patrol_session");

            migrationBuilder.DropColumn(
                name: "time_group_id",
                table: "patrol_session");

            migrationBuilder.DropColumn(
                name: "time_group_name_snap",
                table: "patrol_session");

            migrationBuilder.DropColumn(
                name: "area_name_snapshot",
                table: "patrol_checkpoint_log");

            migrationBuilder.DropColumn(
                name: "distance_from_prev_meters",
                table: "patrol_checkpoint_log");

            migrationBuilder.AddColumn<DateTime>(
                name: "distance_from_prev",
                table: "patrol_checkpoint_log",
                type: "datetime2",
                nullable: true);
        }
    }
}
