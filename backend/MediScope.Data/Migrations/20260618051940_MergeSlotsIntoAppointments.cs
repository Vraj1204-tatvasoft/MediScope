using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediScope.Data.Migrations
{
    /// <inheritdoc />
    public partial class MergeSlotsIntoAppointments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_appointments_appointment_slots_slot_id",
                table: "appointments");

            migrationBuilder.DropTable(
                name: "appointment_slots");

            migrationBuilder.DropIndex(
                name: "IX_appointments_doctor_id",
                table: "appointments");

            migrationBuilder.DropIndex(
                name: "IX_appointments_slot_id",
                table: "appointments");

            migrationBuilder.DropColumn(
                name: "slot_id",
                table: "appointments");

            migrationBuilder.AddColumn<int>(
                name: "duration_minutes",
                table: "appointments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "end_time",
                table: "appointments",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "start_time",
                table: "appointments",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_appointments_doctor_id_start_time",
                table: "appointments",
                columns: new[] { "doctor_id", "start_time" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_appointments_doctor_id_start_time",
                table: "appointments");

            migrationBuilder.DropColumn(
                name: "duration_minutes",
                table: "appointments");

            migrationBuilder.DropColumn(
                name: "end_time",
                table: "appointments");

            migrationBuilder.DropColumn(
                name: "start_time",
                table: "appointments");

            migrationBuilder.AddColumn<Guid>(
                name: "slot_id",
                table: "appointments",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "appointment_slots",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    doctor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true),
                    duration_minutes = table.Column<int>(type: "integer", nullable: false),
                    end_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    notes = table.Column<string>(type: "text", nullable: true),
                    start_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_appointment_slots", x => x.id);
                    table.ForeignKey(
                        name: "FK_appointment_slots_doctors_doctor_id",
                        column: x => x.doctor_id,
                        principalTable: "doctors",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_appointments_doctor_id",
                table: "appointments",
                column: "doctor_id");

            migrationBuilder.CreateIndex(
                name: "IX_appointments_slot_id",
                table: "appointments",
                column: "slot_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_appointment_slots_doctor_id_start_time",
                table: "appointment_slots",
                columns: new[] { "doctor_id", "start_time" });

            migrationBuilder.AddForeignKey(
                name: "FK_appointments_appointment_slots_slot_id",
                table: "appointments",
                column: "slot_id",
                principalTable: "appointment_slots",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
