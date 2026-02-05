using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class buildingIdOnMonitringConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "building_id",
                table: "monitoring_config",
                type: "uniqueidentifier",
                maxLength: 36,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_monitoring_config_building_id",
                table: "monitoring_config",
                column: "building_id");

            migrationBuilder.AddForeignKey(
                name: "FK_monitoring_config_mst_building_building_id",
                table: "monitoring_config",
                column: "building_id",
                principalTable: "mst_building",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
