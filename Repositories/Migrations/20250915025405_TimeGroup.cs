using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class TimeGroup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "time_group",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    application_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
                    _generate = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    created_by = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_by = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_time_group", x => x.id);
                    table.ForeignKey(
                        name: "FK_time_group_mst_application_application_id",
                        column: x => x.application_id,
                        principalTable: "mst_application",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "time_block",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    day_of_week = table.Column<int>(type: "int", nullable: true),
                    start_time = table.Column<TimeSpan>(type: "time", nullable: true),
                    end_time = table.Column<TimeSpan>(type: "time", nullable: true),
                    time_group_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: true),
                    application_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
                    TimeGroupId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    _generate = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    created_by = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_by = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_time_block", x => x.id);
                    table.ForeignKey(
                        name: "FK_time_block_mst_application_application_id",
                        column: x => x.application_id,
                        principalTable: "mst_application",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_time_block_time_group_TimeGroupId1",
                        column: x => x.TimeGroupId1,
                        principalTable: "time_group",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_time_block_time_group_time_group_id",
                        column: x => x.time_group_id,
                        principalTable: "time_group",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_time_block_application_id",
                table: "time_block",
                column: "application_id");

            migrationBuilder.CreateIndex(
                name: "IX_time_block_time_group_id",
                table: "time_block",
                column: "time_group_id");

            migrationBuilder.CreateIndex(
                name: "IX_time_block_TimeGroupId1",
                table: "time_block",
                column: "TimeGroupId1");

            migrationBuilder.CreateIndex(
                name: "IX_time_group_application_id",
                table: "time_group",
                column: "application_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "time_block");

            migrationBuilder.DropTable(
                name: "time_group");
        }
    }
}
