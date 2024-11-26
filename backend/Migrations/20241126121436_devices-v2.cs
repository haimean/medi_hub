using System;
using System.Collections.Generic;
using MediHub.Web.Models;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediHub.Web.Migrations
{
    /// <inheritdoc />
    public partial class devicesv2 : Migration
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
                    manufacturer_name = table.Column<string>(type: "text", nullable: true),
                    manufacturing_year = table.Column<int>(type: "integer", nullable: false),
                    serial_number = table.Column<string>(type: "text", nullable: true),
                    function_name = table.Column<string>(type: "text", nullable: true),
                    installation_contract = table.Column<List<string>>(type: "text[]", nullable: true),
                    machine_status = table.Column<string>(type: "text", nullable: true),
                    import_source = table.Column<string>(type: "text", nullable: true),
                    usage_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    lab_usage = table.Column<string>(type: "text", nullable: true),
                    manager_info = table.Column<ManagerEngineerInfo>(type: "jsonb", nullable: true),
                    engineer_info = table.Column<ManagerEngineerInfo>(type: "jsonb", nullable: true),
                    maintenance_log = table.Column<List<MaintenanceRecord>>(type: "jsonb", nullable: true),
                    maintenance_report = table.Column<List<MaintenanceRecord>>(type: "jsonb", nullable: true),
                    internal_maintenance_check = table.Column<List<MaintenanceRecord>>(type: "jsonb", nullable: true),
                    maintenance_schedule = table.Column<string>(type: "text", nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    name = table.Column<string>(type: "text", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_devices", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "devices");
        }
    }
}
