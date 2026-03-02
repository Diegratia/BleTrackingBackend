using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class swapCardNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "to_card_id",
                table: "card_swap_transaction",
                type: "uniqueidentifier",
                maxLength: 36,
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldMaxLength: 36);

            migrationBuilder.AlterColumn<Guid>(
                name: "from_card_id",
                table: "card_swap_transaction",
                type: "uniqueidentifier",
                maxLength: 36,
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldMaxLength: 36);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "to_card_id",
                table: "card_swap_transaction",
                type: "uniqueidentifier",
                maxLength: 36,
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldMaxLength: 36,
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "from_card_id",
                table: "card_swap_transaction",
                type: "uniqueidentifier",
                maxLength: 36,
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldMaxLength: 36,
                oldNullable: true);
        }
    }
}
