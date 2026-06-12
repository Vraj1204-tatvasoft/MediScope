using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediScope.Data.Migrations
{
    /// <inheritdoc />
    public partial class FlattenHealthMetrics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_health_metrics_health_metric_submissions_submission_id",
                table: "health_metrics");

            migrationBuilder.DropForeignKey(
                name: "FK_health_metrics_patients_PatientId",
                table: "health_metrics");

            migrationBuilder.RenameColumn(
                name: "PatientId",
                table: "health_metrics",
                newName: "patient_id");

            migrationBuilder.RenameIndex(
                name: "IX_health_metrics_PatientId",
                table: "health_metrics",
                newName: "idx_hm_patient_id");

            migrationBuilder.AlterColumn<Guid>(
                name: "patient_id",
                table: "health_metrics",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "HealthMetricSubmissionId",
                table: "health_metrics",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PatientId1",
                table: "health_metrics",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "notes",
                table: "health_metrics",
                type: "character varying(1000)",
                maxLength: 1000,
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
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "recorded_by_user_id",
                table: "health_metrics",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "status",
                table: "health_metrics",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "NORMAL");

            migrationBuilder.CreateIndex(
                name: "idx_hm_recorded_at",
                table: "health_metrics",
                column: "recorded_at");

            migrationBuilder.CreateIndex(
                name: "IX_health_metrics_HealthMetricSubmissionId",
                table: "health_metrics",
                column: "HealthMetricSubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_health_metrics_PatientId1",
                table: "health_metrics",
                column: "PatientId1");

            migrationBuilder.CreateIndex(
                name: "IX_health_metrics_recorded_by_user_id",
                table: "health_metrics",
                column: "recorded_by_user_id");

            migrationBuilder.AddForeignKey(
                name: "FK_health_metrics_health_metric_submissions_HealthMetricSubmis~",
                table: "health_metrics",
                column: "HealthMetricSubmissionId",
                principalTable: "health_metric_submissions",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_health_metrics_patients_PatientId1",
                table: "health_metrics",
                column: "PatientId1",
                principalTable: "patients",
                principalColumn: "id");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_health_metrics_health_metric_submissions_HealthMetricSubmis~",
                table: "health_metrics");

            migrationBuilder.DropForeignKey(
                name: "FK_health_metrics_patients_PatientId1",
                table: "health_metrics");

            migrationBuilder.DropForeignKey(
                name: "FK_health_metrics_patients_patient_id",
                table: "health_metrics");

            migrationBuilder.DropForeignKey(
                name: "FK_health_metrics_users_recorded_by_user_id",
                table: "health_metrics");

            migrationBuilder.DropIndex(
                name: "idx_hm_recorded_at",
                table: "health_metrics");

            migrationBuilder.DropIndex(
                name: "IX_health_metrics_HealthMetricSubmissionId",
                table: "health_metrics");

            migrationBuilder.DropIndex(
                name: "IX_health_metrics_PatientId1",
                table: "health_metrics");

            migrationBuilder.DropIndex(
                name: "IX_health_metrics_recorded_by_user_id",
                table: "health_metrics");

            migrationBuilder.DropColumn(
                name: "HealthMetricSubmissionId",
                table: "health_metrics");

            migrationBuilder.DropColumn(
                name: "PatientId1",
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

            migrationBuilder.DropColumn(
                name: "status",
                table: "health_metrics");

            migrationBuilder.RenameColumn(
                name: "patient_id",
                table: "health_metrics",
                newName: "PatientId");

            migrationBuilder.RenameIndex(
                name: "idx_hm_patient_id",
                table: "health_metrics",
                newName: "IX_health_metrics_PatientId");

            migrationBuilder.AlterColumn<Guid>(
                name: "PatientId",
                table: "health_metrics",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

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
    }
}
