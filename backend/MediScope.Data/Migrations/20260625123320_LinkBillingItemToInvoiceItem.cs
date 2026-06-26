using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediScope.Data.Migrations
{
    /// <inheritdoc />
    public partial class LinkBillingItemToInvoiceItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BillingItemId",
                table: "invoice_items",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "billing_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    item_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    default_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    is_taxable = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_billing_items", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_invoice_items_BillingItemId",
                table: "invoice_items",
                column: "BillingItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_invoice_items_billing_items_BillingItemId",
                table: "invoice_items",
                column: "BillingItemId",
                principalTable: "billing_items",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_invoice_items_billing_items_BillingItemId",
                table: "invoice_items");

            migrationBuilder.DropTable(
                name: "billing_items");

            migrationBuilder.DropIndex(
                name: "IX_invoice_items_BillingItemId",
                table: "invoice_items");

            migrationBuilder.DropColumn(
                name: "BillingItemId",
                table: "invoice_items");
        }
    }
}
