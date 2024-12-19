using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediHub.Web.Migrations
{
    /// <inheritdoc />
    public partial class devices_v3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "contract_duration",
                table: "devices",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "contract_duration",
                table: "devices");
        }
    }
}
