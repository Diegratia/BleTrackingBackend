using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class PatrolArea : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ble_reader_node");

            migrationBuilder.DropIndex(
                name: "IX_trx_visitor_visitor_id_visitor_period_start_visitor_period_end",
                table: "trx_visitor");

            migrationBuilder.CreateTable(
                name: "patrol_area",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    area_shape = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    color = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    floorplan_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: true),
                    floor_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: true),
                    is_active = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
                    application_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    created_by = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_by = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_patrol_area", x => x.id);
                    table.ForeignKey(
                        name: "FK_patrol_area_mst_application_application_id",
                        column: x => x.application_id,
                        principalTable: "mst_application",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_patrol_area_mst_floor_floor_id",
                        column: x => x.floor_id,
                        principalTable: "mst_floor",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_patrol_area_mst_floorplan_floorplan_id",
                        column: x => x.floorplan_id,
                        principalTable: "mst_floorplan",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_trx_visitor_visitor_id_visitor_period_start_visitor_period_end",
                table: "trx_visitor",
                columns: new[] { "visitor_id", "visitor_period_start", "visitor_period_end" });

            migrationBuilder.CreateIndex(
                name: "IX_patrol_area_application_id",
                table: "patrol_area",
                column: "application_id");

            migrationBuilder.CreateIndex(
                name: "IX_patrol_area_floor_id",
                table: "patrol_area",
                column: "floor_id");

            migrationBuilder.CreateIndex(
                name: "IX_patrol_area_floorplan_id",
                table: "patrol_area",
                column: "floorplan_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "patrol_area");

            migrationBuilder.DropIndex(
                name: "IX_trx_visitor_visitor_id_visitor_period_start_visitor_period_end",
                table: "trx_visitor");

            migrationBuilder.CreateTable(
                name: "ble_reader_node",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    application_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    ble_reader_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    distance = table.Column<float>(type: "real", nullable: false),
                    distance_px = table.Column<float>(type: "real", nullable: false),
                    end_pos = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    _generate = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    start_pos = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_by = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ble_reader_node", x => x.id);
                    table.ForeignKey(
                        name: "FK_ble_reader_node_mst_application_application_id",
                        column: x => x.application_id,
                        principalTable: "mst_application",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_ble_reader_node_mst_ble_reader_ble_reader_id",
                        column: x => x.ble_reader_id,
                        principalTable: "mst_ble_reader",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_trx_visitor_visitor_id_visitor_period_start_visitor_period_end",
                table: "trx_visitor",
                columns: new[] { "visitor_id", "visitor_period_start", "visitor_period_end" },
                unique: true,
                filter: "[visitor_id] IS NOT NULL AND [visitor_period_start] IS NOT NULL AND [visitor_period_end] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ble_reader_node__generate",
                table: "ble_reader_node",
                column: "_generate",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ble_reader_node_application_id",
                table: "ble_reader_node",
                column: "application_id");

            migrationBuilder.CreateIndex(
                name: "IX_ble_reader_node_ble_reader_id",
                table: "ble_reader_node",
                column: "ble_reader_id");
        }
    }
}
