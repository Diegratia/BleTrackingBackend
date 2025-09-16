using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class changeCardAccess : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_card_card_groups_CardGroupId1",
                table: "card");

            migrationBuilder.DropIndex(
                name: "IX_card_CardGroupId1",
                table: "card");

            migrationBuilder.DropColumn(
                name: "CardGroupId1",
                table: "card");

            migrationBuilder.AlterColumn<int>(
                name: "access_number",
                table: "card_accesses",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "access_number",
                table: "card_accesses",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<Guid>(
                name: "CardGroupId1",
                table: "card",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_card_CardGroupId1",
                table: "card",
                column: "CardGroupId1");

            migrationBuilder.AddForeignKey(
                name: "FK_card_card_groups_CardGroupId1",
                table: "card",
                column: "CardGroupId1",
                principalTable: "card_groups",
                principalColumn: "id");
        }
    }
}
