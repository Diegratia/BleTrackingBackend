using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class BlacklistVisitorMember : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "is_block",
                table: "mst_member",
                newName: "is_blacklist");

            migrationBuilder.RenameColumn(
                name: "block_at",
                table: "mst_member",
                newName: "blacklist_at");

            migrationBuilder.AddColumn<string>(
                name: "blacklist_reason",
                table: "visitor",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_blacklist",
                table: "visitor",
                type: "bit",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "blacklist_reason",
                table: "visitor");

            migrationBuilder.DropColumn(
                name: "is_blacklist",
                table: "visitor");

            migrationBuilder.RenameColumn(
                name: "is_blacklist",
                table: "mst_member",
                newName: "is_block");

            migrationBuilder.RenameColumn(
                name: "blacklist_at",
                table: "mst_member",
                newName: "block_at");
        }
    }
}
