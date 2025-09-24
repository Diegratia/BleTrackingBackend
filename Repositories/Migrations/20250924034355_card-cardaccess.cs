using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class cardcardaccess : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "card_group_card_accesses");

            migrationBuilder.CreateTable(
                name: "card_card_accesses",
                columns: table => new
                {
                    card_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    card_access_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
                    application_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_card_card_accesses", x => new { x.card_id, x.card_access_id });
                    table.ForeignKey(
                        name: "FK_card_card_accesses_card_accesses_card_access_id",
                        column: x => x.card_access_id,
                        principalTable: "card_accesses",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_card_card_accesses_card_card_id",
                        column: x => x.card_id,
                        principalTable: "card",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_card_card_accesses_card_access_id",
                table: "card_card_accesses",
                column: "card_access_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "card_card_accesses");

            migrationBuilder.CreateTable(
                name: "card_group_card_accesses",
                columns: table => new
                {
                    card_group_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    card_access_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    application_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false)
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
                name: "IX_card_group_card_accesses_card_access_id",
                table: "card_group_card_accesses",
                column: "card_access_id");
        }
    }
}
