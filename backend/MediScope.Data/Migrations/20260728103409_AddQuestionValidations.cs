using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediScope.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddQuestionValidations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "max_length",
                table: "questionnaire_questions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "max_value",
                table: "questionnaire_questions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "min_length",
                table: "questionnaire_questions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "min_value",
                table: "questionnaire_questions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "regex_pattern",
                table: "questionnaire_questions",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "max_length",
                table: "questionnaire_questions");

            migrationBuilder.DropColumn(
                name: "max_value",
                table: "questionnaire_questions");

            migrationBuilder.DropColumn(
                name: "min_length",
                table: "questionnaire_questions");

            migrationBuilder.DropColumn(
                name: "min_value",
                table: "questionnaire_questions");

            migrationBuilder.DropColumn(
                name: "regex_pattern",
                table: "questionnaire_questions");
        }
    }
}
