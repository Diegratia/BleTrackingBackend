using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "mst_application",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    application_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    organization_type = table.Column<string>(type: "nvarchar(255)", nullable: false, defaultValue: "Single"),
                    organization_address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    application_type = table.Column<string>(type: "nvarchar(255)", nullable: false, defaultValue: ""),
                    application_registered = table.Column<DateTime>(type: "datetime2", nullable: false),
                    application_expired = table.Column<DateTime>(type: "datetime2", nullable: false),
                    host_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    host_phone = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    host_email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    host_address = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    application_custom_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    application_custom_domain = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    application_custom_port = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    license_code = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    license_type = table.Column<string>(type: "nvarchar(255)", nullable: false),
                    application_status = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    _generate = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mst_application", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "mst_brand",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    tag = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    application_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    status = table.Column<int>(type: "int", nullable: true),
                    _generate = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mst_brand", x => x.id);
                    table.ForeignKey(
                        name: "FK_mst_brand_mst_application_application_id",
                        column: x => x.application_id,
                        principalTable: "mst_application",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "mst_building",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    image = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    application_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    status = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    MstApplicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    _generate = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    created_by = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_by = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mst_building", x => x.id);
                    table.ForeignKey(
                        name: "FK_mst_building_mst_application_MstApplicationId",
                        column: x => x.MstApplicationId,
                        principalTable: "mst_application",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_mst_building_mst_application_application_id",
                        column: x => x.application_id,
                        principalTable: "mst_application",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "mst_department",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    code = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    department_host = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    application_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    status = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    MstApplicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    _generate = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    created_by = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_by = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mst_department", x => x.id);
                    table.ForeignKey(
                        name: "FK_mst_department_mst_application_MstApplicationId",
                        column: x => x.MstApplicationId,
                        principalTable: "mst_application",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_mst_department_mst_application_application_id",
                        column: x => x.application_id,
                        principalTable: "mst_application",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "mst_district",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    code = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    district_host = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    application_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    status = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    MstApplicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    _generate = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    created_by = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_by = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mst_district", x => x.id);
                    table.ForeignKey(
                        name: "FK_mst_district_mst_application_MstApplicationId",
                        column: x => x.MstApplicationId,
                        principalTable: "mst_application",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_mst_district_mst_application_application_id",
                        column: x => x.application_id,
                        principalTable: "mst_application",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "mst_engine",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    engine_id = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    port = table.Column<int>(type: "int", nullable: true),
                    status = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    is_live = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    last_live = table.Column<DateTime>(type: "datetime2", nullable: false),
                    service_status = table.Column<string>(type: "nvarchar(255)", maxLength: 50, nullable: false),
                    application_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mst_engine", x => x.id);
                    table.ForeignKey(
                        name: "FK_mst_engine_mst_application_application_id",
                        column: x => x.application_id,
                        principalTable: "mst_application",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "mst_organization",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    code = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    organization_host = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    application_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    status = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    MstApplicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    _generate = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    created_by = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_by = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mst_organization", x => x.id);
                    table.ForeignKey(
                        name: "FK_mst_organization_mst_application_MstApplicationId",
                        column: x => x.MstApplicationId,
                        principalTable: "mst_application",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_mst_organization_mst_application_application_id",
                        column: x => x.application_id,
                        principalTable: "mst_application",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "user_group",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    level_priority = table.Column<int>(type: "int", nullable: true),
                    application_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    created_by = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_by = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    status = table.Column<int>(type: "int", nullable: true),
                    _generate = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_group", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_group_mst_application_application_id",
                        column: x => x.application_id,
                        principalTable: "mst_application",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "visitor",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    person_id = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    identity_id = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    identity_type = table.Column<string>(type: "nvarchar(255)", nullable: true),
                    card_number = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ble_card_number = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    phone = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    gender = table.Column<string>(type: "nvarchar(255)", nullable: true),
                    address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    organization_name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    district_name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    department_name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    visitor_group_code = table.Column<long>(type: "bigint", nullable: true),
                    visitor_number = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    visitor_code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    is_vip = table.Column<bool>(type: "bit", nullable: true),
                    status = table.Column<int>(type: "int", nullable: false),
                    face_image = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    upload_fr = table.Column<int>(type: "int", nullable: false),
                    upload_fr_error = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    application_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    MstApplicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    _generate = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    created_by = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_by = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_visitor", x => x.id);
                    table.ForeignKey(
                        name: "FK_visitor_mst_application_MstApplicationId",
                        column: x => x.MstApplicationId,
                        principalTable: "mst_application",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_visitor_mst_application_application_id",
                        column: x => x.application_id,
                        principalTable: "mst_application",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "mst_ble_reader",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    brand_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ip = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    gmac = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    engine_reader_id = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    status = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    application_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    _generate = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    created_by = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_by = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mst_ble_reader", x => x.id);
                    table.ForeignKey(
                        name: "FK_mst_ble_reader_mst_application_application_id",
                        column: x => x.application_id,
                        principalTable: "mst_application",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_mst_ble_reader_mst_brand_brand_id",
                        column: x => x.brand_id,
                        principalTable: "mst_brand",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "mst_integration",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    brand_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    integration_type = table.Column<string>(type: "nvarchar(255)", nullable: false),
                    api_type_auth = table.Column<string>(type: "nvarchar(255)", nullable: false),
                    api_url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    api_auth_username = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    api_auth_passwd = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    api_key_field = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    api_key_value = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    application_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    status = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    _generate = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    created_by = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_by = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mst_integration", x => x.id);
                    table.ForeignKey(
                        name: "FK_mst_integration_mst_application_application_id",
                        column: x => x.application_id,
                        principalTable: "mst_application",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_mst_integration_mst_brand_brand_id",
                        column: x => x.brand_id,
                        principalTable: "mst_brand",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "mst_floor",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    building_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    floor_image = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    pixel_x = table.Column<float>(type: "real", nullable: false),
                    pixel_y = table.Column<float>(type: "real", nullable: false),
                    floor_x = table.Column<float>(type: "real", nullable: false),
                    floor_y = table.Column<float>(type: "real", nullable: false),
                    meter_per_px = table.Column<float>(type: "real", nullable: false),
                    engine_floor_id = table.Column<long>(type: "bigint", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    application_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    _generate = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    created_by = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_by = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mst_floor", x => x.id);
                    table.ForeignKey(
                        name: "FK_mst_floor_mst_application_application_id",
                        column: x => x.application_id,
                        principalTable: "mst_application",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_mst_floor_mst_building_building_id",
                        column: x => x.building_id,
                        principalTable: "mst_building",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "mst_member",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    person_id = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    organization_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    department_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    district_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    identity_id = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    card_number = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ble_card_number = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    phone = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    gender = table.Column<string>(type: "nvarchar(255)", nullable: true),
                    address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    face_image = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    upload_fr = table.Column<int>(type: "int", nullable: false),
                    upload_fr_error = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    birth_date = table.Column<DateOnly>(type: "date", nullable: true),
                    join_date = table.Column<DateOnly>(type: "date", nullable: true),
                    exit_date = table.Column<DateOnly>(type: "date", nullable: true),
                    head_member1 = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    head_member2 = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    application_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    status_employee = table.Column<string>(type: "nvarchar(255)", nullable: true),
                    status = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    MstApplicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MstDepartmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MstDistrictId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MstOrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    _generate = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    created_by = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_by = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mst_member", x => x.id);
                    table.ForeignKey(
                        name: "FK_mst_member_mst_application_MstApplicationId",
                        column: x => x.MstApplicationId,
                        principalTable: "mst_application",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_mst_member_mst_application_application_id",
                        column: x => x.application_id,
                        principalTable: "mst_application",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_mst_member_mst_department_MstDepartmentId",
                        column: x => x.MstDepartmentId,
                        principalTable: "mst_department",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_mst_member_mst_department_department_id",
                        column: x => x.department_id,
                        principalTable: "mst_department",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_mst_member_mst_district_MstDistrictId",
                        column: x => x.MstDistrictId,
                        principalTable: "mst_district",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_mst_member_mst_district_district_id",
                        column: x => x.district_id,
                        principalTable: "mst_district",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_mst_member_mst_organization_MstOrganizationId",
                        column: x => x.MstOrganizationId,
                        principalTable: "mst_organization",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_mst_member_mst_organization_organization_id",
                        column: x => x.organization_id,
                        principalTable: "mst_organization",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "user",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    username = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    password = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    is_created_password = table.Column<int>(type: "int", nullable: false),
                    email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    is_email_confirmation = table.Column<int>(type: "int", nullable: false),
                    email_confirmation_code = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    email_confirmation_expired_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    email_confirmation_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    last_login_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    status_active = table.Column<int>(type: "int", nullable: false),
                    group_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    application_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    _generate = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_mst_application_application_id",
                        column: x => x.application_id,
                        principalTable: "mst_application",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_user_group_UserGroupId",
                        column: x => x.UserGroupId,
                        principalTable: "user_group",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_user_user_group_group_id",
                        column: x => x.group_id,
                        principalTable: "user_group",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ble_reader_node",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    ble_reader_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    start_pos = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    end_pos = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    distance_px = table.Column<float>(type: "real", nullable: false),
                    distance = table.Column<float>(type: "real", nullable: false),
                    application_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    created_by = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_by = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    _generate = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ble_reader_node", x => x.id);
                    table.ForeignKey(
                        name: "FK_ble_reader_node_mst_application_application_id",
                        column: x => x.application_id,
                        principalTable: "mst_application",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_ble_reader_node_mst_ble_reader_ble_reader_id",
                        column: x => x.ble_reader_id,
                        principalTable: "mst_ble_reader",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "mst_access_cctv",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    rtsp = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    integration_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: true),
                    application_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    status = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    MstApplicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    _generate = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    created_by = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_by = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mst_access_cctv", x => x.id);
                    table.ForeignKey(
                        name: "FK_mst_access_cctv_mst_application_MstApplicationId",
                        column: x => x.MstApplicationId,
                        principalTable: "mst_application",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_mst_access_cctv_mst_application_application_id",
                        column: x => x.application_id,
                        principalTable: "mst_application",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_mst_access_cctv_mst_integration_integration_id",
                        column: x => x.integration_id,
                        principalTable: "mst_integration",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "mst_access_control",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    controller_brand_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    type = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    channel = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    door_id = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    raw = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    integration_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: true),
                    application_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    status = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    MstApplicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    _generate = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    created_by = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_by = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mst_access_control", x => x.id);
                    table.ForeignKey(
                        name: "FK_mst_access_control_mst_application_MstApplicationId",
                        column: x => x.MstApplicationId,
                        principalTable: "mst_application",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_mst_access_control_mst_application_application_id",
                        column: x => x.application_id,
                        principalTable: "mst_application",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_mst_access_control_mst_brand_controller_brand_id",
                        column: x => x.controller_brand_id,
                        principalTable: "mst_brand",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_mst_access_control_mst_integration_integration_id",
                        column: x => x.integration_id,
                        principalTable: "mst_integration",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "mst_floorplan",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    floor_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    application_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    status = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    MstApplicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MstFloorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    _generate = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    created_by = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_by = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mst_floorplan", x => x.id);
                    table.ForeignKey(
                        name: "FK_mst_floorplan_mst_application_MstApplicationId",
                        column: x => x.MstApplicationId,
                        principalTable: "mst_application",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_mst_floorplan_mst_application_application_id",
                        column: x => x.application_id,
                        principalTable: "mst_application",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_mst_floorplan_mst_floor_MstFloorId",
                        column: x => x.MstFloorId,
                        principalTable: "mst_floor",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_mst_floorplan_mst_floor_floor_id",
                        column: x => x.floor_id,
                        principalTable: "mst_floor",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "refresh_token",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    token = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    expiry_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_refresh_token", x => x.id);
                    table.ForeignKey(
                        name: "FK_refresh_token_user_user_id",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "floorplan_masked_area",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    floorplan_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    floor_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    area_shape = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    color_area = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    restricted_status = table.Column<string>(type: "nvarchar(255)", nullable: false),
                    engine_area_id = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    status = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    application_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    MstFloorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    _generate = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    created_by = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_by = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_floorplan_masked_area", x => x.id);
                    table.ForeignKey(
                        name: "FK_floorplan_masked_area_mst_application_application_id",
                        column: x => x.application_id,
                        principalTable: "mst_application",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_floorplan_masked_area_mst_floor_MstFloorId",
                        column: x => x.MstFloorId,
                        principalTable: "mst_floor",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_floorplan_masked_area_mst_floor_floor_id",
                        column: x => x.floor_id,
                        principalTable: "mst_floor",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_floorplan_masked_area_mst_floorplan_floorplan_id",
                        column: x => x.floorplan_id,
                        principalTable: "mst_floorplan",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "alarm_record_tracking",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    timestamp = table.Column<DateTime>(type: "datetime2", nullable: true),
                    visitor_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    ble_reader_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    floorplan_masked_area_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    alarm_record_status = table.Column<string>(type: "nvarchar(255)", nullable: false),
                    action = table.Column<string>(type: "nvarchar(255)", nullable: false),
                    application_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    idle_timestamp = table.Column<DateTime>(type: "datetime2", nullable: true),
                    done_timestamp = table.Column<DateTime>(type: "datetime2", nullable: true),
                    cancel_timestamp = table.Column<DateTime>(type: "datetime2", nullable: true),
                    waiting_timestamp = table.Column<DateTime>(type: "datetime2", nullable: true),
                    investigated_timestamp = table.Column<DateTime>(type: "datetime2", nullable: true),
                    investigated_done_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    idle_by = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    done_by = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    cancel_by = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    waiting_by = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    investigated_by = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    investigated_result = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FloorplanMaskedAreaId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MstApplicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MstBleReaderId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    VisitorId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    _generate = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_alarm_record_tracking", x => x.id);
                    table.ForeignKey(
                        name: "FK_alarm_record_tracking_floorplan_masked_area_FloorplanMaskedAreaId1",
                        column: x => x.FloorplanMaskedAreaId1,
                        principalTable: "floorplan_masked_area",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_alarm_record_tracking_floorplan_masked_area_floorplan_masked_area_id",
                        column: x => x.floorplan_masked_area_id,
                        principalTable: "floorplan_masked_area",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_alarm_record_tracking_mst_application_MstApplicationId",
                        column: x => x.MstApplicationId,
                        principalTable: "mst_application",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_alarm_record_tracking_mst_application_application_id",
                        column: x => x.application_id,
                        principalTable: "mst_application",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_alarm_record_tracking_mst_ble_reader_MstBleReaderId",
                        column: x => x.MstBleReaderId,
                        principalTable: "mst_ble_reader",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_alarm_record_tracking_mst_ble_reader_ble_reader_id",
                        column: x => x.ble_reader_id,
                        principalTable: "mst_ble_reader",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_alarm_record_tracking_visitor_VisitorId1",
                        column: x => x.VisitorId1,
                        principalTable: "visitor",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_alarm_record_tracking_visitor_visitor_id",
                        column: x => x.visitor_id,
                        principalTable: "visitor",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "card",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    type = table.Column<string>(type: "nvarchar(255)", nullable: true),
                    card_number = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    qr_code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    dmac = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    is_multi_masked_area = table.Column<bool>(type: "bit", nullable: true),
                    registered_masked_area = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    is_used = table.Column<bool>(type: "bit", nullable: true),
                    last_used_by = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    visitor_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: true),
                    member_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: true),
                    checkin_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    checkout_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    status_card = table.Column<int>(type: "int", nullable: true),
                    application_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    _generate = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    created_by = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_by = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_card", x => x.id);
                    table.ForeignKey(
                        name: "FK_card_floorplan_masked_area_registered_masked_area",
                        column: x => x.registered_masked_area,
                        principalTable: "floorplan_masked_area",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_card_mst_application_application_id",
                        column: x => x.application_id,
                        principalTable: "mst_application",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_card_mst_member_member_id",
                        column: x => x.member_id,
                        principalTable: "mst_member",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_card_visitor_visitor_id",
                        column: x => x.visitor_id,
                        principalTable: "visitor",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "floorplan_device",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    floorplan_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    access_cctv_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    ble_reader_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    access_control_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    pos_x = table.Column<float>(type: "real", nullable: false),
                    pos_y = table.Column<float>(type: "real", nullable: false),
                    pos_px_x = table.Column<float>(type: "real", nullable: false),
                    pos_px_y = table.Column<float>(type: "real", nullable: false),
                    floorplan_masked_area_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    application_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    device_status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    status = table.Column<int>(type: "int", nullable: false),
                    FloorplanMaskedAreaId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MstAccessCctvId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MstAccessControlId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MstApplicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MstBleReaderId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    _generate = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    created_by = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_by = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_floorplan_device", x => x.id);
                    table.ForeignKey(
                        name: "FK_floorplan_device_floorplan_masked_area_FloorplanMaskedAreaId1",
                        column: x => x.FloorplanMaskedAreaId1,
                        principalTable: "floorplan_masked_area",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_floorplan_device_floorplan_masked_area_floorplan_masked_area_id",
                        column: x => x.floorplan_masked_area_id,
                        principalTable: "floorplan_masked_area",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_floorplan_device_mst_access_cctv_MstAccessCctvId",
                        column: x => x.MstAccessCctvId,
                        principalTable: "mst_access_cctv",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_floorplan_device_mst_access_cctv_access_cctv_id",
                        column: x => x.access_cctv_id,
                        principalTable: "mst_access_cctv",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_floorplan_device_mst_access_control_MstAccessControlId",
                        column: x => x.MstAccessControlId,
                        principalTable: "mst_access_control",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_floorplan_device_mst_access_control_access_control_id",
                        column: x => x.access_control_id,
                        principalTable: "mst_access_control",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_floorplan_device_mst_application_MstApplicationId",
                        column: x => x.MstApplicationId,
                        principalTable: "mst_application",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_floorplan_device_mst_application_application_id",
                        column: x => x.application_id,
                        principalTable: "mst_application",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_floorplan_device_mst_ble_reader_MstBleReaderId",
                        column: x => x.MstBleReaderId,
                        principalTable: "mst_ble_reader",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_floorplan_device_mst_ble_reader_ble_reader_id",
                        column: x => x.ble_reader_id,
                        principalTable: "mst_ble_reader",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_floorplan_device_mst_floorplan_floorplan_id",
                        column: x => x.floorplan_id,
                        principalTable: "mst_floorplan",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "tracking_transaction",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    trans_time = table.Column<DateTime>(type: "datetime2", nullable: false),
                    reader_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    card_id = table.Column<long>(type: "bigint", nullable: false),
                    floorplan_masked_area_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    coordinate_x = table.Column<float>(type: "real", nullable: false),
                    coordinate_y = table.Column<float>(type: "real", nullable: false),
                    coordinate_px_x = table.Column<float>(type: "real", nullable: false),
                    coordinate_px_y = table.Column<float>(type: "real", nullable: false),
                    alarm_status = table.Column<string>(type: "nvarchar(255)", nullable: false),
                    battery = table.Column<long>(type: "bigint", nullable: false),
                    application_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    FloorplanMaskedAreaId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MstBleReaderId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tracking_transaction", x => x.id);
                    table.ForeignKey(
                        name: "FK_tracking_transaction_floorplan_masked_area_FloorplanMaskedAreaId1",
                        column: x => x.FloorplanMaskedAreaId1,
                        principalTable: "floorplan_masked_area",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_tracking_transaction_floorplan_masked_area_floorplan_masked_area_id",
                        column: x => x.floorplan_masked_area_id,
                        principalTable: "floorplan_masked_area",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_tracking_transaction_mst_application_application_id",
                        column: x => x.application_id,
                        principalTable: "mst_application",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_tracking_transaction_mst_ble_reader_MstBleReaderId",
                        column: x => x.MstBleReaderId,
                        principalTable: "mst_ble_reader",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_tracking_transaction_mst_ble_reader_reader_id",
                        column: x => x.reader_id,
                        principalTable: "mst_ble_reader",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "trx_visitor",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    checked_in_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    checked_out_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    deny_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    block_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    unblock_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    checkin_by = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    checkout_by = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    deny_by = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    deny_reason = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    block_by = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    block_reason = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    visitor_status = table.Column<string>(type: "nvarchar(255)", nullable: false),
                    visitor_active_status = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    invitation_created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    visitor_group_code = table.Column<long>(type: "bigint", nullable: true),
                    visitor_number = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    visitor_code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    vehicle_plate_number = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    visitor_period_start = table.Column<DateTime>(type: "datetime2", nullable: true),
                    visitor_period_end = table.Column<DateTime>(type: "datetime2", nullable: true),
                    is_invitation_accepted = table.Column<bool>(type: "bit", nullable: true),
                    invitation_code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    invitation_token_expired_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    masked_area_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    parking_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    visitor_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: true),
                    member_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: true),
                    application_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    TrxStatus = table.Column<int>(type: "int", nullable: false),
                    is_member = table.Column<int>(type: "int", nullable: true),
                    agenda = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VisitorId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    _generate = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    created_by = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_by = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trx_visitor", x => x.id);
                    table.ForeignKey(
                        name: "FK_trx_visitor_floorplan_masked_area_masked_area_id",
                        column: x => x.masked_area_id,
                        principalTable: "floorplan_masked_area",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_trx_visitor_mst_application_application_id",
                        column: x => x.application_id,
                        principalTable: "mst_application",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_trx_visitor_mst_member_member_id",
                        column: x => x.member_id,
                        principalTable: "mst_member",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_trx_visitor_visitor_VisitorId1",
                        column: x => x.VisitorId1,
                        principalTable: "visitor",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_trx_visitor_visitor_visitor_id",
                        column: x => x.visitor_id,
                        principalTable: "visitor",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "visitor_blacklist_area",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    floorplan_masked_area_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    visitor_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    application_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    FloorplanMaskedAreaId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    VisitorId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    _generate = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_visitor_blacklist_area", x => x.id);
                    table.ForeignKey(
                        name: "FK_visitor_blacklist_area_floorplan_masked_area_FloorplanMaskedAreaId1",
                        column: x => x.FloorplanMaskedAreaId1,
                        principalTable: "floorplan_masked_area",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_visitor_blacklist_area_floorplan_masked_area_floorplan_masked_area_id",
                        column: x => x.floorplan_masked_area_id,
                        principalTable: "floorplan_masked_area",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_visitor_blacklist_area_mst_application_application_id",
                        column: x => x.application_id,
                        principalTable: "mst_application",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_visitor_blacklist_area_visitor_VisitorId1",
                        column: x => x.VisitorId1,
                        principalTable: "visitor",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_visitor_blacklist_area_visitor_visitor_id",
                        column: x => x.visitor_id,
                        principalTable: "visitor",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "card_record",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    card_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    visitor_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    member_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    type = table.Column<int>(type: "int", nullable: true),
                    timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    checkin_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    checkout_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    checkin_by = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    checkout_by = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    checkout_masked_area = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    checkin_masked_area = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    visitor_type = table.Column<string>(type: "nvarchar(255)", nullable: true),
                    application_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    CardId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MstMemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    VisitorId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_card_record", x => x.id);
                    table.ForeignKey(
                        name: "FK_card_record_card_CardId1",
                        column: x => x.CardId1,
                        principalTable: "card",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_card_record_card_card_id",
                        column: x => x.card_id,
                        principalTable: "card",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_card_record_mst_application_application_id",
                        column: x => x.application_id,
                        principalTable: "mst_application",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_card_record_mst_member_MstMemberId",
                        column: x => x.MstMemberId,
                        principalTable: "mst_member",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_card_record_mst_member_member_id",
                        column: x => x.member_id,
                        principalTable: "mst_member",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_card_record_visitor_VisitorId1",
                        column: x => x.VisitorId1,
                        principalTable: "visitor",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_card_record_visitor_visitor_id",
                        column: x => x.visitor_id,
                        principalTable: "visitor",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "alarm_record_tracking__generate_unique",
                table: "alarm_record_tracking",
                column: "_generate",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_alarm_record_tracking_application_id",
                table: "alarm_record_tracking",
                column: "application_id");

            migrationBuilder.CreateIndex(
                name: "IX_alarm_record_tracking_ble_reader_id",
                table: "alarm_record_tracking",
                column: "ble_reader_id");

            migrationBuilder.CreateIndex(
                name: "IX_alarm_record_tracking_floorplan_masked_area_id",
                table: "alarm_record_tracking",
                column: "floorplan_masked_area_id");

            migrationBuilder.CreateIndex(
                name: "IX_alarm_record_tracking_FloorplanMaskedAreaId1",
                table: "alarm_record_tracking",
                column: "FloorplanMaskedAreaId1");

            migrationBuilder.CreateIndex(
                name: "IX_alarm_record_tracking_MstApplicationId",
                table: "alarm_record_tracking",
                column: "MstApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_alarm_record_tracking_MstBleReaderId",
                table: "alarm_record_tracking",
                column: "MstBleReaderId");

            migrationBuilder.CreateIndex(
                name: "IX_alarm_record_tracking_visitor_id",
                table: "alarm_record_tracking",
                column: "visitor_id");

            migrationBuilder.CreateIndex(
                name: "IX_alarm_record_tracking_VisitorId1",
                table: "alarm_record_tracking",
                column: "VisitorId1");

            migrationBuilder.CreateIndex(
                name: "IX_ble_reader_node__generate",
                table: "ble_reader_node",
                column: "_generate",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ble_reader_node_application_id",
                table: "ble_reader_node",
                column: "application_id");

            migrationBuilder.CreateIndex(
                name: "IX_ble_reader_node_ble_reader_id",
                table: "ble_reader_node",
                column: "ble_reader_id");

            migrationBuilder.CreateIndex(
                name: "IX_card_application_id",
                table: "card",
                column: "application_id");

            migrationBuilder.CreateIndex(
                name: "IX_card_member_id",
                table: "card",
                column: "member_id");

            migrationBuilder.CreateIndex(
                name: "IX_card_registered_masked_area",
                table: "card",
                column: "registered_masked_area");

            migrationBuilder.CreateIndex(
                name: "IX_card_visitor_id",
                table: "card",
                column: "visitor_id");

            migrationBuilder.CreateIndex(
                name: "IX_card_record_application_id",
                table: "card_record",
                column: "application_id");

            migrationBuilder.CreateIndex(
                name: "IX_card_record_card_id",
                table: "card_record",
                column: "card_id");

            migrationBuilder.CreateIndex(
                name: "IX_card_record_CardId1",
                table: "card_record",
                column: "CardId1");

            migrationBuilder.CreateIndex(
                name: "IX_card_record_member_id",
                table: "card_record",
                column: "member_id");

            migrationBuilder.CreateIndex(
                name: "IX_card_record_MstMemberId",
                table: "card_record",
                column: "MstMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_card_record_visitor_id",
                table: "card_record",
                column: "visitor_id");

            migrationBuilder.CreateIndex(
                name: "IX_card_record_VisitorId1",
                table: "card_record",
                column: "VisitorId1");

            migrationBuilder.CreateIndex(
                name: "IX_floorplan_device__generate",
                table: "floorplan_device",
                column: "_generate",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_floorplan_device_access_cctv_id",
                table: "floorplan_device",
                column: "access_cctv_id");

            migrationBuilder.CreateIndex(
                name: "IX_floorplan_device_access_control_id",
                table: "floorplan_device",
                column: "access_control_id");

            migrationBuilder.CreateIndex(
                name: "IX_floorplan_device_application_id",
                table: "floorplan_device",
                column: "application_id");

            migrationBuilder.CreateIndex(
                name: "IX_floorplan_device_ble_reader_id",
                table: "floorplan_device",
                column: "ble_reader_id");

            migrationBuilder.CreateIndex(
                name: "IX_floorplan_device_floorplan_id",
                table: "floorplan_device",
                column: "floorplan_id");

            migrationBuilder.CreateIndex(
                name: "IX_floorplan_device_floorplan_masked_area_id",
                table: "floorplan_device",
                column: "floorplan_masked_area_id");

            migrationBuilder.CreateIndex(
                name: "IX_floorplan_device_FloorplanMaskedAreaId1",
                table: "floorplan_device",
                column: "FloorplanMaskedAreaId1");

            migrationBuilder.CreateIndex(
                name: "IX_floorplan_device_MstAccessCctvId",
                table: "floorplan_device",
                column: "MstAccessCctvId");

            migrationBuilder.CreateIndex(
                name: "IX_floorplan_device_MstAccessControlId",
                table: "floorplan_device",
                column: "MstAccessControlId");

            migrationBuilder.CreateIndex(
                name: "IX_floorplan_device_MstApplicationId",
                table: "floorplan_device",
                column: "MstApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_floorplan_device_MstBleReaderId",
                table: "floorplan_device",
                column: "MstBleReaderId");

            migrationBuilder.CreateIndex(
                name: "IX_floorplan_masked_area_application_id",
                table: "floorplan_masked_area",
                column: "application_id");

            migrationBuilder.CreateIndex(
                name: "IX_floorplan_masked_area_floor_id",
                table: "floorplan_masked_area",
                column: "floor_id");

            migrationBuilder.CreateIndex(
                name: "IX_floorplan_masked_area_floorplan_id",
                table: "floorplan_masked_area",
                column: "floorplan_id");

            migrationBuilder.CreateIndex(
                name: "IX_floorplan_masked_area_MstFloorId",
                table: "floorplan_masked_area",
                column: "MstFloorId");

            migrationBuilder.CreateIndex(
                name: "IX_mst_access_cctv_application_id",
                table: "mst_access_cctv",
                column: "application_id");

            migrationBuilder.CreateIndex(
                name: "IX_mst_access_cctv_integration_id",
                table: "mst_access_cctv",
                column: "integration_id");

            migrationBuilder.CreateIndex(
                name: "IX_mst_access_cctv_MstApplicationId",
                table: "mst_access_cctv",
                column: "MstApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_mst_access_control_application_id",
                table: "mst_access_control",
                column: "application_id");

            migrationBuilder.CreateIndex(
                name: "IX_mst_access_control_controller_brand_id",
                table: "mst_access_control",
                column: "controller_brand_id");

            migrationBuilder.CreateIndex(
                name: "IX_mst_access_control_integration_id",
                table: "mst_access_control",
                column: "integration_id");

            migrationBuilder.CreateIndex(
                name: "IX_mst_access_control_MstApplicationId",
                table: "mst_access_control",
                column: "MstApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_mst_ble_reader_application_id",
                table: "mst_ble_reader",
                column: "application_id");

            migrationBuilder.CreateIndex(
                name: "IX_mst_ble_reader_brand_id",
                table: "mst_ble_reader",
                column: "brand_id");

            migrationBuilder.CreateIndex(
                name: "IX_mst_brand_application_id",
                table: "mst_brand",
                column: "application_id");

            migrationBuilder.CreateIndex(
                name: "IX_mst_building__generate",
                table: "mst_building",
                column: "_generate",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_mst_building_application_id",
                table: "mst_building",
                column: "application_id");

            migrationBuilder.CreateIndex(
                name: "IX_mst_building_MstApplicationId",
                table: "mst_building",
                column: "MstApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_mst_department_application_id",
                table: "mst_department",
                column: "application_id");

            migrationBuilder.CreateIndex(
                name: "IX_mst_department_MstApplicationId",
                table: "mst_department",
                column: "MstApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_mst_district_application_id",
                table: "mst_district",
                column: "application_id");

            migrationBuilder.CreateIndex(
                name: "IX_mst_district_MstApplicationId",
                table: "mst_district",
                column: "MstApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_mst_engine_application_id",
                table: "mst_engine",
                column: "application_id");

            migrationBuilder.CreateIndex(
                name: "IX_mst_floor_application_id",
                table: "mst_floor",
                column: "application_id");

            migrationBuilder.CreateIndex(
                name: "IX_mst_floor_building_id",
                table: "mst_floor",
                column: "building_id");

            migrationBuilder.CreateIndex(
                name: "IX_mst_floorplan__generate",
                table: "mst_floorplan",
                column: "_generate",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_mst_floorplan_application_id",
                table: "mst_floorplan",
                column: "application_id");

            migrationBuilder.CreateIndex(
                name: "IX_mst_floorplan_floor_id",
                table: "mst_floorplan",
                column: "floor_id");

            migrationBuilder.CreateIndex(
                name: "IX_mst_floorplan_MstApplicationId",
                table: "mst_floorplan",
                column: "MstApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_mst_floorplan_MstFloorId",
                table: "mst_floorplan",
                column: "MstFloorId");

            migrationBuilder.CreateIndex(
                name: "IX_mst_integration_application_id",
                table: "mst_integration",
                column: "application_id");

            migrationBuilder.CreateIndex(
                name: "IX_mst_integration_brand_id",
                table: "mst_integration",
                column: "brand_id");

            migrationBuilder.CreateIndex(
                name: "IX_mst_member_application_id",
                table: "mst_member",
                column: "application_id");

            migrationBuilder.CreateIndex(
                name: "IX_mst_member_department_id",
                table: "mst_member",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "IX_mst_member_district_id",
                table: "mst_member",
                column: "district_id");

            migrationBuilder.CreateIndex(
                name: "IX_mst_member_email",
                table: "mst_member",
                column: "email");

            migrationBuilder.CreateIndex(
                name: "IX_mst_member_MstApplicationId",
                table: "mst_member",
                column: "MstApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_mst_member_MstDepartmentId",
                table: "mst_member",
                column: "MstDepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_mst_member_MstDistrictId",
                table: "mst_member",
                column: "MstDistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_mst_member_MstOrganizationId",
                table: "mst_member",
                column: "MstOrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_mst_member_organization_id",
                table: "mst_member",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "IX_mst_member_person_id",
                table: "mst_member",
                column: "person_id");

            migrationBuilder.CreateIndex(
                name: "IX_mst_organization_application_id",
                table: "mst_organization",
                column: "application_id");

            migrationBuilder.CreateIndex(
                name: "IX_mst_organization_MstApplicationId",
                table: "mst_organization",
                column: "MstApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_refresh_token_user_id",
                table: "refresh_token",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_tracking_transaction_application_id",
                table: "tracking_transaction",
                column: "application_id");

            migrationBuilder.CreateIndex(
                name: "IX_tracking_transaction_floorplan_masked_area_id",
                table: "tracking_transaction",
                column: "floorplan_masked_area_id");

            migrationBuilder.CreateIndex(
                name: "IX_tracking_transaction_FloorplanMaskedAreaId1",
                table: "tracking_transaction",
                column: "FloorplanMaskedAreaId1");

            migrationBuilder.CreateIndex(
                name: "IX_tracking_transaction_MstBleReaderId",
                table: "tracking_transaction",
                column: "MstBleReaderId");

            migrationBuilder.CreateIndex(
                name: "IX_tracking_transaction_reader_id",
                table: "tracking_transaction",
                column: "reader_id");

            migrationBuilder.CreateIndex(
                name: "IX_trx_visitor_application_id",
                table: "trx_visitor",
                column: "application_id");

            migrationBuilder.CreateIndex(
                name: "IX_trx_visitor_masked_area_id",
                table: "trx_visitor",
                column: "masked_area_id");

            migrationBuilder.CreateIndex(
                name: "IX_trx_visitor_member_id",
                table: "trx_visitor",
                column: "member_id");

            migrationBuilder.CreateIndex(
                name: "IX_trx_visitor_visitor_id",
                table: "trx_visitor",
                column: "visitor_id");

            migrationBuilder.CreateIndex(
                name: "IX_trx_visitor_visitor_id_visitor_period_start_visitor_period_end",
                table: "trx_visitor",
                columns: new[] { "visitor_id", "visitor_period_start", "visitor_period_end" },
                unique: true,
                filter: "[visitor_id] IS NOT NULL AND [visitor_period_start] IS NOT NULL AND [visitor_period_end] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_trx_visitor_visitor_id_visitor_status",
                table: "trx_visitor",
                columns: new[] { "visitor_id", "visitor_status" });

            migrationBuilder.CreateIndex(
                name: "IX_trx_visitor_visitor_period_start",
                table: "trx_visitor",
                column: "visitor_period_start");

            migrationBuilder.CreateIndex(
                name: "IX_trx_visitor_visitor_status",
                table: "trx_visitor",
                column: "visitor_status");

            migrationBuilder.CreateIndex(
                name: "IX_trx_visitor_VisitorId1",
                table: "trx_visitor",
                column: "VisitorId1");

            migrationBuilder.CreateIndex(
                name: "IX_user_application_id",
                table: "user",
                column: "application_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_group_id",
                table: "user",
                column: "group_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_UserGroupId",
                table: "user",
                column: "UserGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_user_group_application_id",
                table: "user_group",
                column: "application_id");

            migrationBuilder.CreateIndex(
                name: "IX_visitor_application_id",
                table: "visitor",
                column: "application_id");

            migrationBuilder.CreateIndex(
                name: "IX_visitor_email",
                table: "visitor",
                column: "email");

            migrationBuilder.CreateIndex(
                name: "IX_visitor_MstApplicationId",
                table: "visitor",
                column: "MstApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_visitor_person_id",
                table: "visitor",
                column: "person_id");

            migrationBuilder.CreateIndex(
                name: "IX_visitor_blacklist_area_application_id",
                table: "visitor_blacklist_area",
                column: "application_id");

            migrationBuilder.CreateIndex(
                name: "IX_visitor_blacklist_area_floorplan_masked_area_id",
                table: "visitor_blacklist_area",
                column: "floorplan_masked_area_id");

            migrationBuilder.CreateIndex(
                name: "IX_visitor_blacklist_area_FloorplanMaskedAreaId1",
                table: "visitor_blacklist_area",
                column: "FloorplanMaskedAreaId1");

            migrationBuilder.CreateIndex(
                name: "IX_visitor_blacklist_area_visitor_id",
                table: "visitor_blacklist_area",
                column: "visitor_id");

            migrationBuilder.CreateIndex(
                name: "IX_visitor_blacklist_area_VisitorId1",
                table: "visitor_blacklist_area",
                column: "VisitorId1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "alarm_record_tracking");

            migrationBuilder.DropTable(
                name: "ble_reader_node");

            migrationBuilder.DropTable(
                name: "card_record");

            migrationBuilder.DropTable(
                name: "floorplan_device");

            migrationBuilder.DropTable(
                name: "mst_engine");

            migrationBuilder.DropTable(
                name: "refresh_token");

            migrationBuilder.DropTable(
                name: "tracking_transaction");

            migrationBuilder.DropTable(
                name: "trx_visitor");

            migrationBuilder.DropTable(
                name: "visitor_blacklist_area");

            migrationBuilder.DropTable(
                name: "card");

            migrationBuilder.DropTable(
                name: "mst_access_cctv");

            migrationBuilder.DropTable(
                name: "mst_access_control");

            migrationBuilder.DropTable(
                name: "user");

            migrationBuilder.DropTable(
                name: "mst_ble_reader");

            migrationBuilder.DropTable(
                name: "floorplan_masked_area");

            migrationBuilder.DropTable(
                name: "mst_member");

            migrationBuilder.DropTable(
                name: "visitor");

            migrationBuilder.DropTable(
                name: "mst_integration");

            migrationBuilder.DropTable(
                name: "user_group");

            migrationBuilder.DropTable(
                name: "mst_floorplan");

            migrationBuilder.DropTable(
                name: "mst_department");

            migrationBuilder.DropTable(
                name: "mst_district");

            migrationBuilder.DropTable(
                name: "mst_organization");

            migrationBuilder.DropTable(
                name: "mst_brand");

            migrationBuilder.DropTable(
                name: "mst_floor");

            migrationBuilder.DropTable(
                name: "mst_building");

            migrationBuilder.DropTable(
                name: "mst_application");
        }
    }
}
