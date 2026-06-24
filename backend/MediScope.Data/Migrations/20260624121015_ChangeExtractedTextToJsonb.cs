using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediScope.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChangeExtractedTextToJsonb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Bypass EF Core and give Postgres the explicit cast command it requires
            migrationBuilder.Sql("ALTER TABLE medical_documents ALTER COLUMN extracted_text TYPE jsonb USING extracted_text::jsonb;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // If we ever need to undo this migration, cast it back to text safely
            migrationBuilder.Sql("ALTER TABLE medical_documents ALTER COLUMN extracted_text TYPE text USING extracted_text::text;");
        }
    }
}