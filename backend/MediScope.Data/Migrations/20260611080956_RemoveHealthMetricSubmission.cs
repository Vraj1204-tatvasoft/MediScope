using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediScope.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveHealthMetricSubmission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_health_metrics_health_metric_submissions_HealthMetricSubmis~",
                table: "health_metrics");

            migrationBuilder.DropTable(
                name: "health_metric_submissions");

            migrationBuilder.DropIndex(
                name: "IX_health_metrics_HealthMetricSubmissionId",
                table: "health_metrics");

            migrationBuilder.DropColumn(
                name: "HealthMetricSubmissionId",
                table: "health_metrics");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "HealthMetricSubmissionId",
                table: "health_metrics",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "health_metric_submissions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    patient_id = table.Column<Guid>(type: "uuid", nullable: false),
                    recorded_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    notes = table.Column<string>(type: "text", nullable: true),
                    recorded_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    recorded_by_role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true)
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
                name: "IX_health_metrics_HealthMetricSubmissionId",
                table: "health_metrics",
                column: "HealthMetricSubmissionId");

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
                name: "FK_health_metrics_health_metric_submissions_HealthMetricSubmis~",
                table: "health_metrics",
                column: "HealthMetricSubmissionId",
                principalTable: "health_metric_submissions",
                principalColumn: "id");
        }
    }
}
