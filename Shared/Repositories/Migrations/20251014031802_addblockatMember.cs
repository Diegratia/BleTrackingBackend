using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class addblockatMember : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "block_at",
                table: "card");

            migrationBuilder.DropColumn(
                name: "is_block",
                table: "card");

            migrationBuilder.AddColumn<DateTime>(
                name: "block_at",
                table: "mst_member",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_block",
                table: "mst_member",
                type: "bit",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "block_at",
                table: "mst_member");

            migrationBuilder.DropColumn(
                name: "is_block",
                table: "mst_member");

            migrationBuilder.AddColumn<DateTime>(
                name: "block_at",
                table: "card",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_block",
                table: "card",
                type: "bit",
                nullable: true);
        }
    }
}
