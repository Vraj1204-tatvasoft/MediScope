using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediScope.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddQuestionnaireAssignmentAndDraftStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PatientId1",
                table: "questionnaire_submissions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "assignment_id",
                table: "questionnaire_submissions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "pdf_path",
                table: "questionnaire_submissions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "status",
                table: "questionnaire_submissions",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Draft");

            migrationBuilder.AddColumn<DateTime>(
                name: "submitted_at",
                table: "questionnaire_submissions",
                type: "timestamptz",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "questionnaire_assignments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    questionnaire_id = table.Column<Guid>(type: "uuid", nullable: false),
                    patient_id = table.Column<Guid>(type: "uuid", nullable: false),
                    assigned_by = table.Column<Guid>(type: "uuid", nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_questionnaire_assignments", x => x.id);
                    table.ForeignKey(
                        name: "FK_questionnaire_assignments_patients_patient_id",
                        column: x => x.patient_id,
                        principalTable: "patients",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_questionnaire_assignments_questionnaires_questionnaire_id",
                        column: x => x.questionnaire_id,
                        principalTable: "questionnaires",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "idx_qs_assignment_id",
                table: "questionnaire_submissions",
                column: "assignment_id");

            migrationBuilder.CreateIndex(
                name: "IX_questionnaire_submissions_PatientId1",
                table: "questionnaire_submissions",
                column: "PatientId1");

            migrationBuilder.CreateIndex(
                name: "idx_qa_assigned_by",
                table: "questionnaire_assignments",
                column: "assigned_by");

            migrationBuilder.CreateIndex(
                name: "idx_qa_patient_id",
                table: "questionnaire_assignments",
                column: "patient_id");

            migrationBuilder.CreateIndex(
                name: "idx_qa_questionnaire_id",
                table: "questionnaire_assignments",
                column: "questionnaire_id");

            migrationBuilder.AddForeignKey(
                name: "FK_questionnaire_submissions_patients_PatientId1",
                table: "questionnaire_submissions",
                column: "PatientId1",
                principalTable: "patients",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_questionnaire_submissions_questionnaire_assignments_assignm~",
                table: "questionnaire_submissions",
                column: "assignment_id",
                principalTable: "questionnaire_assignments",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_questionnaire_submissions_patients_PatientId1",
                table: "questionnaire_submissions");

            migrationBuilder.DropForeignKey(
                name: "FK_questionnaire_submissions_questionnaire_assignments_assignm~",
                table: "questionnaire_submissions");

            migrationBuilder.DropTable(
                name: "questionnaire_assignments");

            migrationBuilder.DropIndex(
                name: "idx_qs_assignment_id",
                table: "questionnaire_submissions");

            migrationBuilder.DropIndex(
                name: "IX_questionnaire_submissions_PatientId1",
                table: "questionnaire_submissions");

            migrationBuilder.DropColumn(
                name: "PatientId1",
                table: "questionnaire_submissions");

            migrationBuilder.DropColumn(
                name: "assignment_id",
                table: "questionnaire_submissions");

            migrationBuilder.DropColumn(
                name: "pdf_path",
                table: "questionnaire_submissions");

            migrationBuilder.DropColumn(
                name: "status",
                table: "questionnaire_submissions");

            migrationBuilder.DropColumn(
                name: "submitted_at",
                table: "questionnaire_submissions");
        }
    }
}
