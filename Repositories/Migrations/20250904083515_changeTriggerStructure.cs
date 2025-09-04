using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class changeTriggerStructure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_alarm_record_tracking_AlarmTriggers_alarm_triggers_id",
                table: "alarm_record_tracking");

            migrationBuilder.DropForeignKey(
                name: "FK_AlarmTriggers_mst_application_application_id",
                table: "AlarmTriggers");

            migrationBuilder.DropForeignKey(
                name: "FK_AlarmTriggers_mst_floorplan_floorplan_id",
                table: "AlarmTriggers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AlarmTriggers",
                table: "AlarmTriggers");

            migrationBuilder.RenameTable(
                name: "AlarmTriggers",
                newName: "alarm_triggers");

            migrationBuilder.RenameIndex(
                name: "IX_AlarmTriggers_floorplan_id",
                table: "alarm_triggers",
                newName: "IX_alarm_triggers_floorplan_id");

            migrationBuilder.RenameIndex(
                name: "IX_AlarmTriggers_application_id",
                table: "alarm_triggers",
                newName: "IX_alarm_triggers_application_id");

            migrationBuilder.AlterColumn<Guid>(
                name: "visitor_id",
                table: "alarm_record_tracking",
                type: "uniqueidentifier",
                maxLength: 36,
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldMaxLength: 36);

            migrationBuilder.AlterColumn<Guid>(
                name: "floorplan_masked_area_id",
                table: "alarm_record_tracking",
                type: "uniqueidentifier",
                maxLength: 36,
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldMaxLength: 36);

            migrationBuilder.AlterColumn<Guid>(
                name: "ble_reader_id",
                table: "alarm_record_tracking",
                type: "uniqueidentifier",
                maxLength: 36,
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldMaxLength: 36);

            migrationBuilder.AlterColumn<Guid>(
                name: "alarm_triggers_id",
                table: "alarm_record_tracking",
                type: "uniqueidentifier",
                maxLength: 36,
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldMaxLength: 36);

            migrationBuilder.AlterColumn<string>(
                name: "alarm_record_status",
                table: "alarm_record_tracking",
                type: "nvarchar(255)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)");

            migrationBuilder.AlterColumn<string>(
                name: "action",
                table: "alarm_record_tracking",
                type: "nvarchar(255)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)");

            migrationBuilder.AlterColumn<string>(
                name: "alarm_record_status",
                table: "alarm_triggers",
                type: "nvarchar(255)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)");

            migrationBuilder.AlterColumn<string>(
                name: "action",
                table: "alarm_triggers",
                type: "nvarchar(255)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)");

            migrationBuilder.AddColumn<string>(
                name: "cancel_by",
                table: "alarm_triggers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "cancel_timestamp",
                table: "alarm_triggers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "done_by",
                table: "alarm_triggers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "done_timestamp",
                table: "alarm_triggers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "idle_by",
                table: "alarm_triggers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "idle_timestamp",
                table: "alarm_triggers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "investigated_by",
                table: "alarm_triggers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "investigated_done_at",
                table: "alarm_triggers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "investigated_result",
                table: "alarm_triggers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "investigated_timestamp",
                table: "alarm_triggers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "waiting_by",
                table: "alarm_triggers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "waiting_timestamp",
                table: "alarm_triggers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_alarm_triggers",
                table: "alarm_triggers",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_alarm_record_tracking_alarm_triggers_alarm_triggers_id",
                table: "alarm_record_tracking",
                column: "alarm_triggers_id",
                principalTable: "alarm_triggers",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_alarm_triggers_mst_application_application_id",
                table: "alarm_triggers",
                column: "application_id",
                principalTable: "mst_application",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_alarm_triggers_mst_floorplan_floorplan_id",
                table: "alarm_triggers",
                column: "floorplan_id",
                principalTable: "mst_floorplan",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_alarm_record_tracking_alarm_triggers_alarm_triggers_id",
                table: "alarm_record_tracking");

            migrationBuilder.DropForeignKey(
                name: "FK_alarm_triggers_mst_application_application_id",
                table: "alarm_triggers");

            migrationBuilder.DropForeignKey(
                name: "FK_alarm_triggers_mst_floorplan_floorplan_id",
                table: "alarm_triggers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_alarm_triggers",
                table: "alarm_triggers");

            migrationBuilder.DropColumn(
                name: "cancel_by",
                table: "alarm_triggers");

            migrationBuilder.DropColumn(
                name: "cancel_timestamp",
                table: "alarm_triggers");

            migrationBuilder.DropColumn(
                name: "done_by",
                table: "alarm_triggers");

            migrationBuilder.DropColumn(
                name: "done_timestamp",
                table: "alarm_triggers");

            migrationBuilder.DropColumn(
                name: "idle_by",
                table: "alarm_triggers");

            migrationBuilder.DropColumn(
                name: "idle_timestamp",
                table: "alarm_triggers");

            migrationBuilder.DropColumn(
                name: "investigated_by",
                table: "alarm_triggers");

            migrationBuilder.DropColumn(
                name: "investigated_done_at",
                table: "alarm_triggers");

            migrationBuilder.DropColumn(
                name: "investigated_result",
                table: "alarm_triggers");

            migrationBuilder.DropColumn(
                name: "investigated_timestamp",
                table: "alarm_triggers");

            migrationBuilder.DropColumn(
                name: "waiting_by",
                table: "alarm_triggers");

            migrationBuilder.DropColumn(
                name: "waiting_timestamp",
                table: "alarm_triggers");

            migrationBuilder.RenameTable(
                name: "alarm_triggers",
                newName: "AlarmTriggers");

            migrationBuilder.RenameIndex(
                name: "IX_alarm_triggers_floorplan_id",
                table: "AlarmTriggers",
                newName: "IX_AlarmTriggers_floorplan_id");

            migrationBuilder.RenameIndex(
                name: "IX_alarm_triggers_application_id",
                table: "AlarmTriggers",
                newName: "IX_AlarmTriggers_application_id");

            migrationBuilder.AlterColumn<Guid>(
                name: "visitor_id",
                table: "alarm_record_tracking",
                type: "uniqueidentifier",
                maxLength: 36,
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldMaxLength: 36,
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "floorplan_masked_area_id",
                table: "alarm_record_tracking",
                type: "uniqueidentifier",
                maxLength: 36,
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldMaxLength: 36,
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "ble_reader_id",
                table: "alarm_record_tracking",
                type: "uniqueidentifier",
                maxLength: 36,
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldMaxLength: 36,
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "alarm_triggers_id",
                table: "alarm_record_tracking",
                type: "uniqueidentifier",
                maxLength: 36,
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldMaxLength: 36,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "alarm_record_status",
                table: "alarm_record_tracking",
                type: "nvarchar(255)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "action",
                table: "alarm_record_tracking",
                type: "nvarchar(255)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "alarm_record_status",
                table: "AlarmTriggers",
                type: "nvarchar(255)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "action",
                table: "AlarmTriggers",
                type: "nvarchar(255)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_AlarmTriggers",
                table: "AlarmTriggers",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_alarm_record_tracking_AlarmTriggers_alarm_triggers_id",
                table: "alarm_record_tracking",
                column: "alarm_triggers_id",
                principalTable: "AlarmTriggers",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_AlarmTriggers_mst_application_application_id",
                table: "AlarmTriggers",
                column: "application_id",
                principalTable: "mst_application",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_AlarmTriggers_mst_floorplan_floorplan_id",
                table: "AlarmTriggers",
                column: "floorplan_id",
                principalTable: "mst_floorplan",
                principalColumn: "id");
        }
    }
}
