using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediHub.Web.Migrations
{
    /// <inheritdoc />
    public partial class devices_v4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "device_troubleshooting_instructions",
                table: "devices",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "device_usage_instructions",
                table: "devices",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "device_troubleshooting_instructions",
                table: "devices");

            migrationBuilder.DropColumn(
                name: "device_usage_instructions",
                table: "devices");
        }
    }
}
