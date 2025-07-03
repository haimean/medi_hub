using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediHub.Web.Migrations
{
    /// <inheritdoc />
    public partial class NewDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "devices",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    device_avatar = table.Column<List<string>>(type: "text[]", nullable: true),
                    device_code = table.Column<string>(type: "text", nullable: true),
                    device_name = table.Column<string>(type: "text", nullable: true),
                    manufacturer_country = table.Column<string>(type: "text", nullable: true),
                    manufacturer_name = table.Column<int>(type: "integer", nullable: false),
                    manufacturing_year = table.Column<int>(type: "integer", nullable: false),
                    serial_number = table.Column<string>(type: "text", nullable: true),
                    machine_status = table.Column<int>(type: "integer", nullable: false),
                    import_source = table.Column<string>(type: "text", nullable: true),
                    function_name = table.Column<string>(type: "text", nullable: true),
                    installation_contract = table.Column<string>(type: "text", nullable: true),
                    usage_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpirationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    lab_usage = table.Column<string>(type: "text", nullable: true),
                    manager_info = table.Column<string>(type: "text", nullable: true),
                    manager_phonenumber = table.Column<string>(type: "text", nullable: true),
                    engineer_info = table.Column<string>(type: "text", nullable: true),
                    engineer_phonenumber = table.Column<string>(type: "text", nullable: true),
                    device_usage_instructions = table.Column<string>(type: "text", nullable: true),
                    appraisal_file = table.Column<string>(type: "text", nullable: true),
                    device_status = table.Column<int>(type: "integer", nullable: false),
                    maintenance_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    maintenance_next_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    maintenance_schedule = table.Column<int>(type: "integer", nullable: false),
                    calibration_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    calibration_next_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    replace_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    replace_next_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    name = table.Column<string>(type: "text", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_devices", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "MaintenanceRecordEntity",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    maintaind_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    maintenance_date = table.Column<string>(type: "text", nullable: true),
                    file_links = table.Column<string>(type: "text", nullable: true),
                    device_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type_of_maintenance = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "text", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaintenanceRecordEntity", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    username = table.Column<string>(type: "text", nullable: true),
                    password_hash = table.Column<string>(type: "text", nullable: true),
                    email = table.Column<string>(type: "text", nullable: true),
                    phone_number = table.Column<string>(type: "text", nullable: true),
                    full_name = table.Column<string>(type: "text", nullable: true),
                    role = table.Column<List<string>>(type: "text[]", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    last_login = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_logout = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    token_expiration = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_token_valid = table.Column<bool>(type: "boolean", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    name = table.Column<string>(type: "text", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "devices");

            migrationBuilder.DropTable(
                name: "MaintenanceRecordEntity");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
