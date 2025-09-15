using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class CardAccessGroup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "_generate",
                table: "visitor_blacklist_area");

            migrationBuilder.DropColumn(
                name: "_generate",
                table: "visitor");

            migrationBuilder.DropColumn(
                name: "_generate",
                table: "trx_visitor");

            migrationBuilder.DropColumn(
                name: "_generate",
                table: "time_group");

            migrationBuilder.DropColumn(
                name: "_generate",
                table: "time_block");

            migrationBuilder.DropColumn(
                name: "_generate",
                table: "mst_member");

            migrationBuilder.DropColumn(
                name: "_generate",
                table: "mst_ble_reader");

            migrationBuilder.DropColumn(
                name: "_generate",
                table: "mst_access_control");

            migrationBuilder.DropColumn(
                name: "_generate",
                table: "mst_access_cctv");

            migrationBuilder.DropColumn(
                name: "_generate",
                table: "floorplan_masked_area");

            migrationBuilder.DropColumn(
                name: "_generate",
                table: "card_record");

            migrationBuilder.DropColumn(
                name: "_generate",
                table: "card");

            migrationBuilder.AddColumn<Guid>(
                name: "CardGroupId1",
                table: "card",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "card_group_id",
                table: "card",
                type: "uniqueidentifier",
                maxLength: 36,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "card_accesses",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    access_number = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    application_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
                    created_by = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_by = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_card_accesses", x => x.id);
                    table.ForeignKey(
                        name: "FK_card_accesses_mst_application_application_id",
                        column: x => x.application_id,
                        principalTable: "mst_application",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "card_groups",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    application_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
                    created_by = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_by = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_card_groups", x => x.id);
                    table.ForeignKey(
                        name: "FK_card_groups_mst_application_application_id",
                        column: x => x.application_id,
                        principalTable: "mst_application",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "card_access_masked_areas",
                columns: table => new
                {
                    card_access_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    masked_area_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    application_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_card_access_masked_areas", x => new { x.card_access_id, x.masked_area_id });
                    table.ForeignKey(
                        name: "FK_card_access_masked_areas_card_accesses_card_access_id",
                        column: x => x.card_access_id,
                        principalTable: "card_accesses",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_card_access_masked_areas_floorplan_masked_area_masked_area_id",
                        column: x => x.masked_area_id,
                        principalTable: "floorplan_masked_area",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "card_group_card_accesses",
                columns: table => new
                {
                    card_group_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    card_access_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
                    application_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_card_group_card_accesses", x => new { x.card_group_id, x.card_access_id });
                    table.ForeignKey(
                        name: "FK_card_group_card_accesses_card_accesses_card_access_id",
                        column: x => x.card_access_id,
                        principalTable: "card_accesses",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_card_group_card_accesses_card_groups_card_group_id",
                        column: x => x.card_group_id,
                        principalTable: "card_groups",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_card_card_group_id",
                table: "card",
                column: "card_group_id");

            migrationBuilder.CreateIndex(
                name: "IX_card_CardGroupId1",
                table: "card",
                column: "CardGroupId1");

            migrationBuilder.CreateIndex(
                name: "IX_card_access_masked_areas_masked_area_id",
                table: "card_access_masked_areas",
                column: "masked_area_id");

            migrationBuilder.CreateIndex(
                name: "IX_card_accesses_application_id",
                table: "card_accesses",
                column: "application_id");

            migrationBuilder.CreateIndex(
                name: "IX_card_group_card_accesses_card_access_id",
                table: "card_group_card_accesses",
                column: "card_access_id");

            migrationBuilder.CreateIndex(
                name: "IX_card_groups_application_id",
                table: "card_groups",
                column: "application_id");

            migrationBuilder.AddForeignKey(
                name: "FK_card_card_groups_CardGroupId1",
                table: "card",
                column: "CardGroupId1",
                principalTable: "card_groups",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_card_card_groups_card_group_id",
                table: "card",
                column: "card_group_id",
                principalTable: "card_groups",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_card_card_groups_CardGroupId1",
                table: "card");

            migrationBuilder.DropForeignKey(
                name: "FK_card_card_groups_card_group_id",
                table: "card");

            migrationBuilder.DropTable(
                name: "card_access_masked_areas");

            migrationBuilder.DropTable(
                name: "card_group_card_accesses");

            migrationBuilder.DropTable(
                name: "card_accesses");

            migrationBuilder.DropTable(
                name: "card_groups");

            migrationBuilder.DropIndex(
                name: "IX_card_card_group_id",
                table: "card");

            migrationBuilder.DropIndex(
                name: "IX_card_CardGroupId1",
                table: "card");

            migrationBuilder.DropColumn(
                name: "CardGroupId1",
                table: "card");

            migrationBuilder.DropColumn(
                name: "card_group_id",
                table: "card");

            migrationBuilder.AddColumn<long>(
                name: "_generate",
                table: "visitor_blacklist_area",
                type: "bigint",
                nullable: false,
                defaultValue: 0L)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<long>(
                name: "_generate",
                table: "visitor",
                type: "bigint",
                nullable: false,
                defaultValue: 0L)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<long>(
                name: "_generate",
                table: "trx_visitor",
                type: "bigint",
                nullable: false,
                defaultValue: 0L)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<long>(
                name: "_generate",
                table: "time_group",
                type: "bigint",
                nullable: false,
                defaultValue: 0L)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<long>(
                name: "_generate",
                table: "time_block",
                type: "bigint",
                nullable: false,
                defaultValue: 0L)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<long>(
                name: "_generate",
                table: "mst_member",
                type: "bigint",
                nullable: false,
                defaultValue: 0L)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<long>(
                name: "_generate",
                table: "mst_ble_reader",
                type: "bigint",
                nullable: false,
                defaultValue: 0L)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<long>(
                name: "_generate",
                table: "mst_access_control",
                type: "bigint",
                nullable: false,
                defaultValue: 0L)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<long>(
                name: "_generate",
                table: "mst_access_cctv",
                type: "bigint",
                nullable: false,
                defaultValue: 0L)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<long>(
                name: "_generate",
                table: "floorplan_masked_area",
                type: "bigint",
                nullable: false,
                defaultValue: 0L)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<long>(
                name: "_generate",
                table: "card_record",
                type: "bigint",
                nullable: false,
                defaultValue: 0L)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<long>(
                name: "_generate",
                table: "card",
                type: "bigint",
                nullable: false,
                defaultValue: 0L)
                .Annotation("SqlServer:Identity", "1, 1");
        }
    }
}
