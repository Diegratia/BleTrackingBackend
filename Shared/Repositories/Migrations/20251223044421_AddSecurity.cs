using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddSecurity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "MstSecurityId",
                table: "card_record",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "security_id",
                table: "card",
                type: "uniqueidentifier",
                maxLength: 36,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "MstSecurityId",
                table: "alarm_triggers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "security_id",
                table: "alarm_triggers",
                type: "uniqueidentifier",
                maxLength: 36,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "MstSecurityId",
                table: "alarm_record_tracking",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "mst_security",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    person_id = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    organization_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: true),
                    department_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: true),
                    district_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: true),
                    identity_id = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    card_number = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ble_card_number = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    phone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    email = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    gender = table.Column<string>(type: "nvarchar(255)", nullable: true),
                    address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    face_image = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    upload_fr = table.Column<int>(type: "int", nullable: false),
                    upload_fr_error = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    birth_date = table.Column<DateOnly>(type: "date", nullable: true),
                    join_date = table.Column<DateOnly>(type: "date", nullable: true),
                    exit_date = table.Column<DateOnly>(type: "date", nullable: true),
                    application_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    status_employee = table.Column<string>(type: "nvarchar(255)", nullable: true),
                    status = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    created_by = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_by = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mst_security", x => x.id);
                    table.ForeignKey(
                        name: "FK_mst_security_mst_application_application_id",
                        column: x => x.application_id,
                        principalTable: "mst_application",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_mst_security_mst_department_department_id",
                        column: x => x.department_id,
                        principalTable: "mst_department",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_mst_security_mst_district_district_id",
                        column: x => x.district_id,
                        principalTable: "mst_district",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_mst_security_mst_organization_organization_id",
                        column: x => x.organization_id,
                        principalTable: "mst_organization",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_card_record_MstSecurityId",
                table: "card_record",
                column: "MstSecurityId");

            migrationBuilder.CreateIndex(
                name: "IX_card_security_id",
                table: "card",
                column: "security_id");

            migrationBuilder.CreateIndex(
                name: "IX_alarm_triggers_MstSecurityId",
                table: "alarm_triggers",
                column: "MstSecurityId");

            migrationBuilder.CreateIndex(
                name: "IX_alarm_triggers_security_id",
                table: "alarm_triggers",
                column: "security_id");

            migrationBuilder.CreateIndex(
                name: "IX_alarm_record_tracking_MstSecurityId",
                table: "alarm_record_tracking",
                column: "MstSecurityId");

            migrationBuilder.CreateIndex(
                name: "IX_mst_security_application_id",
                table: "mst_security",
                column: "application_id");

            migrationBuilder.CreateIndex(
                name: "IX_mst_security_department_id",
                table: "mst_security",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "IX_mst_security_district_id",
                table: "mst_security",
                column: "district_id");

            migrationBuilder.CreateIndex(
                name: "IX_mst_security_email",
                table: "mst_security",
                column: "email");

            migrationBuilder.CreateIndex(
                name: "IX_mst_security_organization_id",
                table: "mst_security",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "IX_mst_security_person_id",
                table: "mst_security",
                column: "person_id");

            migrationBuilder.AddForeignKey(
                name: "FK_alarm_record_tracking_mst_security_MstSecurityId",
                table: "alarm_record_tracking",
                column: "MstSecurityId",
                principalTable: "mst_security",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_alarm_triggers_mst_security_MstSecurityId",
                table: "alarm_triggers",
                column: "MstSecurityId",
                principalTable: "mst_security",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_alarm_triggers_mst_security_security_id",
                table: "alarm_triggers",
                column: "security_id",
                principalTable: "mst_security",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_card_mst_security_security_id",
                table: "card",
                column: "security_id",
                principalTable: "mst_security",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_card_record_mst_security_MstSecurityId",
                table: "card_record",
                column: "MstSecurityId",
                principalTable: "mst_security",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_alarm_record_tracking_mst_security_MstSecurityId",
                table: "alarm_record_tracking");

            migrationBuilder.DropForeignKey(
                name: "FK_alarm_triggers_mst_security_MstSecurityId",
                table: "alarm_triggers");

            migrationBuilder.DropForeignKey(
                name: "FK_alarm_triggers_mst_security_security_id",
                table: "alarm_triggers");

            migrationBuilder.DropForeignKey(
                name: "FK_card_mst_security_security_id",
                table: "card");

            migrationBuilder.DropForeignKey(
                name: "FK_card_record_mst_security_MstSecurityId",
                table: "card_record");

            migrationBuilder.DropTable(
                name: "mst_security");

            migrationBuilder.DropIndex(
                name: "IX_card_record_MstSecurityId",
                table: "card_record");

            migrationBuilder.DropIndex(
                name: "IX_card_security_id",
                table: "card");

            migrationBuilder.DropIndex(
                name: "IX_alarm_triggers_MstSecurityId",
                table: "alarm_triggers");

            migrationBuilder.DropIndex(
                name: "IX_alarm_triggers_security_id",
                table: "alarm_triggers");

            migrationBuilder.DropIndex(
                name: "IX_alarm_record_tracking_MstSecurityId",
                table: "alarm_record_tracking");

            migrationBuilder.DropColumn(
                name: "MstSecurityId",
                table: "card_record");

            migrationBuilder.DropColumn(
                name: "security_id",
                table: "card");

            migrationBuilder.DropColumn(
                name: "MstSecurityId",
                table: "alarm_triggers");

            migrationBuilder.DropColumn(
                name: "security_id",
                table: "alarm_triggers");

            migrationBuilder.DropColumn(
                name: "MstSecurityId",
                table: "alarm_record_tracking");
        }
    }
}
