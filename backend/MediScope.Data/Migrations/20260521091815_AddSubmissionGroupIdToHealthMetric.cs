using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediScope.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSubmissionGroupIdToHealthMetric : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "submission_group_id",
                table: "health_metrics",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "idx_hm_patient_submission",
                table: "health_metrics",
                columns: new[] { "patient_id", "submission_group_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_hm_patient_submission",
                table: "health_metrics");

            migrationBuilder.DropColumn(
                name: "submission_group_id",
                table: "health_metrics");
        }
    }
}
