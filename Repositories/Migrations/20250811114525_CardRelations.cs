using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class CardRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_card_floorplan_masked_area_registered_masked_area",
                table: "card");

            migrationBuilder.RenameColumn(
                name: "registered_masked_area",
                table: "card",
                newName: "registered_masked_area_id");

            migrationBuilder.RenameIndex(
                name: "IX_card_registered_masked_area",
                table: "card",
                newName: "IX_card_registered_masked_area_id");

            migrationBuilder.AddForeignKey(
                name: "FK_card_floorplan_masked_area_registered_masked_area_id",
                table: "card",
                column: "registered_masked_area_id",
                principalTable: "floorplan_masked_area",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_card_floorplan_masked_area_registered_masked_area_id",
                table: "card");

            migrationBuilder.RenameColumn(
                name: "registered_masked_area_id",
                table: "card",
                newName: "registered_masked_area");

            migrationBuilder.RenameIndex(
                name: "IX_card_registered_masked_area_id",
                table: "card",
                newName: "IX_card_registered_masked_area");

            migrationBuilder.AddForeignKey(
                name: "FK_card_floorplan_masked_area_registered_masked_area",
                table: "card",
                column: "registered_masked_area",
                principalTable: "floorplan_masked_area",
                principalColumn: "id");
        }
    }
}
