using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediScope.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSessionIdToUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "current_session_id",
                table: "users",
                type: "uuid",
                nullable: false,
                defaultValueSql: "gen_random_uuid()");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "current_session_id",
                table: "users");
        }
    }
}
