using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediScope.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDoctorPatientTable : Migration
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
                defaultValue: "pending_admin",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldDefaultValue: "pending");

            migrationBuilder.AlterColumn<Guid>(
                name: "doctor_id",
                table: "doctor_patient",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<DateTime>(
                name: "assigned_at",
                table: "doctor_patient",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "now()");

            migrationBuilder.AddColumn<string>(
                name: "admin_note",
                table: "doctor_patient",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "admin_reviewed_at",
                table: "doctor_patient",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "admin_note",
                table: "doctor_patient");

            migrationBuilder.DropColumn(
                name: "admin_reviewed_at",
                table: "doctor_patient");

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
                oldDefaultValue: "pending_admin");

            migrationBuilder.AlterColumn<Guid>(
                name: "doctor_id",
                table: "doctor_patient",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "assigned_at",
                table: "doctor_patient",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");
        }
    }
}
