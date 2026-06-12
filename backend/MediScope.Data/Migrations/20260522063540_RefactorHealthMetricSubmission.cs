using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediScope.Data.Migrations
{
    /// <inheritdoc />
    public partial class RefactorHealthMetricSubmission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_health_metrics_patients_patient_id",
                table: "health_metrics");

            migrationBuilder.DropForeignKey(
                name: "FK_health_metrics_users_recorded_by_user_id",
                table: "health_metrics");

            migrationBuilder.DropIndex(
                name: "idx_hm_patient_submission",
                table: "health_metrics");

            migrationBuilder.DropIndex(
                name: "idx_hm_patient_type_date",
                table: "health_metrics");

            migrationBuilder.DropIndex(
                name: "IX_health_metrics_recorded_by_user_id",
                table: "health_metrics");

            migrationBuilder.DropColumn(
                name: "notes",
                table: "health_metrics");

            migrationBuilder.DropColumn(
                name: "recorded_at",
                table: "health_metrics");

            migrationBuilder.DropColumn(
                name: "recorded_by_role",
                table: "health_metrics");

            migrationBuilder.DropColumn(
                name: "recorded_by_user_id",
                table: "health_metrics");

            migrationBuilder.RenameColumn(
                name: "patient_id",
                table: "health_metrics",
                newName: "PatientId");

            migrationBuilder.RenameColumn(
                name: "submission_group_id",
                table: "health_metrics",
                newName: "submission_id");

            migrationBuilder.RenameIndex(
                name: "IX_health_metrics_metric_type",
                table: "health_metrics",
                newName: "idx_hm_metric_type");

            migrationBuilder.AlterColumn<Guid>(
                name: "PatientId",
                table: "health_metrics",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.CreateTable(
                name: "health_metric_submissions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    patient_id = table.Column<Guid>(type: "uuid", nullable: false),
                    recorded_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    recorded_by_role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    recorded_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
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
                    table.PrimaryKey("PK_health_metric_submissions", x => x.id);
                    table.ForeignKey(
                        name: "FK_health_metric_submissions_patients_patient_id",
                        column: x => x.patient_id,
                        principalTable: "patients",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_health_metric_submissions_users_recorded_by_user_id",
                        column: x => x.recorded_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "idx_hm_submission",
                table: "health_metrics",
                column: "submission_id");

            migrationBuilder.CreateIndex(
                name: "IX_health_metrics_PatientId",
                table: "health_metrics",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "idx_hms_patient_date",
                table: "health_metric_submissions",
                columns: new[] { "patient_id", "recorded_at" });

            migrationBuilder.CreateIndex(
                name: "idx_hms_patient_status",
                table: "health_metric_submissions",
                columns: new[] { "patient_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_health_metric_submissions_recorded_by_user_id",
                table: "health_metric_submissions",
                column: "recorded_by_user_id");

            migrationBuilder.AddForeignKey(
                name: "FK_health_metrics_health_metric_submissions_submission_id",
                table: "health_metrics",
                column: "submission_id",
                principalTable: "health_metric_submissions",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_health_metrics_patients_PatientId",
                table: "health_metrics",
                column: "PatientId",
                principalTable: "patients",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_health_metrics_health_metric_submissions_submission_id",
                table: "health_metrics");

            migrationBuilder.DropForeignKey(
                name: "FK_health_metrics_patients_PatientId",
                table: "health_metrics");

            migrationBuilder.DropTable(
                name: "health_metric_submissions");

            migrationBuilder.DropIndex(
                name: "idx_hm_submission",
                table: "health_metrics");

            migrationBuilder.DropIndex(
                name: "IX_health_metrics_PatientId",
                table: "health_metrics");

            migrationBuilder.RenameColumn(
                name: "PatientId",
                table: "health_metrics",
                newName: "patient_id");

            migrationBuilder.RenameColumn(
                name: "submission_id",
                table: "health_metrics",
                newName: "submission_group_id");

            migrationBuilder.RenameIndex(
                name: "idx_hm_metric_type",
                table: "health_metrics",
                newName: "IX_health_metrics_metric_type");

            migrationBuilder.AlterColumn<Guid>(
                name: "patient_id",
                table: "health_metrics",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "notes",
                table: "health_metrics",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "recorded_at",
                table: "health_metrics",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "recorded_by_role",
                table: "health_metrics",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "recorded_by_user_id",
                table: "health_metrics",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "idx_hm_patient_submission",
                table: "health_metrics",
                columns: new[] { "patient_id", "submission_group_id" });

            migrationBuilder.CreateIndex(
                name: "idx_hm_patient_type_date",
                table: "health_metrics",
                columns: new[] { "patient_id", "metric_type", "recorded_at" });

            migrationBuilder.CreateIndex(
                name: "IX_health_metrics_recorded_by_user_id",
                table: "health_metrics",
                column: "recorded_by_user_id");

            migrationBuilder.AddForeignKey(
                name: "FK_health_metrics_patients_patient_id",
                table: "health_metrics",
                column: "patient_id",
                principalTable: "patients",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_health_metrics_users_recorded_by_user_id",
                table: "health_metrics",
                column: "recorded_by_user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
