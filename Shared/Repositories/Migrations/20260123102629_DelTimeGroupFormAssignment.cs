using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class DelTimeGroupFormAssignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_patrol_assignment_time_group_time_group_id",
                table: "patrol_assignment");

            migrationBuilder.DropIndex(
                name: "IX_patrol_assignment_time_group_id",
                table: "patrol_assignment");

            migrationBuilder.DropColumn(
                name: "time_group_id",
                table: "patrol_assignment");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "time_group_id",
                table: "patrol_assignment",
                type: "uniqueidentifier",
                maxLength: 36,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_patrol_assignment_time_group_id",
                table: "patrol_assignment",
                column: "time_group_id");

            migrationBuilder.AddForeignKey(
                name: "FK_patrol_assignment_time_group_time_group_id",
                table: "patrol_assignment",
                column: "time_group_id",
                principalTable: "time_group",
                principalColumn: "id");
        }
    }
}
