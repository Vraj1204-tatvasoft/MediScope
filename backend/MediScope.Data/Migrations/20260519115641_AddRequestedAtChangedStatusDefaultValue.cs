using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediScope.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRequestedAtChangedStatusDefaultValue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "doctor_patient",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "pending",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldDefaultValue: "active");

            migrationBuilder.AddColumn<DateTime>(
                name: "requested_at",
                table: "doctor_patient",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "requested_at",
                table: "doctor_patient");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "doctor_patient",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "active",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldDefaultValue: "pending");
        }
    }
}
