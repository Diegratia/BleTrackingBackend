using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class PatrolPlanningEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "patrol_assignment",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    patrol_route_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: true),
                    time_group_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: true),
                    start_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    end_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_by = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_by = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    application_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_patrol_assignment", x => x.id);
                    table.ForeignKey(
                        name: "FK_patrol_assignment_mst_application_application_id",
                        column: x => x.application_id,
                        principalTable: "mst_application",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_patrol_assignment_patrol_route_patrol_route_id",
                        column: x => x.patrol_route_id,
                        principalTable: "patrol_route",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_patrol_assignment_time_group_time_group_id",
                        column: x => x.time_group_id,
                        principalTable: "time_group",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "security_group",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_by = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_by = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    application_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_security_group", x => x.id);
                    table.ForeignKey(
                        name: "FK_security_group_mst_application_application_id",
                        column: x => x.application_id,
                        principalTable: "mst_application",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "patrol_assignment_security",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    patrol_assignment_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    security_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    created_by = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_by = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    application_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_patrol_assignment_security", x => x.id);
                    table.ForeignKey(
                        name: "FK_patrol_assignment_security_mst_application_application_id",
                        column: x => x.application_id,
                        principalTable: "mst_application",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_patrol_assignment_security_mst_security_security_id",
                        column: x => x.security_id,
                        principalTable: "mst_security",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_patrol_assignment_security_patrol_assignment_patrol_assignment_id",
                        column: x => x.patrol_assignment_id,
                        principalTable: "patrol_assignment",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "security_group_member",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    security_group_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: true),
                    security_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: true),
                    created_by = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_by = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    application_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_security_group_member", x => x.id);
                    table.ForeignKey(
                        name: "FK_security_group_member_mst_application_application_id",
                        column: x => x.application_id,
                        principalTable: "mst_application",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_security_group_member_mst_security_security_id",
                        column: x => x.security_id,
                        principalTable: "mst_security",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_security_group_member_security_group_security_group_id",
                        column: x => x.security_group_id,
                        principalTable: "security_group",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_patrol_assignment_application_id",
                table: "patrol_assignment",
                column: "application_id");

            migrationBuilder.CreateIndex(
                name: "IX_patrol_assignment_patrol_route_id",
                table: "patrol_assignment",
                column: "patrol_route_id");

            migrationBuilder.CreateIndex(
                name: "IX_patrol_assignment_time_group_id",
                table: "patrol_assignment",
                column: "time_group_id");

            migrationBuilder.CreateIndex(
                name: "IX_patrol_assignment_security_application_id",
                table: "patrol_assignment_security",
                column: "application_id");

            migrationBuilder.CreateIndex(
                name: "IX_patrol_assignment_security_patrol_assignment_id",
                table: "patrol_assignment_security",
                column: "patrol_assignment_id");

            migrationBuilder.CreateIndex(
                name: "IX_patrol_assignment_security_security_id",
                table: "patrol_assignment_security",
                column: "security_id");

            migrationBuilder.CreateIndex(
                name: "IX_security_group_application_id",
                table: "security_group",
                column: "application_id");

            migrationBuilder.CreateIndex(
                name: "IX_security_group_member_application_id",
                table: "security_group_member",
                column: "application_id");

            migrationBuilder.CreateIndex(
                name: "IX_security_group_member_security_group_id",
                table: "security_group_member",
                column: "security_group_id");

            migrationBuilder.CreateIndex(
                name: "IX_security_group_member_security_id",
                table: "security_group_member",
                column: "security_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "patrol_assignment_security");

            migrationBuilder.DropTable(
                name: "security_group_member");

            migrationBuilder.DropTable(
                name: "patrol_assignment");

            migrationBuilder.DropTable(
                name: "security_group");
        }
    }
}
