using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class CardAccessTimeGroups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "card_access_time_groups",
                columns: table => new
                {
                    card_access_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    time_group_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    application_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_card_access_time_groups", x => new { x.card_access_id, x.time_group_id });
                    table.ForeignKey(
                        name: "FK_card_access_time_groups_card_accesses_card_access_id",
                        column: x => x.card_access_id,
                        principalTable: "card_accesses",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_card_access_time_groups_time_group_time_group_id",
                        column: x => x.time_group_id,
                        principalTable: "time_group",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_card_access_time_groups_time_group_id",
                table: "card_access_time_groups",
                column: "time_group_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "card_access_time_groups");
        }
    }
}
