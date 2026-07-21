using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediScope.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPatientAdmissionModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "patient_admissions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    admission_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    patient_id = table.Column<Guid>(type: "uuid", nullable: false),
                    doctor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ward_id = table.Column<Guid>(type: "uuid", nullable: false),
                    room_id = table.Column<Guid>(type: "uuid", nullable: false),
                    bed_id = table.Column<Guid>(type: "uuid", nullable: false),
                    admission_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    admission_reason = table.Column<string>(type: "text", nullable: false),
                    expected_discharge_date = table.Column<DateOnly>(type: "date", nullable: true),
                    actual_discharge_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    discharge_notes = table.Column<string>(type: "text", nullable: true),
                    remarks = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_patient_admissions", x => x.id);
                    table.ForeignKey(
                        name: "FK_patient_admissions_beds_bed_id",
                        column: x => x.bed_id,
                        principalTable: "beds",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_patient_admissions_doctors_doctor_id",
                        column: x => x.doctor_id,
                        principalTable: "doctors",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_patient_admissions_patients_patient_id",
                        column: x => x.patient_id,
                        principalTable: "patients",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_patient_admissions_rooms_room_id",
                        column: x => x.room_id,
                        principalTable: "rooms",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_patient_admissions_wards_ward_id",
                        column: x => x.ward_id,
                        principalTable: "wards",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "bed_transfer_history",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    admission_id = table.Column<Guid>(type: "uuid", nullable: false),
                    from_ward_id = table.Column<Guid>(type: "uuid", nullable: false),
                    from_room_id = table.Column<Guid>(type: "uuid", nullable: false),
                    from_bed_id = table.Column<Guid>(type: "uuid", nullable: false),
                    to_ward_id = table.Column<Guid>(type: "uuid", nullable: false),
                    to_room_id = table.Column<Guid>(type: "uuid", nullable: false),
                    to_bed_id = table.Column<Guid>(type: "uuid", nullable: false),
                    transfer_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    transfer_reason = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bed_transfer_history", x => x.id);
                    table.ForeignKey(
                        name: "FK_bed_transfer_history_beds_from_bed_id",
                        column: x => x.from_bed_id,
                        principalTable: "beds",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_bed_transfer_history_beds_to_bed_id",
                        column: x => x.to_bed_id,
                        principalTable: "beds",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_bed_transfer_history_patient_admissions_admission_id",
                        column: x => x.admission_id,
                        principalTable: "patient_admissions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_bed_transfer_history_rooms_from_room_id",
                        column: x => x.from_room_id,
                        principalTable: "rooms",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_bed_transfer_history_rooms_to_room_id",
                        column: x => x.to_room_id,
                        principalTable: "rooms",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_bed_transfer_history_wards_from_ward_id",
                        column: x => x.from_ward_id,
                        principalTable: "wards",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_bed_transfer_history_wards_to_ward_id",
                        column: x => x.to_ward_id,
                        principalTable: "wards",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_bed_transfer_history_admission_id",
                table: "bed_transfer_history",
                column: "admission_id");

            migrationBuilder.CreateIndex(
                name: "IX_bed_transfer_history_from_bed_id",
                table: "bed_transfer_history",
                column: "from_bed_id");

            migrationBuilder.CreateIndex(
                name: "IX_bed_transfer_history_from_room_id",
                table: "bed_transfer_history",
                column: "from_room_id");

            migrationBuilder.CreateIndex(
                name: "IX_bed_transfer_history_from_ward_id",
                table: "bed_transfer_history",
                column: "from_ward_id");

            migrationBuilder.CreateIndex(
                name: "IX_bed_transfer_history_to_bed_id",
                table: "bed_transfer_history",
                column: "to_bed_id");

            migrationBuilder.CreateIndex(
                name: "IX_bed_transfer_history_to_room_id",
                table: "bed_transfer_history",
                column: "to_room_id");

            migrationBuilder.CreateIndex(
                name: "IX_bed_transfer_history_to_ward_id",
                table: "bed_transfer_history",
                column: "to_ward_id");

            migrationBuilder.CreateIndex(
                name: "IX_patient_admissions_bed_id",
                table: "patient_admissions",
                column: "bed_id");

            migrationBuilder.CreateIndex(
                name: "IX_patient_admissions_doctor_id",
                table: "patient_admissions",
                column: "doctor_id");

            migrationBuilder.CreateIndex(
                name: "IX_patient_admissions_patient_id",
                table: "patient_admissions",
                column: "patient_id");

            migrationBuilder.CreateIndex(
                name: "IX_patient_admissions_room_id",
                table: "patient_admissions",
                column: "room_id");

            migrationBuilder.CreateIndex(
                name: "IX_patient_admissions_ward_id",
                table: "patient_admissions",
                column: "ward_id");

            migrationBuilder.CreateIndex(
                name: "uq_admission_number",
                table: "patient_admissions",
                column: "admission_number",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bed_transfer_history");

            migrationBuilder.DropTable(
                name: "patient_admissions");
        }
    }
}
