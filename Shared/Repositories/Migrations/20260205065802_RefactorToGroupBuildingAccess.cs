using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class RefactorToGroupBuildingAccess : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "group_building_access",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    group_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    building_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    application_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    created_by = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_by = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_group_building_access", x => x.id);
                    table.ForeignKey(
                        name: "FK_group_building_access_mst_building_building_id",
                        column: x => x.building_id,
                        principalTable: "mst_building",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_group_building_access_user_group_group_id",
                        column: x => x.group_id,
                        principalTable: "user_group",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_group_building_access_building_id",
                table: "group_building_access",
                column: "building_id");

            migrationBuilder.CreateIndex(
                name: "IX_group_building_access_group_id",
                table: "group_building_access",
                column: "group_id");

            // DATA MIGRATION: Migrate existing user building accesses to group building accesses
            migrationBuilder.Sql(
                @"INSERT INTO group_building_access (id, group_id, building_id, application_id, created_by, created_at, updated_by, updated_at, status)
                SELECT
                    NEWID(),
                    u.group_id,
                    uba.building_id,
                    uba.application_id,
                    uba.created_by,
                    uba.created_at,
                    uba.updated_by,
                    uba.updated_at,
                    uba.status
                FROM user_building_access uba
                INNER JOIN [user] u ON uba.user_id = u.id
                WHERE uba.status != 0
                GROUP BY u.group_id, uba.building_id, uba.application_id, uba.created_by, uba.created_at, uba.updated_by, uba.updated_at, uba.status");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "group_building_access");
        }
    }
}
