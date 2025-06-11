using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class fixDiviceDataType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_alarm_record_tracking_visitor_visitor",
                table: "alarm_record_tracking");

            migrationBuilder.RenameColumn(
                name: "visitor",
                table: "alarm_record_tracking",
                newName: "visitor_id");

            migrationBuilder.RenameIndex(
                name: "IX_alarm_record_tracking_visitor",
                table: "alarm_record_tracking",
                newName: "IX_alarm_record_tracking_visitor_id");

            migrationBuilder.AlterColumn<decimal>(
                name: "pos_px_y",
                table: "floorplan_device",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<decimal>(
                name: "pos_px_x",
                table: "floorplan_device",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.CreateTable(
                name: "mst_tracking_log",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    beacon_id = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    pair = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    first_reader_id = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    second_reader_id = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    point_x = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    point_y = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    first_reader_x = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    first_reader_y = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    second_reader_x = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    second_reader_y = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    time = table.Column<byte[]>(type: "timestamp", nullable: false),
                    floorplan_device_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    floorplan_id = table.Column<Guid>(type: "uuid", nullable: false),
                    floor_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    floorplan_masked_area_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mst_tracking_log", x => x.id);
                    table.ForeignKey(
                        name: "FK_mst_tracking_log_floorplan_masked_area_floorplan_masked_area_id",
                        column: x => x.floorplan_masked_area_id,
                        principalTable: "floorplan_masked_area",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_mst_tracking_log_mst_floor_floor_id",
                        column: x => x.floor_id,
                        principalTable: "mst_floor",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_mst_tracking_log_mst_floorplan_floorplan_id",
                        column: x => x.floorplan_id,
                        principalTable: "mst_floorplan",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_mst_tracking_log_floorplan_device",
                        column: x => x.floorplan_device_id,
                        principalTable: "floorplan_device",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "record_tracking_log",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    table_name = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    floorplan_id = table.Column<Guid>(type: "uuid", nullable: false),
                    floorplan_timestamp = table.Column<byte[]>(type: "timestamp", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_record_tracking_log", x => x.id);
                    table.ForeignKey(
                        name: "fk_record_tracking_log_floorplan",
                        column: x => x.floorplan_id,
                        principalTable: "mst_floorplan",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_mst_tracking_log_floor_id",
                table: "mst_tracking_log",
                column: "floor_id");

            migrationBuilder.CreateIndex(
                name: "IX_mst_tracking_log_floorplan_device_id",
                table: "mst_tracking_log",
                column: "floorplan_device_id");

            migrationBuilder.CreateIndex(
                name: "IX_mst_tracking_log_floorplan_id",
                table: "mst_tracking_log",
                column: "floorplan_id");

            migrationBuilder.CreateIndex(
                name: "IX_mst_tracking_log_floorplan_masked_area_id",
                table: "mst_tracking_log",
                column: "floorplan_masked_area_id");

            migrationBuilder.CreateIndex(
                name: "IX_record_tracking_log_floorplan_id",
                table: "record_tracking_log",
                column: "floorplan_id");

            migrationBuilder.AddForeignKey(
                name: "FK_alarm_record_tracking_visitor_visitor_id",
                table: "alarm_record_tracking",
                column: "visitor_id",
                principalTable: "visitor",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_alarm_record_tracking_visitor_visitor_id",
                table: "alarm_record_tracking");

            migrationBuilder.DropTable(
                name: "mst_tracking_log");

            migrationBuilder.DropTable(
                name: "record_tracking_log");

            migrationBuilder.RenameColumn(
                name: "visitor_id",
                table: "alarm_record_tracking",
                newName: "visitor");

            migrationBuilder.RenameIndex(
                name: "IX_alarm_record_tracking_visitor_id",
                table: "alarm_record_tracking",
                newName: "IX_alarm_record_tracking_visitor");

            migrationBuilder.AlterColumn<long>(
                name: "pos_px_y",
                table: "floorplan_device",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<long>(
                name: "pos_px_x",
                table: "floorplan_device",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AddForeignKey(
                name: "FK_alarm_record_tracking_visitor_visitor",
                table: "alarm_record_tracking",
                column: "visitor",
                principalTable: "visitor",
                principalColumn: "id");
        }
    }
}
