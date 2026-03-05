using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddPatrolShiftReplacement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "cycle_count",
                table: "patrol_assignment",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "cycle_type",
                table: "patrol_assignment",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "duration_type",
                table: "patrol_assignment",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "max_dwell_time",
                table: "patrol_assignment",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "min_dwell_time",
                table: "patrol_assignment",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "start_type",
                table: "patrol_assignment",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "patrol_shift_replacement",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    patrol_assignment_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    original_security_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    substitute_security_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    replacement_start_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    replacement_end_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_by = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_by = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    application_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_patrol_shift_replacement", x => x.id);
                    table.ForeignKey(
                        name: "FK_patrol_shift_replacement_mst_application_application_id",
                        column: x => x.application_id,
                        principalTable: "mst_application",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_patrol_shift_replacement_mst_security_original_security_id",
                        column: x => x.original_security_id,
                        principalTable: "mst_security",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_patrol_shift_replacement_mst_security_substitute_security_id",
                        column: x => x.substitute_security_id,
                        principalTable: "mst_security",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_patrol_shift_replacement_patrol_assignment_patrol_assignment_id",
                        column: x => x.patrol_assignment_id,
                        principalTable: "patrol_assignment",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_patrol_shift_replacement_application_id",
                table: "patrol_shift_replacement",
                column: "application_id");

            migrationBuilder.CreateIndex(
                name: "IX_patrol_shift_replacement_original_security_id",
                table: "patrol_shift_replacement",
                column: "original_security_id");

            migrationBuilder.CreateIndex(
                name: "IX_patrol_shift_replacement_patrol_assignment_id",
                table: "patrol_shift_replacement",
                column: "patrol_assignment_id");

            migrationBuilder.CreateIndex(
                name: "IX_patrol_shift_replacement_substitute_security_id",
                table: "patrol_shift_replacement",
                column: "substitute_security_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "patrol_shift_replacement");

            migrationBuilder.DropColumn(
                name: "cycle_count",
                table: "patrol_assignment");

            migrationBuilder.DropColumn(
                name: "cycle_type",
                table: "patrol_assignment");

            migrationBuilder.DropColumn(
                name: "duration_type",
                table: "patrol_assignment");

            migrationBuilder.DropColumn(
                name: "max_dwell_time",
                table: "patrol_assignment");

            migrationBuilder.DropColumn(
                name: "min_dwell_time",
                table: "patrol_assignment");

            migrationBuilder.DropColumn(
                name: "start_type",
                table: "patrol_assignment");
        }
    }
}
