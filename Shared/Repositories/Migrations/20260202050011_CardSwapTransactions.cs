using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class CardSwapTransactions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "card_swap_transaction",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    from_card_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    to_card_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    trx_visitor_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: true),
                    visitor_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    swap_type = table.Column<string>(type: "nvarchar(255)", nullable: true),
                    card_swap_status = table.Column<string>(type: "nvarchar(255)", nullable: true),
                    masked_area_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    swap_by = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    application_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    swap_chain_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    swap_sequence = table.Column<int>(type: "int", nullable: false),
                    identity_type = table.Column<int>(type: "int", nullable: true),
                    identity_value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    completed_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    executed_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_card_swap_transaction", x => x.id);
                    table.ForeignKey(
                        name: "FK_card_swap_transaction_card_from_card_id",
                        column: x => x.from_card_id,
                        principalTable: "card",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_card_swap_transaction_card_to_card_id",
                        column: x => x.to_card_id,
                        principalTable: "card",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_card_swap_transaction_floorplan_masked_area_masked_area_id",
                        column: x => x.masked_area_id,
                        principalTable: "floorplan_masked_area",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_card_swap_transaction_mst_application_application_id",
                        column: x => x.application_id,
                        principalTable: "mst_application",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_card_swap_transaction_trx_visitor_trx_visitor_id",
                        column: x => x.trx_visitor_id,
                        principalTable: "trx_visitor",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_card_swap_transaction_visitor_visitor_id",
                        column: x => x.visitor_id,
                        principalTable: "visitor",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_card_swap_transaction_application_id",
                table: "card_swap_transaction",
                column: "application_id");

            migrationBuilder.CreateIndex(
                name: "IX_card_swap_transaction_executed_at",
                table: "card_swap_transaction",
                column: "executed_at");

            migrationBuilder.CreateIndex(
                name: "IX_card_swap_transaction_from_card_id",
                table: "card_swap_transaction",
                column: "from_card_id");

            migrationBuilder.CreateIndex(
                name: "IX_card_swap_transaction_masked_area_id",
                table: "card_swap_transaction",
                column: "masked_area_id");

            migrationBuilder.CreateIndex(
                name: "IX_card_swap_transaction_swap_chain_id",
                table: "card_swap_transaction",
                column: "swap_chain_id");

            migrationBuilder.CreateIndex(
                name: "IX_card_swap_transaction_swap_sequence",
                table: "card_swap_transaction",
                column: "swap_sequence");

            migrationBuilder.CreateIndex(
                name: "IX_card_swap_transaction_to_card_id",
                table: "card_swap_transaction",
                column: "to_card_id");

            migrationBuilder.CreateIndex(
                name: "IX_card_swap_transaction_trx_visitor_id",
                table: "card_swap_transaction",
                column: "trx_visitor_id");

            migrationBuilder.CreateIndex(
                name: "IX_card_swap_transaction_visitor_id",
                table: "card_swap_transaction",
                column: "visitor_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "card_swap_transaction");
        }
    }
}
