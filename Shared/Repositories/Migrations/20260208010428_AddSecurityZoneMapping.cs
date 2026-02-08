using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddSecurityZoneMapping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "security_zone_mapping",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    area_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    area_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    security_zone = table.Column<int>(type: "int", nullable: false),
                    requires_escort = table.Column<bool>(type: "bit", nullable: false),
                    allowed_from_zones = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_by = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_by = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    application_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_security_zone_mapping", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "security_zone_mapping");
        }
    }
}
