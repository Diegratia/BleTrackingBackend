using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class PatrolAction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "patrol_session",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    patrol_route_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    patrol_assignment_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    security_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    started_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ended_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_by = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_by = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    application_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_patrol_session", x => x.id);
                    table.ForeignKey(
                        name: "FK_patrol_session_mst_application_application_id",
                        column: x => x.application_id,
                        principalTable: "mst_application",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_patrol_session_mst_security_security_id",
                        column: x => x.security_id,
                        principalTable: "mst_security",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_patrol_session_patrol_assignment_patrol_assignment_id",
                        column: x => x.patrol_assignment_id,
                        principalTable: "patrol_assignment",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_patrol_session_patrol_route_patrol_route_id",
                        column: x => x.patrol_route_id,
                        principalTable: "patrol_route",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "patrol_case",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    case_type = table.Column<int>(type: "int", nullable: false),
                    case_status = table.Column<int>(type: "int", nullable: false),
                    patrol_session_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: true),
                    security_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: true),
                    approved_by_head_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: true),
                    patrol_assignment_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    patrol_route_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: true),
                    created_by = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_by = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    application_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_patrol_case", x => x.id);
                    table.ForeignKey(
                        name: "FK_patrol_case_mst_application_application_id",
                        column: x => x.application_id,
                        principalTable: "mst_application",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_patrol_case_mst_security_approved_by_head_id",
                        column: x => x.approved_by_head_id,
                        principalTable: "mst_security",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_patrol_case_mst_security_security_id",
                        column: x => x.security_id,
                        principalTable: "mst_security",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_patrol_case_patrol_assignment_patrol_assignment_id",
                        column: x => x.patrol_assignment_id,
                        principalTable: "patrol_assignment",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_patrol_case_patrol_route_patrol_route_id",
                        column: x => x.patrol_route_id,
                        principalTable: "patrol_route",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_patrol_case_patrol_session_patrol_session_id",
                        column: x => x.patrol_session_id,
                        principalTable: "patrol_session",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "patrol_checkpoint_log",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    patrol_session_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: true),
                    patrol_area_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    order_index = table.Column<int>(type: "int", nullable: true),
                    arrived_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    left_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    distance_from_prev = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_by = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_by = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    application_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_patrol_checkpoint_log", x => x.id);
                    table.ForeignKey(
                        name: "FK_patrol_checkpoint_log_mst_application_application_id",
                        column: x => x.application_id,
                        principalTable: "mst_application",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_patrol_checkpoint_log_patrol_area_patrol_area_id",
                        column: x => x.patrol_area_id,
                        principalTable: "patrol_area",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_patrol_checkpoint_log_patrol_session_patrol_session_id",
                        column: x => x.patrol_session_id,
                        principalTable: "patrol_session",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "patrol_case_attachment",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    patrol_case_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: true),
                    file_url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    file_type = table.Column<string>(type: "nvarchar(255)", nullable: true),
                    mime_type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    uploaded_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_by = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_by = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    application_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_patrol_case_attachment", x => x.id);
                    table.ForeignKey(
                        name: "FK_patrol_case_attachment_mst_application_application_id",
                        column: x => x.application_id,
                        principalTable: "mst_application",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_patrol_case_attachment_patrol_case_patrol_case_id",
                        column: x => x.patrol_case_id,
                        principalTable: "patrol_case",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_patrol_case_application_id",
                table: "patrol_case",
                column: "application_id");

            migrationBuilder.CreateIndex(
                name: "IX_patrol_case_approved_by_head_id",
                table: "patrol_case",
                column: "approved_by_head_id");

            migrationBuilder.CreateIndex(
                name: "IX_patrol_case_patrol_assignment_id",
                table: "patrol_case",
                column: "patrol_assignment_id");

            migrationBuilder.CreateIndex(
                name: "IX_patrol_case_patrol_route_id",
                table: "patrol_case",
                column: "patrol_route_id");

            migrationBuilder.CreateIndex(
                name: "IX_patrol_case_patrol_session_id",
                table: "patrol_case",
                column: "patrol_session_id");

            migrationBuilder.CreateIndex(
                name: "IX_patrol_case_security_id",
                table: "patrol_case",
                column: "security_id");

            migrationBuilder.CreateIndex(
                name: "IX_patrol_case_attachment_application_id",
                table: "patrol_case_attachment",
                column: "application_id");

            migrationBuilder.CreateIndex(
                name: "IX_patrol_case_attachment_patrol_case_id",
                table: "patrol_case_attachment",
                column: "patrol_case_id");

            migrationBuilder.CreateIndex(
                name: "IX_patrol_checkpoint_log_application_id",
                table: "patrol_checkpoint_log",
                column: "application_id");

            migrationBuilder.CreateIndex(
                name: "IX_patrol_checkpoint_log_patrol_area_id",
                table: "patrol_checkpoint_log",
                column: "patrol_area_id");

            migrationBuilder.CreateIndex(
                name: "IX_patrol_checkpoint_log_patrol_session_id",
                table: "patrol_checkpoint_log",
                column: "patrol_session_id");

            migrationBuilder.CreateIndex(
                name: "IX_patrol_session_application_id",
                table: "patrol_session",
                column: "application_id");

            migrationBuilder.CreateIndex(
                name: "IX_patrol_session_patrol_assignment_id",
                table: "patrol_session",
                column: "patrol_assignment_id");

            migrationBuilder.CreateIndex(
                name: "IX_patrol_session_patrol_route_id",
                table: "patrol_session",
                column: "patrol_route_id");

            migrationBuilder.CreateIndex(
                name: "IX_patrol_session_security_id",
                table: "patrol_session",
                column: "security_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "patrol_case_attachment");

            migrationBuilder.DropTable(
                name: "patrol_checkpoint_log");

            migrationBuilder.DropTable(
                name: "patrol_case");

            migrationBuilder.DropTable(
                name: "patrol_session");
        }
    }
}
