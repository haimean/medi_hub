using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediHub.Web.Migrations
{
    /// <inheritdoc />
    public partial class user_v2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_token_valid",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "token_expiration",
                table: "users",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_token_valid",
                table: "users");

            migrationBuilder.DropColumn(
                name: "token_expiration",
                table: "users");
        }
    }
}
