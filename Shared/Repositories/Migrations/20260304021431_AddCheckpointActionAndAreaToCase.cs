using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddCheckpointActionAndAreaToCase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "checkpoint_status",
                table: "patrol_checkpoint_log",
                type: "nvarchar(255)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "cleared_at",
                table: "patrol_checkpoint_log",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "notes",
                table: "patrol_checkpoint_log",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "patrol_area_id",
                table: "patrol_case",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "patrol_area_name_snap",
                table: "patrol_case",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_patrol_case_patrol_area_id",
                table: "patrol_case",
                column: "patrol_area_id");

            migrationBuilder.AddForeignKey(
                name: "FK_patrol_case_patrol_area_patrol_area_id",
                table: "patrol_case",
                column: "patrol_area_id",
                principalTable: "patrol_area",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_patrol_case_patrol_area_patrol_area_id",
                table: "patrol_case");

            migrationBuilder.DropIndex(
                name: "IX_patrol_case_patrol_area_id",
                table: "patrol_case");

            migrationBuilder.DropColumn(
                name: "checkpoint_status",
                table: "patrol_checkpoint_log");

            migrationBuilder.DropColumn(
                name: "cleared_at",
                table: "patrol_checkpoint_log");

            migrationBuilder.DropColumn(
                name: "notes",
                table: "patrol_checkpoint_log");

            migrationBuilder.DropColumn(
                name: "patrol_area_id",
                table: "patrol_case");

            migrationBuilder.DropColumn(
                name: "patrol_area_name_snap",
                table: "patrol_case");
        }
    }
}
