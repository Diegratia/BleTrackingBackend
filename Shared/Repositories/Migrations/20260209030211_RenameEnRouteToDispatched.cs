using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class RenameEnRouteToDispatched : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "en_route_at",
                table: "alarm_triggers",
                newName: "dispatched_at");

            migrationBuilder.RenameColumn(
                name: "en_route_by",
                table: "alarm_triggers",
                newName: "dispatched_by");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "dispatched_at",
                table: "alarm_triggers",
                newName: "en_route_at");

            migrationBuilder.RenameColumn(
                name: "dispatched_by",
                table: "alarm_triggers",
                newName: "en_route_by");
        }
    }
}
