using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class addMemberatAlarmRecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "member_id",
                table: "alarm_record_tracking",
                type: "uniqueidentifier",
                maxLength: 36,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_alarm_record_tracking_member_id",
                table: "alarm_record_tracking",
                column: "member_id");

            migrationBuilder.AddForeignKey(
                name: "FK_alarm_record_tracking_mst_member_member_id",
                table: "alarm_record_tracking",
                column: "member_id",
                principalTable: "mst_member",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_alarm_record_tracking_mst_member_member_id",
                table: "alarm_record_tracking");

            migrationBuilder.DropIndex(
                name: "IX_alarm_record_tracking_member_id",
                table: "alarm_record_tracking");

            migrationBuilder.DropColumn(
                name: "member_id",
                table: "alarm_record_tracking");
        }
    }
}
