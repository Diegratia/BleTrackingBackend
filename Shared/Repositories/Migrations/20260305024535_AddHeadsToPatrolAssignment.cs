using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddHeadsToPatrolAssignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "security_head_1",
                table: "patrol_assignment",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "security_head_2",
                table: "patrol_assignment",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_patrol_assignment_security_head_1",
                table: "patrol_assignment",
                column: "security_head_1");

            migrationBuilder.CreateIndex(
                name: "IX_patrol_assignment_security_head_2",
                table: "patrol_assignment",
                column: "security_head_2");

            migrationBuilder.AddForeignKey(
                name: "FK_patrol_assignment_mst_security_security_head_1",
                table: "patrol_assignment",
                column: "security_head_1",
                principalTable: "mst_security",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_patrol_assignment_mst_security_security_head_2",
                table: "patrol_assignment",
                column: "security_head_2",
                principalTable: "mst_security",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_patrol_assignment_mst_security_security_head_1",
                table: "patrol_assignment");

            migrationBuilder.DropForeignKey(
                name: "FK_patrol_assignment_mst_security_security_head_2",
                table: "patrol_assignment");

            migrationBuilder.DropIndex(
                name: "IX_patrol_assignment_security_head_1",
                table: "patrol_assignment");

            migrationBuilder.DropIndex(
                name: "IX_patrol_assignment_security_head_2",
                table: "patrol_assignment");

            migrationBuilder.DropColumn(
                name: "security_head_1",
                table: "patrol_assignment");

            migrationBuilder.DropColumn(
                name: "security_head_2",
                table: "patrol_assignment");
        }
    }
}
