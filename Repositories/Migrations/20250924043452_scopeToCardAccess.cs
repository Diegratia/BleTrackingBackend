using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class scopeToCardAccess : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "access_scope",
                table: "card_groups");

            migrationBuilder.AddColumn<string>(
                name: "access_scope",
                table: "card_accesses",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "access_scope",
                table: "card_accesses");

            migrationBuilder.AddColumn<string>(
                name: "access_scope",
                table: "card_groups",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
