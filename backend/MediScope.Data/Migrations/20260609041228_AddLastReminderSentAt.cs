using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediScope.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLastReminderSentAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LastReminderSentAt",
                table: "doctor_patient",
                newName: "last_reminder_sent_at");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "last_reminder_sent_at",
                table: "doctor_patient",
                newName: "LastReminderSentAt");
        }
    }
}
