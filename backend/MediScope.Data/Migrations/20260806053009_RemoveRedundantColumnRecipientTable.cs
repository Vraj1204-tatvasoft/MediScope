using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediScope.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveRedundantColumnRecipientTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "device_token",
                table: "broadcast_recipients");

            migrationBuilder.DropColumn(
                name: "email",
                table: "broadcast_recipients");

            migrationBuilder.DropColumn(
                name: "full_name",
                table: "broadcast_recipients");

            migrationBuilder.DropColumn(
                name: "phone",
                table: "broadcast_recipients");

            migrationBuilder.DropColumn(
                name: "role",
                table: "broadcast_recipients");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "device_token",
                table: "broadcast_recipients",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "email",
                table: "broadcast_recipients",
                type: "character varying(320)",
                maxLength: 320,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "full_name",
                table: "broadcast_recipients",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "phone",
                table: "broadcast_recipients",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "role",
                table: "broadcast_recipients",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
