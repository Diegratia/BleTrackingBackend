using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class addVisitornMembertoTrans : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "member_id",
                table: "tracking_transaction",
                type: "uniqueidentifier",
                maxLength: 36,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "visitor_id",
                table: "tracking_transaction",
                type: "uniqueidentifier",
                maxLength: 36,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "mst_engine",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "created_by",
                table: "mst_engine",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "mst_engine",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "updated_by",
                table: "mst_engine",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_tracking_transaction_member_id",
                table: "tracking_transaction",
                column: "member_id");

            migrationBuilder.CreateIndex(
                name: "IX_tracking_transaction_visitor_id",
                table: "tracking_transaction",
                column: "visitor_id");

            migrationBuilder.CreateIndex(
                name: "IX_mst_engine_engine_id",
                table: "mst_engine",
                column: "engine_id",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_tracking_transaction_mst_member_member_id",
                table: "tracking_transaction",
                column: "member_id",
                principalTable: "mst_member",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_tracking_transaction_visitor_visitor_id",
                table: "tracking_transaction",
                column: "visitor_id",
                principalTable: "visitor",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tracking_transaction_mst_member_member_id",
                table: "tracking_transaction");

            migrationBuilder.DropForeignKey(
                name: "FK_tracking_transaction_visitor_visitor_id",
                table: "tracking_transaction");

            migrationBuilder.DropIndex(
                name: "IX_tracking_transaction_member_id",
                table: "tracking_transaction");

            migrationBuilder.DropIndex(
                name: "IX_tracking_transaction_visitor_id",
                table: "tracking_transaction");

            migrationBuilder.DropIndex(
                name: "IX_mst_engine_engine_id",
                table: "mst_engine");

            migrationBuilder.DropColumn(
                name: "member_id",
                table: "tracking_transaction");

            migrationBuilder.DropColumn(
                name: "visitor_id",
                table: "tracking_transaction");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "mst_engine");

            migrationBuilder.DropColumn(
                name: "created_by",
                table: "mst_engine");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "mst_engine");

            migrationBuilder.DropColumn(
                name: "updated_by",
                table: "mst_engine");
        }
    }
}
