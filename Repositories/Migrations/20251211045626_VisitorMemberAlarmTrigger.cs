using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class VisitorMemberAlarmTrigger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "member_id",
                table: "alarm_triggers",
                type: "uniqueidentifier",
                maxLength: 36,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "visitor_id",
                table: "alarm_triggers",
                type: "uniqueidentifier",
                maxLength: 36,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_alarm_triggers_member_id",
                table: "alarm_triggers",
                column: "member_id");

            migrationBuilder.CreateIndex(
                name: "IX_alarm_triggers_visitor_id",
                table: "alarm_triggers",
                column: "visitor_id");

            migrationBuilder.AddForeignKey(
                name: "FK_alarm_triggers_mst_member_member_id",
                table: "alarm_triggers",
                column: "member_id",
                principalTable: "mst_member",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_alarm_triggers_visitor_visitor_id",
                table: "alarm_triggers",
                column: "visitor_id",
                principalTable: "visitor",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_alarm_triggers_mst_member_member_id",
                table: "alarm_triggers");

            migrationBuilder.DropForeignKey(
                name: "FK_alarm_triggers_visitor_visitor_id",
                table: "alarm_triggers");

            migrationBuilder.DropIndex(
                name: "IX_alarm_triggers_member_id",
                table: "alarm_triggers");

            migrationBuilder.DropIndex(
                name: "IX_alarm_triggers_visitor_id",
                table: "alarm_triggers");

            migrationBuilder.DropColumn(
                name: "member_id",
                table: "alarm_triggers");

            migrationBuilder.DropColumn(
                name: "visitor_id",
                table: "alarm_triggers");
        }
    }
}
