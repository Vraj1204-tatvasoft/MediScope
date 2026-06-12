using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediScope.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDoctorContactUniqueConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "contact_number",
                table: "doctors",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_doctors_contact_number",
                table: "doctors",
                column: "contact_number",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_doctors_contact_number",
                table: "doctors");

            migrationBuilder.AlterColumn<string>(
                name: "contact_number",
                table: "doctors",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10);
        }
    }
}
