using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediScope.Data.Migrations
{
    /// <inheritdoc />
    public partial class ManualSnakeCaseAppointments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_appointment_slots_doctors_DoctorId",
                table: "appointment_slots");

            migrationBuilder.DropForeignKey(
                name: "FK_appointments_appointment_slots_SlotId",
                table: "appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_appointments_doctors_DoctorId",
                table: "appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_appointments_patients_PatientId",
                table: "appointments");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "appointments",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "SlotId",
                table: "appointments",
                newName: "slot_id");

            migrationBuilder.RenameColumn(
                name: "RescheduledTo",
                table: "appointments",
                newName: "rescheduled_to");

            migrationBuilder.RenameColumn(
                name: "RescheduleReason",
                table: "appointments",
                newName: "reschedule_reason");

            migrationBuilder.RenameColumn(
                name: "PatientNotes",
                table: "appointments",
                newName: "patient_notes");

            migrationBuilder.RenameColumn(
                name: "PatientId",
                table: "appointments",
                newName: "patient_id");

            migrationBuilder.RenameColumn(
                name: "DoctorNotes",
                table: "appointments",
                newName: "doctor_notes");

            migrationBuilder.RenameColumn(
                name: "DoctorId",
                table: "appointments",
                newName: "doctor_id");

            migrationBuilder.RenameIndex(
                name: "IX_appointments_SlotId",
                table: "appointments",
                newName: "IX_appointments_slot_id");

            migrationBuilder.RenameIndex(
                name: "IX_appointments_PatientId",
                table: "appointments",
                newName: "IX_appointments_patient_id");

            migrationBuilder.RenameIndex(
                name: "IX_appointments_DoctorId",
                table: "appointments",
                newName: "IX_appointments_doctor_id");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "appointment_slots",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Notes",
                table: "appointment_slots",
                newName: "notes");

            migrationBuilder.RenameColumn(
                name: "StartTime",
                table: "appointment_slots",
                newName: "start_time");

            migrationBuilder.RenameColumn(
                name: "EndTime",
                table: "appointment_slots",
                newName: "end_time");

            migrationBuilder.RenameColumn(
                name: "DurationMinutes",
                table: "appointment_slots",
                newName: "duration_minutes");

            migrationBuilder.RenameColumn(
                name: "DoctorId",
                table: "appointment_slots",
                newName: "doctor_id");

            migrationBuilder.RenameIndex(
                name: "IX_appointment_slots_DoctorId_StartTime",
                table: "appointment_slots",
                newName: "IX_appointment_slots_doctor_id_start_time");

            migrationBuilder.AddForeignKey(
                name: "FK_appointment_slots_doctors_doctor_id",
                table: "appointment_slots",
                column: "doctor_id",
                principalTable: "doctors",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_appointments_appointment_slots_slot_id",
                table: "appointments",
                column: "slot_id",
                principalTable: "appointment_slots",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_appointments_doctors_doctor_id",
                table: "appointments",
                column: "doctor_id",
                principalTable: "doctors",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_appointments_patients_patient_id",
                table: "appointments",
                column: "patient_id",
                principalTable: "patients",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_appointment_slots_doctors_doctor_id",
                table: "appointment_slots");

            migrationBuilder.DropForeignKey(
                name: "FK_appointments_appointment_slots_slot_id",
                table: "appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_appointments_doctors_doctor_id",
                table: "appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_appointments_patients_patient_id",
                table: "appointments");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "appointments",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "slot_id",
                table: "appointments",
                newName: "SlotId");

            migrationBuilder.RenameColumn(
                name: "rescheduled_to",
                table: "appointments",
                newName: "RescheduledTo");

            migrationBuilder.RenameColumn(
                name: "reschedule_reason",
                table: "appointments",
                newName: "RescheduleReason");

            migrationBuilder.RenameColumn(
                name: "patient_notes",
                table: "appointments",
                newName: "PatientNotes");

            migrationBuilder.RenameColumn(
                name: "patient_id",
                table: "appointments",
                newName: "PatientId");

            migrationBuilder.RenameColumn(
                name: "doctor_notes",
                table: "appointments",
                newName: "DoctorNotes");

            migrationBuilder.RenameColumn(
                name: "doctor_id",
                table: "appointments",
                newName: "DoctorId");

            migrationBuilder.RenameIndex(
                name: "IX_appointments_slot_id",
                table: "appointments",
                newName: "IX_appointments_SlotId");

            migrationBuilder.RenameIndex(
                name: "IX_appointments_patient_id",
                table: "appointments",
                newName: "IX_appointments_PatientId");

            migrationBuilder.RenameIndex(
                name: "IX_appointments_doctor_id",
                table: "appointments",
                newName: "IX_appointments_DoctorId");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "appointment_slots",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "notes",
                table: "appointment_slots",
                newName: "Notes");

            migrationBuilder.RenameColumn(
                name: "start_time",
                table: "appointment_slots",
                newName: "StartTime");

            migrationBuilder.RenameColumn(
                name: "end_time",
                table: "appointment_slots",
                newName: "EndTime");

            migrationBuilder.RenameColumn(
                name: "duration_minutes",
                table: "appointment_slots",
                newName: "DurationMinutes");

            migrationBuilder.RenameColumn(
                name: "doctor_id",
                table: "appointment_slots",
                newName: "DoctorId");

            migrationBuilder.RenameIndex(
                name: "IX_appointment_slots_doctor_id_start_time",
                table: "appointment_slots",
                newName: "IX_appointment_slots_DoctorId_StartTime");

            migrationBuilder.AddForeignKey(
                name: "FK_appointment_slots_doctors_DoctorId",
                table: "appointment_slots",
                column: "DoctorId",
                principalTable: "doctors",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_appointments_appointment_slots_SlotId",
                table: "appointments",
                column: "SlotId",
                principalTable: "appointment_slots",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_appointments_doctors_DoctorId",
                table: "appointments",
                column: "DoctorId",
                principalTable: "doctors",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_appointments_patients_PatientId",
                table: "appointments",
                column: "PatientId",
                principalTable: "patients",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
