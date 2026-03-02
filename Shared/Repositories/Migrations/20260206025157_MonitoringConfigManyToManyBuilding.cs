using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class MonitoringConfigManyToManyBuilding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "monitoring_config_building_access",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    monitoring_config_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    building_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    application_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    created_by = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_by = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_monitoring_config_building_access", x => x.id);
                    table.ForeignKey(
                        name: "FK_monitoring_config_building_access_monitoring_config_monitoring_config_id",
                        column: x => x.monitoring_config_id,
                        principalTable: "monitoring_config",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_monitoring_config_building_access_mst_building_building_id",
                        column: x => x.building_id,
                        principalTable: "mst_building",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_monitoring_config_building_access_building_id",
                table: "monitoring_config_building_access",
                column: "building_id");

            migrationBuilder.CreateIndex(
                name: "IX_monitoring_config_building_access_monitoring_config_id",
                table: "monitoring_config_building_access",
                column: "monitoring_config_id");

            // Migrate existing building_id values to junction table
            migrationBuilder.Sql(
                @"INSERT INTO monitoring_config_building_access (
                    id, monitoring_config_id, building_id, application_id,
                    status, created_at, updated_at, created_by, updated_by
                )
                SELECT
                    NEWID() as id,
                    id as monitoring_config_id,
                    building_id as building_id,
                    application_id,
                    1 as status,
                    created_at, updated_at, created_by, updated_by
                FROM monitoring_config
                WHERE building_id IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "monitoring_config_building_access");
        }
    }
}
