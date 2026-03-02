using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class PatrolAreaTimeGroups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "schedule_type",
                table: "time_group",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "patrol_route_time_groups",
                columns: table => new
                {
                    patrol_route_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    time_group_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    application_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_patrol_route_time_groups", x => new { x.patrol_route_id, x.time_group_id });
                    table.ForeignKey(
                        name: "FK_patrol_route_time_groups_patrol_route_patrol_route_id",
                        column: x => x.patrol_route_id,
                        principalTable: "patrol_route",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_patrol_route_time_groups_time_group_time_group_id",
                        column: x => x.time_group_id,
                        principalTable: "time_group",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_patrol_route_time_groups_time_group_id",
                table: "patrol_route_time_groups",
                column: "time_group_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "patrol_route_time_groups");

            migrationBuilder.DropColumn(
                name: "schedule_type",
                table: "time_group");
        }
    }
}
