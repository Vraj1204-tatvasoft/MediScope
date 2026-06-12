using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediScope.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMedicalDocumentsModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "medical_documents",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    patient_id = table.Column<Guid>(type: "uuid", nullable: false),
                    doctor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    file_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    stored_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    file_path = table.Column<string>(type: "text", nullable: false),
                    content_type = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    file_size_bytes = table.Column<long>(type: "bigint", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    is_viewed_by_doctor = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_reviewed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    feedback = table.Column<string>(type: "text", nullable: true),
                    severity = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    reviewed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_medical_documents", x => x.id);
                    table.ForeignKey(
                        name: "FK_medical_documents_doctors_doctor_id",
                        column: x => x.doctor_id,
                        principalTable: "doctors",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_medical_documents_patients_patient_id",
                        column: x => x.patient_id,
                        principalTable: "patients",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "idx_medical_documents_doctor",
                table: "medical_documents",
                column: "doctor_id");

            migrationBuilder.CreateIndex(
                name: "idx_medical_documents_patient",
                table: "medical_documents",
                column: "patient_id");

            migrationBuilder.CreateIndex(
                name: "idx_medical_documents_patient_doctor",
                table: "medical_documents",
                columns: new[] { "patient_id", "doctor_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "medical_documents");
        }
    }
}
