using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class GeofencesUtilityTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "boundary",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    area_shape = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    color = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    floorplan_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: true),
                    floor_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: true),
                    engine_id = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_boundary", x => x.id);
                    table.ForeignKey(
                        name: "FK_boundary_mst_application_application_id",
                        column: x => x.application_id,
                        principalTable: "mst_application",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_boundary_mst_floor_floor_id",
                        column: x => x.floor_id,
                        principalTable: "mst_floor",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_boundary_mst_floorplan_floorplan_id",
                        column: x => x.floorplan_id,
                        principalTable: "mst_floorplan",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "overpopulating",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    area_shape = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    color = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    floorplan_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: true),
                    floor_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: true),
                    engine_id = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_overpopulating", x => x.id);
                    table.ForeignKey(
                        name: "FK_overpopulating_mst_application_application_id",
                        column: x => x.application_id,
                        principalTable: "mst_application",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_overpopulating_mst_floor_floor_id",
                        column: x => x.floor_id,
                        principalTable: "mst_floor",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_overpopulating_mst_floorplan_floorplan_id",
                        column: x => x.floorplan_id,
                        principalTable: "mst_floorplan",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "stay_on_area",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    area_shape = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    color = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    floorplan_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: true),
                    floor_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: true),
                    engine_id = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_stay_on_area", x => x.id);
                    table.ForeignKey(
                        name: "FK_stay_on_area_mst_application_application_id",
                        column: x => x.application_id,
                        principalTable: "mst_application",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_stay_on_area_mst_floor_floor_id",
                        column: x => x.floor_id,
                        principalTable: "mst_floor",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_stay_on_area_mst_floorplan_floorplan_id",
                        column: x => x.floorplan_id,
                        principalTable: "mst_floorplan",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_boundary_application_id",
                table: "boundary",
                column: "application_id");

            migrationBuilder.CreateIndex(
                name: "IX_boundary_floor_id",
                table: "boundary",
                column: "floor_id");

            migrationBuilder.CreateIndex(
                name: "IX_boundary_floorplan_id",
                table: "boundary",
                column: "floorplan_id");

            migrationBuilder.CreateIndex(
                name: "IX_overpopulating_application_id",
                table: "overpopulating",
                column: "application_id");

            migrationBuilder.CreateIndex(
                name: "IX_overpopulating_floor_id",
                table: "overpopulating",
                column: "floor_id");

            migrationBuilder.CreateIndex(
                name: "IX_overpopulating_floorplan_id",
                table: "overpopulating",
                column: "floorplan_id");

            migrationBuilder.CreateIndex(
                name: "IX_stay_on_area_application_id",
                table: "stay_on_area",
                column: "application_id");

            migrationBuilder.CreateIndex(
                name: "IX_stay_on_area_floor_id",
                table: "stay_on_area",
                column: "floor_id");

            migrationBuilder.CreateIndex(
                name: "IX_stay_on_area_floorplan_id",
                table: "stay_on_area",
                column: "floorplan_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "boundary");

            migrationBuilder.DropTable(
                name: "overpopulating");

            migrationBuilder.DropTable(
                name: "stay_on_area");
        }
    }
}
