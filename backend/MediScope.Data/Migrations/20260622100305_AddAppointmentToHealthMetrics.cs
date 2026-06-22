using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediScope.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAppointmentToHealthMetrics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "appointment_id",
                table: "health_metrics",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "idx_hm_appointment_id",
                table: "health_metrics",
                column: "appointment_id");

            migrationBuilder.AddForeignKey(
                name: "FK_health_metrics_appointments_appointment_id",
                table: "health_metrics",
                column: "appointment_id",
                principalTable: "appointments",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_health_metrics_appointments_appointment_id",
                table: "health_metrics");

            migrationBuilder.DropIndex(
                name: "idx_hm_appointment_id",
                table: "health_metrics");

            migrationBuilder.DropColumn(
                name: "appointment_id",
                table: "health_metrics");
        }
    }
}
