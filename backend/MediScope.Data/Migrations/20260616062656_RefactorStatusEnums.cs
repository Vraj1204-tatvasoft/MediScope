using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediScope.Data.Migrations
{
    /// <inheritdoc />
    public partial class RefactorStatusEnums : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_health_metrics_patients_PatientId1",
                table: "health_metrics");

            migrationBuilder.DropIndex(
                name: "IX_health_metrics_PatientId1",
                table: "health_metrics");

            migrationBuilder.DropColumn(
                name: "PatientId1",
                table: "health_metrics");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PatientId1",
                table: "health_metrics",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_health_metrics_PatientId1",
                table: "health_metrics",
                column: "PatientId1");

            migrationBuilder.AddForeignKey(
                name: "FK_health_metrics_patients_PatientId1",
                table: "health_metrics",
                column: "PatientId1",
                principalTable: "patients",
                principalColumn: "id");
        }
    }
}
