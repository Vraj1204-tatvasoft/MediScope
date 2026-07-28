using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediScope.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddQuestionnaireModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "questionnaires",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    department = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    status = table.Column<string>(type: "text", nullable: false, defaultValue: "Active"),
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
                    table.PrimaryKey("PK_questionnaires", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "questionnaire_questions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    questionnaire_id = table.Column<Guid>(type: "uuid", nullable: false),
                    label = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    field_type = table.Column<string>(type: "text", nullable: false),
                    placeholder = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    is_required = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    display_order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    default_value = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_questionnaire_questions", x => x.id);
                    table.ForeignKey(
                        name: "FK_questionnaire_questions_questionnaires_questionnaire_id",
                        column: x => x.questionnaire_id,
                        principalTable: "questionnaires",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "questionnaire_submissions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    questionnaire_id = table.Column<Guid>(type: "uuid", nullable: false),
                    patient_id = table.Column<Guid>(type: "uuid", nullable: false),
                    submitted_by = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_questionnaire_submissions", x => x.id);
                    table.ForeignKey(
                        name: "FK_questionnaire_submissions_patients_patient_id",
                        column: x => x.patient_id,
                        principalTable: "patients",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_questionnaire_submissions_questionnaires_questionnaire_id",
                        column: x => x.questionnaire_id,
                        principalTable: "questionnaires",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "question_options",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    question_id = table.Column<Guid>(type: "uuid", nullable: false),
                    option_label = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    option_value = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    display_order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_question_options", x => x.id);
                    table.ForeignKey(
                        name: "FK_question_options_questionnaire_questions_question_id",
                        column: x => x.question_id,
                        principalTable: "questionnaire_questions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "submission_responses",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    submission_id = table.Column<Guid>(type: "uuid", nullable: false),
                    question_id = table.Column<Guid>(type: "uuid", nullable: false),
                    response_value = table.Column<string>(type: "text", nullable: true),
                    response_values = table.Column<string[]>(type: "text[]", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_submission_responses", x => x.id);
                    table.ForeignKey(
                        name: "FK_submission_responses_questionnaire_questions_question_id",
                        column: x => x.question_id,
                        principalTable: "questionnaire_questions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_submission_responses_questionnaire_submissions_submission_id",
                        column: x => x.submission_id,
                        principalTable: "questionnaire_submissions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "idx_qo_question_id",
                table: "question_options",
                column: "question_id");

            migrationBuilder.CreateIndex(
                name: "idx_qq_questionnaire_id",
                table: "questionnaire_questions",
                column: "questionnaire_id");

            migrationBuilder.CreateIndex(
                name: "idx_qs_patient_id",
                table: "questionnaire_submissions",
                column: "patient_id");

            migrationBuilder.CreateIndex(
                name: "idx_qs_questionnaire_id",
                table: "questionnaire_submissions",
                column: "questionnaire_id");

            migrationBuilder.CreateIndex(
                name: "idx_questionnaires_status_deleted",
                table: "questionnaires",
                columns: new[] { "status", "is_deleted" });

            migrationBuilder.CreateIndex(
                name: "idx_sr_submission_id",
                table: "submission_responses",
                column: "submission_id");

            migrationBuilder.CreateIndex(
                name: "IX_submission_responses_question_id",
                table: "submission_responses",
                column: "question_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "question_options");

            migrationBuilder.DropTable(
                name: "submission_responses");

            migrationBuilder.DropTable(
                name: "questionnaire_questions");

            migrationBuilder.DropTable(
                name: "questionnaire_submissions");

            migrationBuilder.DropTable(
                name: "questionnaires");
        }
    }
}
