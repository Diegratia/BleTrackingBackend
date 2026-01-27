using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class cardIsBlockAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "visitor_blacklist_area");

            migrationBuilder.AddColumn<DateTime>(
                name: "block_at",
                table: "card",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_block",
                table: "card",
                type: "bit",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "blacklist_area",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    floorplan_masked_area_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    visitor_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: true),
                    member_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: true),
                    application_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    status = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    FloorplanMaskedAreaId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    VisitorId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    created_by = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_by = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_blacklist_area", x => x.id);
                    table.ForeignKey(
                        name: "FK_blacklist_area_floorplan_masked_area_FloorplanMaskedAreaId1",
                        column: x => x.FloorplanMaskedAreaId1,
                        principalTable: "floorplan_masked_area",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_blacklist_area_floorplan_masked_area_floorplan_masked_area_id",
                        column: x => x.floorplan_masked_area_id,
                        principalTable: "floorplan_masked_area",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_blacklist_area_mst_application_application_id",
                        column: x => x.application_id,
                        principalTable: "mst_application",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_blacklist_area_mst_member_member_id",
                        column: x => x.member_id,
                        principalTable: "mst_member",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_blacklist_area_visitor_VisitorId1",
                        column: x => x.VisitorId1,
                        principalTable: "visitor",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_blacklist_area_visitor_visitor_id",
                        column: x => x.visitor_id,
                        principalTable: "visitor",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_blacklist_area_application_id",
                table: "blacklist_area",
                column: "application_id");

            migrationBuilder.CreateIndex(
                name: "IX_blacklist_area_floorplan_masked_area_id",
                table: "blacklist_area",
                column: "floorplan_masked_area_id");

            migrationBuilder.CreateIndex(
                name: "IX_blacklist_area_FloorplanMaskedAreaId1",
                table: "blacklist_area",
                column: "FloorplanMaskedAreaId1");

            migrationBuilder.CreateIndex(
                name: "IX_blacklist_area_member_id",
                table: "blacklist_area",
                column: "member_id");

            migrationBuilder.CreateIndex(
                name: "IX_blacklist_area_visitor_id",
                table: "blacklist_area",
                column: "visitor_id");

            migrationBuilder.CreateIndex(
                name: "IX_blacklist_area_VisitorId1",
                table: "blacklist_area",
                column: "VisitorId1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "blacklist_area");

            migrationBuilder.DropColumn(
                name: "block_at",
                table: "card");

            migrationBuilder.DropColumn(
                name: "is_block",
                table: "card");

            migrationBuilder.CreateTable(
                name: "visitor_blacklist_area",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    application_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    floorplan_masked_area_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    visitor_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    FloorplanMaskedAreaId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    status = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_by = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VisitorId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_visitor_blacklist_area", x => x.id);
                    table.ForeignKey(
                        name: "FK_visitor_blacklist_area_floorplan_masked_area_FloorplanMaskedAreaId1",
                        column: x => x.FloorplanMaskedAreaId1,
                        principalTable: "floorplan_masked_area",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_visitor_blacklist_area_floorplan_masked_area_floorplan_masked_area_id",
                        column: x => x.floorplan_masked_area_id,
                        principalTable: "floorplan_masked_area",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_visitor_blacklist_area_mst_application_application_id",
                        column: x => x.application_id,
                        principalTable: "mst_application",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_visitor_blacklist_area_visitor_VisitorId1",
                        column: x => x.VisitorId1,
                        principalTable: "visitor",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_visitor_blacklist_area_visitor_visitor_id",
                        column: x => x.visitor_id,
                        principalTable: "visitor",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_visitor_blacklist_area_application_id",
                table: "visitor_blacklist_area",
                column: "application_id");

            migrationBuilder.CreateIndex(
                name: "IX_visitor_blacklist_area_floorplan_masked_area_id",
                table: "visitor_blacklist_area",
                column: "floorplan_masked_area_id");

            migrationBuilder.CreateIndex(
                name: "IX_visitor_blacklist_area_FloorplanMaskedAreaId1",
                table: "visitor_blacklist_area",
                column: "FloorplanMaskedAreaId1");

            migrationBuilder.CreateIndex(
                name: "IX_visitor_blacklist_area_visitor_id",
                table: "visitor_blacklist_area",
                column: "visitor_id");

            migrationBuilder.CreateIndex(
                name: "IX_visitor_blacklist_area_VisitorId1",
                table: "visitor_blacklist_area",
                column: "VisitorId1");
        }
    }
}
