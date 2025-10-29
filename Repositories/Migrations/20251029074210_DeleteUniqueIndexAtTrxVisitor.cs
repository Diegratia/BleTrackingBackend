using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class DeleteUniqueIndexAtTrxVisitor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_trx_visitor_visitor_id_visitor_period_start_visitor_period_end",
                table: "trx_visitor");

            migrationBuilder.CreateIndex(
                name: "IX_trx_visitor_visitor_id_visitor_period_start_visitor_period_end",
                table: "trx_visitor",
                columns: new[] { "visitor_id", "visitor_period_start", "visitor_period_end" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_trx_visitor_visitor_id_visitor_period_start_visitor_period_end",
                table: "trx_visitor");

            migrationBuilder.CreateIndex(
                name: "IX_trx_visitor_visitor_id_visitor_period_start_visitor_period_end",
                table: "trx_visitor",
                columns: new[] { "visitor_id", "visitor_period_start", "visitor_period_end" },
                unique: true,
                filter: "[visitor_id] IS NOT NULL AND [visitor_period_start] IS NOT NULL AND [visitor_period_end] IS NOT NULL");
        }
    }
}
