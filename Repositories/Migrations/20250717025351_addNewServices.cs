using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class addNewServices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "block_by",
                table: "visitor");

            migrationBuilder.DropColumn(
                name: "checkin_by",
                table: "visitor");

            migrationBuilder.DropColumn(
                name: "checkout_by",
                table: "visitor");

            migrationBuilder.DropColumn(
                name: "deny_by",
                table: "visitor");

            migrationBuilder.DropColumn(
                name: "portal_key",
                table: "visitor");

            migrationBuilder.DropColumn(
                name: "reason_block",
                table: "visitor");

            migrationBuilder.DropColumn(
                name: "reason_deny",
                table: "visitor");

            migrationBuilder.DropColumn(
                name: "reason_unblock",
                table: "visitor");

            migrationBuilder.DropColumn(
                name: "registered_date",
                table: "visitor");

            migrationBuilder.DropColumn(
                name: "timestamp_blocked",
                table: "visitor");

            migrationBuilder.DropColumn(
                name: "timestamp_checked_in",
                table: "visitor");

            migrationBuilder.DropColumn(
                name: "timestamp_checked_out",
                table: "visitor");

            migrationBuilder.DropColumn(
                name: "timestamp_deny",
                table: "visitor");

            migrationBuilder.DropColumn(
                name: "timestamp_pre_registration",
                table: "visitor");

            migrationBuilder.DropColumn(
                name: "location_px_x",
                table: "mst_ble_reader");

            migrationBuilder.DropColumn(
                name: "location_px_y",
                table: "mst_ble_reader");

            migrationBuilder.DropColumn(
                name: "location_x",
                table: "mst_ble_reader");

            migrationBuilder.DropColumn(
                name: "location_y",
                table: "mst_ble_reader");

            migrationBuilder.DropColumn(
                name: "mac",
                table: "mst_ble_reader");

            migrationBuilder.DropColumn(
                name: "position_px_x",
                table: "floorplan_masked_area");

            migrationBuilder.DropColumn(
                name: "position_px_y",
                table: "floorplan_masked_area");

            migrationBuilder.DropColumn(
                name: "wide_area",
                table: "floorplan_masked_area");

            migrationBuilder.RenameColumn(
                name: "visitor_end",
                table: "visitor",
                newName: "visitor_period_start");

            migrationBuilder.RenameColumn(
                name: "visitor_arrival",
                table: "visitor",
                newName: "visitor_period_end");

            migrationBuilder.RenameColumn(
                name: "unblock_by",
                table: "visitor",
                newName: "visitor_type");

            migrationBuilder.RenameColumn(
                name: "timestamp_unblocked",
                table: "visitor",
                newName: "email_verification_send_at");

            migrationBuilder.AlterColumn<int>(
                name: "status",
                table: "visitor",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)");

            migrationBuilder.AddColumn<Guid>(
                name: "department_id",
                table: "visitor",
                type: "uniqueidentifier",
                maxLength: 36,
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "district_id",
                table: "visitor",
                type: "uniqueidentifier",
                maxLength: 36,
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "email_verification_token",
                table: "visitor",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "is_email_vervied",
                table: "visitor",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_employee",
                table: "visitor",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_vip",
                table: "visitor",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "organization_id",
                table: "visitor",
                type: "uniqueidentifier",
                maxLength: 36,
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<float>(
                name: "coordinate_y",
                table: "tracking_transaction",
                type: "real",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<float>(
                name: "coordinate_x",
                table: "tracking_transaction",
                type: "real",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<float>(
                name: "coordinate_px_y",
                table: "tracking_transaction",
                type: "real",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<float>(
                name: "coordinate_px_x",
                table: "tracking_transaction",
                type: "real",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<float>(
                name: "pixel_y",
                table: "mst_floor",
                type: "real",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<float>(
                name: "pixel_x",
                table: "mst_floor",
                type: "real",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<float>(
                name: "meter_per_px",
                table: "mst_floor",
                type: "real",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<float>(
                name: "floor_y",
                table: "mst_floor",
                type: "real",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<float>(
                name: "floor_x",
                table: "mst_floor",
                type: "real",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<float>(
                name: "pos_y",
                table: "floorplan_device",
                type: "real",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<float>(
                name: "pos_x",
                table: "floorplan_device",
                type: "real",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<float>(
                name: "pos_px_y",
                table: "floorplan_device",
                type: "real",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<float>(
                name: "pos_px_x",
                table: "floorplan_device",
                type: "real",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<float>(
                name: "distance_px",
                table: "ble_reader_node",
                type: "real",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<float>(
                name: "distance",
                table: "ble_reader_node",
                type: "real",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldMaxLength: 255);

            migrationBuilder.CreateTable(
                name: "trx_visitor",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    checked_in_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    checked_out_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    deny_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    block_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    unblock_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    checkin_by = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    checkout_by = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    deny_by = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    deny_reason = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    block_by = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    block_reason = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    visitor_type = table.Column<string>(type: "nvarchar(255)", nullable: false),
                    invitation_created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    visitor_group_code = table.Column<long>(type: "bigint", nullable: false),
                    visitor_number = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    visitor_code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    vehicle_plate_number = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    remarks = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    site_id = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    parking_id = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    visitor_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trx_visitor", x => x.id);
                    table.ForeignKey(
                        name: "FK_trx_visitor_visitor_visitor_id",
                        column: x => x.visitor_id,
                        principalTable: "visitor",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "visitor_card",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    number = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    card_type = table.Column<string>(type: "nvarchar(255)", nullable: false),
                    qr_code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    mac = table.Column<string>(type: "nvarchar(255)", nullable: false),
                    checkin_status = table.Column<int>(type: "int", nullable: false),
                    enable_status = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    site_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    is_member = table.Column<int>(type: "int", nullable: false),
                    application_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_visitor_card", x => x.id);
                    table.ForeignKey(
                        name: "FK_visitor_card_mst_application_application_id",
                        column: x => x.application_id,
                        principalTable: "mst_application",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "card_record",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    visitor_name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    card_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    visitor_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    member_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    checkin_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    checkout_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    checkin_by = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    checkout_by = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    checkout_site_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    checkin_site_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    visitor_type = table.Column<string>(type: "nvarchar(255)", nullable: false),
                    MstMemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    VisitorCardId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    VisitorId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_card_record", x => x.id);
                    table.ForeignKey(
                        name: "FK_card_record_mst_member_MstMemberId",
                        column: x => x.MstMemberId,
                        principalTable: "mst_member",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_card_record_mst_member_member_id",
                        column: x => x.member_id,
                        principalTable: "mst_member",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_card_record_visitor_VisitorId1",
                        column: x => x.VisitorId1,
                        principalTable: "visitor",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_card_record_visitor_card_VisitorCardId",
                        column: x => x.VisitorCardId,
                        principalTable: "visitor_card",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_card_record_visitor_card_card_id",
                        column: x => x.card_id,
                        principalTable: "visitor_card",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_card_record_visitor_visitor_id",
                        column: x => x.visitor_id,
                        principalTable: "visitor",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_visitor_department_id",
                table: "visitor",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "IX_visitor_district_id",
                table: "visitor",
                column: "district_id");

            migrationBuilder.CreateIndex(
                name: "IX_visitor_organization_id",
                table: "visitor",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "IX_card_record_card_id",
                table: "card_record",
                column: "card_id");

            migrationBuilder.CreateIndex(
                name: "IX_card_record_member_id",
                table: "card_record",
                column: "member_id");

            migrationBuilder.CreateIndex(
                name: "IX_card_record_MstMemberId",
                table: "card_record",
                column: "MstMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_card_record_visitor_id",
                table: "card_record",
                column: "visitor_id");

            migrationBuilder.CreateIndex(
                name: "IX_card_record_VisitorCardId",
                table: "card_record",
                column: "VisitorCardId");

            migrationBuilder.CreateIndex(
                name: "IX_card_record_VisitorId1",
                table: "card_record",
                column: "VisitorId1");

            migrationBuilder.CreateIndex(
                name: "IX_trx_visitor_visitor_id",
                table: "trx_visitor",
                column: "visitor_id");

            migrationBuilder.CreateIndex(
                name: "IX_visitor_card_application_id",
                table: "visitor_card",
                column: "application_id");

            migrationBuilder.AddForeignKey(
                name: "FK_visitor_mst_department_department_id",
                table: "visitor",
                column: "department_id",
                principalTable: "mst_department",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_visitor_mst_district_district_id",
                table: "visitor",
                column: "district_id",
                principalTable: "mst_district",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_visitor_mst_organization_organization_id",
                table: "visitor",
                column: "organization_id",
                principalTable: "mst_organization",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_visitor_mst_department_department_id",
                table: "visitor");

            migrationBuilder.DropForeignKey(
                name: "FK_visitor_mst_district_district_id",
                table: "visitor");

            migrationBuilder.DropForeignKey(
                name: "FK_visitor_mst_organization_organization_id",
                table: "visitor");

            migrationBuilder.DropTable(
                name: "card_record");

            migrationBuilder.DropTable(
                name: "trx_visitor");

            migrationBuilder.DropTable(
                name: "visitor_card");

            migrationBuilder.DropIndex(
                name: "IX_visitor_department_id",
                table: "visitor");

            migrationBuilder.DropIndex(
                name: "IX_visitor_district_id",
                table: "visitor");

            migrationBuilder.DropIndex(
                name: "IX_visitor_organization_id",
                table: "visitor");

            migrationBuilder.DropColumn(
                name: "department_id",
                table: "visitor");

            migrationBuilder.DropColumn(
                name: "district_id",
                table: "visitor");

            migrationBuilder.DropColumn(
                name: "email_verification_token",
                table: "visitor");

            migrationBuilder.DropColumn(
                name: "is_email_vervied",
                table: "visitor");

            migrationBuilder.DropColumn(
                name: "is_employee",
                table: "visitor");

            migrationBuilder.DropColumn(
                name: "is_vip",
                table: "visitor");

            migrationBuilder.DropColumn(
                name: "organization_id",
                table: "visitor");

            migrationBuilder.RenameColumn(
                name: "visitor_type",
                table: "visitor",
                newName: "unblock_by");

            migrationBuilder.RenameColumn(
                name: "visitor_period_start",
                table: "visitor",
                newName: "visitor_end");

            migrationBuilder.RenameColumn(
                name: "visitor_period_end",
                table: "visitor",
                newName: "visitor_arrival");

            migrationBuilder.RenameColumn(
                name: "email_verification_send_at",
                table: "visitor",
                newName: "timestamp_unblocked");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "visitor",
                type: "nvarchar(255)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "block_by",
                table: "visitor",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "checkin_by",
                table: "visitor",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "checkout_by",
                table: "visitor",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "deny_by",
                table: "visitor",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "portal_key",
                table: "visitor",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "reason_block",
                table: "visitor",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "reason_deny",
                table: "visitor",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "reason_unblock",
                table: "visitor",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "registered_date",
                table: "visitor",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "timestamp_blocked",
                table: "visitor",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "timestamp_checked_in",
                table: "visitor",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "timestamp_checked_out",
                table: "visitor",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "timestamp_deny",
                table: "visitor",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "timestamp_pre_registration",
                table: "visitor",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<decimal>(
                name: "coordinate_y",
                table: "tracking_transaction",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AlterColumn<decimal>(
                name: "coordinate_x",
                table: "tracking_transaction",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AlterColumn<long>(
                name: "coordinate_px_y",
                table: "tracking_transaction",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AlterColumn<long>(
                name: "coordinate_px_x",
                table: "tracking_transaction",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AlterColumn<long>(
                name: "pixel_y",
                table: "mst_floor",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AlterColumn<long>(
                name: "pixel_x",
                table: "mst_floor",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AlterColumn<decimal>(
                name: "meter_per_px",
                table: "mst_floor",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AlterColumn<long>(
                name: "floor_y",
                table: "mst_floor",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AlterColumn<long>(
                name: "floor_x",
                table: "mst_floor",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AddColumn<long>(
                name: "location_px_x",
                table: "mst_ble_reader",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "location_px_y",
                table: "mst_ble_reader",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<decimal>(
                name: "location_x",
                table: "mst_ble_reader",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "location_y",
                table: "mst_ble_reader",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "mac",
                table: "mst_ble_reader",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "position_px_x",
                table: "floorplan_masked_area",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "position_px_y",
                table: "floorplan_masked_area",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "wide_area",
                table: "floorplan_masked_area",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AlterColumn<decimal>(
                name: "pos_y",
                table: "floorplan_device",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AlterColumn<decimal>(
                name: "pos_x",
                table: "floorplan_device",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AlterColumn<decimal>(
                name: "pos_px_y",
                table: "floorplan_device",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AlterColumn<decimal>(
                name: "pos_px_x",
                table: "floorplan_device",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AlterColumn<decimal>(
                name: "distance_px",
                table: "ble_reader_node",
                type: "decimal(18,2)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(float),
                oldType: "real",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<decimal>(
                name: "distance",
                table: "ble_reader_node",
                type: "decimal(18,2)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(float),
                oldType: "real",
                oldMaxLength: 255);
        }
    }
}
