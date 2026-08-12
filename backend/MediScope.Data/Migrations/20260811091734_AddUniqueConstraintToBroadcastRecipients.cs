using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediScope.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueConstraintToBroadcastRecipients : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_broadcast_recipients_broadcast_id",
                table: "broadcast_recipients");

            migrationBuilder.CreateIndex(
                name: "uk_broadcast_user",
                table: "broadcast_recipients",
                columns: new[] { "broadcast_id", "user_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "uk_broadcast_user",
                table: "broadcast_recipients");

            migrationBuilder.CreateIndex(
                name: "IX_broadcast_recipients_broadcast_id",
                table: "broadcast_recipients",
                column: "broadcast_id");
        }
    }
}
