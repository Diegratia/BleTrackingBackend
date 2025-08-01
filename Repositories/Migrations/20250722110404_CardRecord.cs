﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class CardRecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "card",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    type = table.Column<string>(type: "nvarchar(255)", nullable: false),
                    card_number = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    card_barcode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    is_multi_site = table.Column<bool>(type: "bit", nullable: true),
                    registered_site = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    is_used = table.Column<bool>(type: "bit", nullable: true),
                    last_used_by = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    status_card = table.Column<bool>(type: "bit", nullable: true),
                    _generate = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    created_by = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    updated_by = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_card", x => x.id);
                });

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
                    _generate = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    tag = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mst_brand", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "mst_engine",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    engine_id = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    port = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    is_live = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    last_live = table.Column<DateTime>(type: "datetime2", nullable: false),
                    service_status = table.Column<string>(type: "nvarchar(255)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mst_engine", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "mst_building",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    image = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    application_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    created_by = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_by = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    MstApplicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MstBuildingId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    _generate = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1")
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
                    table.ForeignKey(
                        name: "FK_mst_building_mst_building_MstBuildingId",
                        column: x => x.MstBuildingId,
                        principalTable: "mst_building",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "mst_department",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    _generate = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    code = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    department_host = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    application_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    created_by = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_by = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    MstApplicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
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
                    _generate = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    code = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    district_host = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    application_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    created_by = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_by = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    MstApplicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
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
                name: "mst_organization",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    _generate = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    code = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    organization_host = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    application_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    created_by = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_by = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    MstApplicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
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
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    level_priority = table.Column<int>(type: "int", nullable: false),
                    application_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    created_by = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_by = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    status = table.Column<int>(type: "int", nullable: true)
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
                name: "mst_ble_reader",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    _generate = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    brand_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ip = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    gmac = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    engine_reader_id = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    created_by = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_by = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mst_ble_reader", x => x.id);
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
                    _generate = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    brand_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    integration_type = table.Column<string>(type: "nvarchar(255)", nullable: false),
                    api_type_auth = table.Column<string>(type: "nvarchar(255)", nullable: false),
                    api_url = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    api_auth_username = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    api_auth_passwd = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    api_key_field = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    api_key_value = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    application_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    created_by = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_by = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false, defaultValue: 1)
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
                    generate = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    building_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    floor_image = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    pixel_x = table.Column<float>(type: "real", nullable: false),
                    pixel_y = table.Column<float>(type: "real", nullable: false),
                    floor_x = table.Column<float>(type: "real", nullable: false),
                    floor_y = table.Column<float>(type: "real", nullable: false),
                    meter_per_px = table.Column<float>(type: "real", nullable: false),
                    engine_floor_id = table.Column<long>(type: "bigint", nullable: false),
                    created_by = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_by = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mst_floor", x => x.id);
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
                    identity_id = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    card_number = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ble_card_number = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    phone = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    gender = table.Column<string>(type: "nvarchar(255)", nullable: false),
                    address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    face_image = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    upload_fr = table.Column<int>(type: "int", nullable: false),
                    upload_fr_error = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    birth_date = table.Column<DateOnly>(type: "date", nullable: false),
                    join_date = table.Column<DateOnly>(type: "date", nullable: false),
                    exit_date = table.Column<DateOnly>(type: "date", nullable: false),
                    head_member1 = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    head_member2 = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    application_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    status_employee = table.Column<string>(type: "nvarchar(255)", nullable: false),
                    created_by = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_by = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    MstApplicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MstDepartmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MstDistrictId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MstOrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    _generate = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1")
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
                name: "visitor",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    person_id = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    identity_id = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    card_number = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ble_card_number = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    visitor_type = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    phone = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    gender = table.Column<string>(type: "nvarchar(255)", nullable: false),
                    address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    organization_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    district_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    department_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    is_vip = table.Column<bool>(type: "bit", nullable: false),
                    is_email_vervied = table.Column<bool>(type: "bit", nullable: false),
                    email_verification_send_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    email_verification_token = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    visitor_period_start = table.Column<DateTime>(type: "datetime2", nullable: false),
                    visitor_period_end = table.Column<DateTime>(type: "datetime2", nullable: false),
                    is_employee = table.Column<bool>(type: "bit", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
                    face_image = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    upload_fr = table.Column<int>(type: "int", nullable: false),
                    upload_fr_error = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    application_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    MstApplicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    _generate = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1")
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
                    table.ForeignKey(
                        name: "FK_visitor_mst_department_department_id",
                        column: x => x.department_id,
                        principalTable: "mst_department",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_visitor_mst_district_district_id",
                        column: x => x.district_id,
                        principalTable: "mst_district",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_visitor_mst_organization_organization_id",
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
                    UserGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    _generate = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user", x => x.id);
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
                    start_pos = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    end_pos = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    distance_px = table.Column<float>(type: "real", maxLength: 255, nullable: false),
                    distance = table.Column<float>(type: "real", maxLength: 255, nullable: false),
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
                    created_by = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_by = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    integration_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    application_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    status = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    MstApplicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    _generate = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1")
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
                    controller_brand_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    type = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    channel = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    door_id = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    raw = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    integration_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    application_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    created_by = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_by = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    MstApplicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    _generate = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1")
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
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
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
                    name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    floor_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    application_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    created_by = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_by = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    MstApplicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MstFloorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    _generate = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1")
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
                name: "trx_visitor",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    checked_in_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    checked_out_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    deny_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    block_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    unblock_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    checkin_by = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    checkout_by = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    deny_by = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    deny_reason = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    block_by = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    block_reason = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    visitor_type = table.Column<string>(type: "nvarchar(255)", nullable: false),
                    invitation_created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    visitor_group_code = table.Column<long>(type: "bigint", nullable: false),
                    visitor_number = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    visitor_code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    vehicle_plate_number = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    remarks = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    site_id = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    parking_id = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    visitor_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trx_visitor", x => x.id);
                    table.ForeignKey(
                        name: "FK_trx_visitor_visitor_visitor_id",
                        column: x => x.visitor_id,
                        principalTable: "visitor",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "visitor_card",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    number = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    card_type = table.Column<string>(type: "nvarchar(255)", nullable: true),
                    qr_code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    mac = table.Column<string>(type: "nvarchar(255)", nullable: true),
                    card_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    checkin_status = table.Column<int>(type: "int", nullable: true),
                    enable_status = table.Column<int>(type: "int", nullable: true),
                    status = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    site_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    is_visitor = table.Column<int>(type: "int", nullable: true),
                    application_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    visitor_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    member_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    _generate = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    created_by = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    updated_by = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_visitor_card", x => x.id);
                    table.ForeignKey(
                        name: "FK_visitor_card_card_card_id",
                        column: x => x.card_id,
                        principalTable: "card",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_visitor_card_mst_application_application_id",
                        column: x => x.application_id,
                        principalTable: "mst_application",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_visitor_card_mst_member_member_id",
                        column: x => x.member_id,
                        principalTable: "mst_member",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_visitor_card_visitor_visitor_id",
                        column: x => x.visitor_id,
                        principalTable: "visitor",
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
                    _generate = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    floorplan_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    floor_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    area_shape = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    color_area = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    restricted_status = table.Column<string>(type: "nvarchar(255)", nullable: false),
                    engine_area_id = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    created_by = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_by = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    MstFloorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_floorplan_masked_area", x => x.id);
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
                name: "card_record",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    visitor_card_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    visitor_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    member_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    type = table.Column<int>(type: "int", nullable: true),
                    timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    checkin_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    checkout_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    checkin_by = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    checkout_by = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    checkout_site_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    checkin_site_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    visitor_type = table.Column<string>(type: "nvarchar(255)", nullable: false),
                    MstMemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    VisitorCardId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    VisitorId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_card_record", x => x.id);
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
                        name: "FK_card_record_visitor_card_VisitorCardId1",
                        column: x => x.VisitorCardId1,
                        principalTable: "visitor_card",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_card_record_visitor_card_visitor_card_id",
                        column: x => x.visitor_card_id,
                        principalTable: "visitor_card",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_card_record_visitor_visitor_id",
                        column: x => x.visitor_id,
                        principalTable: "visitor",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "alarm_record_tracking",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    visitor_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    ble_reader_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    floorplan_masked_area_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    alarm_record_status = table.Column<string>(type: "nvarchar(255)", nullable: false),
                    action = table.Column<string>(type: "nvarchar(255)", nullable: false),
                    application_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    idle_timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    done_timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    cancel_timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    waiting_timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    investigated_timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    investigated_done_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    idle_by = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    done_by = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    cancel_by = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    waiting_by = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    investigated_by = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    investigated_result = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FloorplanMaskedAreaId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                name: "floorplan_device",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    type = table.Column<string>(type: "nvarchar(max)", nullable: false),
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
                    created_by = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_by = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    device_status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
                    FloorplanMaskedAreaId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MstAccessCctvId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MstAccessControlId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MstApplicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MstBleReaderId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    _generate = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1")
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
                name: "visitor_blacklist_area",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    floorplan_masked_area_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    visitor_id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
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

            migrationBuilder.CreateIndex(
                name: "alarm_record_tracking__generate_unique",
                table: "alarm_record_tracking",
                column: "_generate",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_alarm_record_tracking_application_id",
                table: "alarm_record_tracking",
                column: "application_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_alarm_record_tracking_ble_reader_id",
                table: "alarm_record_tracking",
                column: "ble_reader_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_alarm_record_tracking_floorplan_masked_area_id",
                table: "alarm_record_tracking",
                column: "floorplan_masked_area_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_alarm_record_tracking_FloorplanMaskedAreaId1",
                table: "alarm_record_tracking",
                column: "FloorplanMaskedAreaId1");

            migrationBuilder.CreateIndex(
                name: "IX_alarm_record_tracking_MstBleReaderId",
                table: "alarm_record_tracking",
                column: "MstBleReaderId");

            migrationBuilder.CreateIndex(
                name: "IX_alarm_record_tracking_visitor_id",
                table: "alarm_record_tracking",
                column: "visitor_id",
                unique: true);

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
                name: "IX_card_record_member_id",
                table: "card_record",
                column: "member_id");

            migrationBuilder.CreateIndex(
                name: "IX_card_record_MstMemberId",
                table: "card_record",
                column: "MstMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_card_record_visitor_card_id",
                table: "card_record",
                column: "visitor_card_id");

            migrationBuilder.CreateIndex(
                name: "IX_card_record_visitor_id",
                table: "card_record",
                column: "visitor_id");

            migrationBuilder.CreateIndex(
                name: "IX_card_record_VisitorCardId1",
                table: "card_record",
                column: "VisitorCardId1");

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
                name: "IX_mst_ble_reader_brand_id",
                table: "mst_ble_reader",
                column: "brand_id");

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
                name: "IX_mst_building_MstBuildingId",
                table: "mst_building",
                column: "MstBuildingId");

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
                name: "IX_trx_visitor_visitor_id",
                table: "trx_visitor",
                column: "visitor_id");

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
                name: "IX_visitor_department_id",
                table: "visitor",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "IX_visitor_district_id",
                table: "visitor",
                column: "district_id");

            migrationBuilder.CreateIndex(
                name: "IX_visitor_email",
                table: "visitor",
                column: "email");

            migrationBuilder.CreateIndex(
                name: "IX_visitor_MstApplicationId",
                table: "visitor",
                column: "MstApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_visitor_organization_id",
                table: "visitor",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "IX_visitor_person_id",
                table: "visitor",
                column: "person_id");

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

            migrationBuilder.CreateIndex(
                name: "IX_visitor_card_application_id",
                table: "visitor_card",
                column: "application_id");

            migrationBuilder.CreateIndex(
                name: "IX_visitor_card_card_id",
                table: "visitor_card",
                column: "card_id");

            migrationBuilder.CreateIndex(
                name: "IX_visitor_card_member_id",
                table: "visitor_card",
                column: "member_id");

            migrationBuilder.CreateIndex(
                name: "IX_visitor_card_visitor_id",
                table: "visitor_card",
                column: "visitor_id");
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
                name: "visitor_card");

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
                name: "card");

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
