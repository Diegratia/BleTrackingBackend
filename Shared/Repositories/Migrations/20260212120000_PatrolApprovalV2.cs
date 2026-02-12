using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class PatrolApprovalV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_patrol_case_mst_security_approved_by_head_id",
                table: "patrol_case");

            migrationBuilder.DropIndex(
                name: "IX_patrol_case_approved_by_head_id",
                table: "patrol_case");

            migrationBuilder.DropColumn(
                name: "approved_by_head_id",
                table: "patrol_case");

            migrationBuilder.AddColumn<Guid>(
                name: "security_head_1",
                table: "mst_security",
                type: "uniqueidentifier",
                maxLength: 36,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "security_head_2",
                table: "mst_security",
                type: "uniqueidentifier",
                maxLength: 36,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "approval_type",
                table: "patrol_assignment",
                type: "nvarchar(255)",
                nullable: false,
                defaultValue: "withoutapproval");

            migrationBuilder.AddColumn<Guid>(
                name: "security_head_1",
                table: "patrol_case",
                type: "uniqueidentifier",
                maxLength: 36,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "security_head_2",
                table: "patrol_case",
                type: "uniqueidentifier",
                maxLength: 36,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "approval_type",
                table: "patrol_case",
                type: "nvarchar(255)",
                nullable: false,
                defaultValue: "withoutapproval");

            migrationBuilder.AddColumn<Guid>(
                name: "approved_by_head_1_id",
                table: "patrol_case",
                type: "uniqueidentifier",
                maxLength: 36,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "approved_by_head_2_id",
                table: "patrol_case",
                type: "uniqueidentifier",
                maxLength: 36,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_mst_security_security_head_1",
                table: "mst_security",
                column: "security_head_1");

            migrationBuilder.CreateIndex(
                name: "IX_mst_security_security_head_2",
                table: "mst_security",
                column: "security_head_2");

            migrationBuilder.CreateIndex(
                name: "IX_patrol_case_security_head_1",
                table: "patrol_case",
                column: "security_head_1");

            migrationBuilder.CreateIndex(
                name: "IX_patrol_case_security_head_2",
                table: "patrol_case",
                column: "security_head_2");

            migrationBuilder.CreateIndex(
                name: "IX_patrol_case_approved_by_head_1_id",
                table: "patrol_case",
                column: "approved_by_head_1_id");

            migrationBuilder.CreateIndex(
                name: "IX_patrol_case_approved_by_head_2_id",
                table: "patrol_case",
                column: "approved_by_head_2_id");

            migrationBuilder.AddForeignKey(
                name: "FK_mst_security_mst_security_security_head_1",
                table: "mst_security",
                column: "security_head_1",
                principalTable: "mst_security",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_mst_security_mst_security_security_head_2",
                table: "mst_security",
                column: "security_head_2",
                principalTable: "mst_security",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_patrol_case_mst_security_approved_by_head_1_id",
                table: "patrol_case",
                column: "approved_by_head_1_id",
                principalTable: "mst_security",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_patrol_case_mst_security_approved_by_head_2_id",
                table: "patrol_case",
                column: "approved_by_head_2_id",
                principalTable: "mst_security",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_patrol_case_mst_security_security_head_1",
                table: "patrol_case",
                column: "security_head_1",
                principalTable: "mst_security",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_patrol_case_mst_security_security_head_2",
                table: "patrol_case",
                column: "security_head_2",
                principalTable: "mst_security",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_mst_security_mst_security_security_head_1",
                table: "mst_security");

            migrationBuilder.DropForeignKey(
                name: "FK_mst_security_mst_security_security_head_2",
                table: "mst_security");

            migrationBuilder.DropForeignKey(
                name: "FK_patrol_case_mst_security_approved_by_head_1_id",
                table: "patrol_case");

            migrationBuilder.DropForeignKey(
                name: "FK_patrol_case_mst_security_approved_by_head_2_id",
                table: "patrol_case");

            migrationBuilder.DropForeignKey(
                name: "FK_patrol_case_mst_security_security_head_1",
                table: "patrol_case");

            migrationBuilder.DropForeignKey(
                name: "FK_patrol_case_mst_security_security_head_2",
                table: "patrol_case");

            migrationBuilder.DropIndex(
                name: "IX_mst_security_security_head_1",
                table: "mst_security");

            migrationBuilder.DropIndex(
                name: "IX_mst_security_security_head_2",
                table: "mst_security");

            migrationBuilder.DropIndex(
                name: "IX_patrol_case_security_head_1",
                table: "patrol_case");

            migrationBuilder.DropIndex(
                name: "IX_patrol_case_security_head_2",
                table: "patrol_case");

            migrationBuilder.DropIndex(
                name: "IX_patrol_case_approved_by_head_1_id",
                table: "patrol_case");

            migrationBuilder.DropIndex(
                name: "IX_patrol_case_approved_by_head_2_id",
                table: "patrol_case");

            migrationBuilder.DropColumn(
                name: "security_head_1",
                table: "mst_security");

            migrationBuilder.DropColumn(
                name: "security_head_2",
                table: "mst_security");

            migrationBuilder.DropColumn(
                name: "approval_type",
                table: "patrol_assignment");

            migrationBuilder.DropColumn(
                name: "security_head_1",
                table: "patrol_case");

            migrationBuilder.DropColumn(
                name: "security_head_2",
                table: "patrol_case");

            migrationBuilder.DropColumn(
                name: "approval_type",
                table: "patrol_case");

            migrationBuilder.DropColumn(
                name: "approved_by_head_1_id",
                table: "patrol_case");

            migrationBuilder.DropColumn(
                name: "approved_by_head_2_id",
                table: "patrol_case");

            migrationBuilder.AddColumn<Guid>(
                name: "approved_by_head_id",
                table: "patrol_case",
                type: "uniqueidentifier",
                maxLength: 36,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_patrol_case_approved_by_head_id",
                table: "patrol_case",
                column: "approved_by_head_id");

            migrationBuilder.AddForeignKey(
                name: "FK_patrol_case_mst_security_approved_by_head_id",
                table: "patrol_case",
                column: "approved_by_head_id",
                principalTable: "mst_security",
                principalColumn: "id");
        }
    }
}
