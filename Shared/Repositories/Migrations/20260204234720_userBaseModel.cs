using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class userBaseModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "_generate",
                table: "user");

            migrationBuilder.RenameColumn(
                name: "status_active",
                table: "user",
                newName: "status");

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "user",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "created_by",
                table: "user",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "user",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "updated_by",
                table: "user",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "created_at",
                table: "user");

            migrationBuilder.DropColumn(
                name: "created_by",
                table: "user");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "user");

            migrationBuilder.DropColumn(
                name: "updated_by",
                table: "user");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "user",
                newName: "status_active");

            migrationBuilder.AddColumn<long>(
                name: "_generate",
                table: "user",
                type: "bigint",
                nullable: false,
                defaultValue: 0L)
                .Annotation("SqlServer:Identity", "1, 1");
        }
    }
}
