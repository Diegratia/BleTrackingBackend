using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddTrxVisitorExtendedTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "extended_visitor_time",
                table: "trx_visitor",
                type: "int",
                nullable: true);

            // Create evacuation alerts
            migrationBuilder.CreateTable(
                name: "evacuation_alerts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    alert_status = table.Column<string>(type: "nvarchar(255)", nullable: false),
                    trigger_type = table.Column<string>(type: "nvarchar(255)", nullable: false),
                    triggered_by = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    started_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    completed_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    completion_notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    completed_by = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    total_required = table.Column<int>(type: "int", nullable: false),
                    total_evacuated = table.Column<int>(type: "int", nullable: false),
                    total_confirmed = table.Column<int>(type: "int", nullable: false),
                    total_safe = table.Column<int>(type: "int", nullable: false),
                    total_remaining = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
                    application_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    created_by = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_by = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_evacuation_alerts", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "evacuation_assembly_points",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    area_shape = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    color = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    floorplan_masked_area_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: true),
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
                    table.PrimaryKey("PK_evacuation_assembly_points", x => x.id);
                    table.ForeignKey(
                        name: "FK_evacuation_assembly_points_floorplan_masked_area_floorplan_masked_area_id",
                        column: x => x.floorplan_masked_area_id,
                        principalTable: "floorplan_masked_area",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_evacuation_assembly_points_mst_floor_floor_id",
                        column: x => x.floor_id,
                        principalTable: "mst_floor",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_evacuation_assembly_points_mst_floorplan_floorplan_id",
                        column: x => x.floorplan_id,
                        principalTable: "mst_floorplan",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "evacuation_transactions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    evacuation_alert_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    evacuation_assembly_point_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    person_category = table.Column<string>(type: "nvarchar(255)", nullable: false),
                    member_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: true),
                    visitor_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: true),
                    security_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: true),
                    card_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: true),
                    person_status = table.Column<string>(type: "nvarchar(255)", nullable: false),
                    detected_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    last_detected_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    confirmed_by = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    confirmed_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    confirmation_notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    status = table.Column<int>(type: "int", nullable: false),
                    application_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    created_by = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_by = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_evacuation_transactions", x => x.id);
                    table.ForeignKey(
                        name: "FK_evacuation_transactions_card_card_id",
                        column: x => x.card_id,
                        principalTable: "card",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_evacuation_transactions_evacuation_alerts_evacuation_alert_id",
                        column: x => x.evacuation_alert_id,
                        principalTable: "evacuation_alerts",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_evacuation_transactions_evacuation_assembly_points_evacuation_assembly_point_id",
                        column: x => x.evacuation_assembly_point_id,
                        principalTable: "evacuation_assembly_points",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_evacuation_transactions_mst_member_member_id",
                        column: x => x.member_id,
                        principalTable: "mst_member",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_evacuation_transactions_mst_security_security_id",
                        column: x => x.security_id,
                        principalTable: "mst_security",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_evacuation_transactions_visitor_visitor_id",
                        column: x => x.visitor_id,
                        principalTable: "visitor",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_evacuation_assembly_points_floor_id",
                table: "evacuation_assembly_points",
                column: "floor_id");

            migrationBuilder.CreateIndex(
                name: "IX_evacuation_assembly_points_floorplan_id",
                table: "evacuation_assembly_points",
                column: "floorplan_id");

            migrationBuilder.CreateIndex(
                name: "IX_evacuation_assembly_points_floorplan_masked_area_id",
                table: "evacuation_assembly_points",
                column: "floorplan_masked_area_id");

            migrationBuilder.CreateIndex(
                name: "IX_evacuation_transactions_card_id",
                table: "evacuation_transactions",
                column: "card_id");

            migrationBuilder.CreateIndex(
                name: "IX_evacuation_transactions_evacuation_alert_id",
                table: "evacuation_transactions",
                column: "evacuation_alert_id");

            migrationBuilder.CreateIndex(
                name: "IX_evacuation_transactions_evacuation_assembly_point_id",
                table: "evacuation_transactions",
                column: "evacuation_assembly_point_id");

            migrationBuilder.CreateIndex(
                name: "IX_evacuation_transactions_member_id",
                table: "evacuation_transactions",
                column: "member_id");

            migrationBuilder.CreateIndex(
                name: "IX_evacuation_transactions_security_id",
                table: "evacuation_transactions",
                column: "security_id");

            migrationBuilder.CreateIndex(
                name: "IX_evacuation_transactions_visitor_id",
                table: "evacuation_transactions",
                column: "visitor_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "evacuation_transactions");

            migrationBuilder.DropTable(
                name: "evacuation_alerts");

            migrationBuilder.DropTable(
                name: "evacuation_assembly_points");
        }
    }
}
