using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediScope.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChangeBillingItemName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_invoice_items_billing_items_BillingItemId",
                table: "invoice_items");

            migrationBuilder.RenameColumn(
                name: "BillingItemId",
                table: "invoice_items",
                newName: "billing_item_id");

            migrationBuilder.RenameIndex(
                name: "IX_invoice_items_BillingItemId",
                table: "invoice_items",
                newName: "IX_invoice_items_billing_item_id");

            migrationBuilder.AddForeignKey(
                name: "FK_invoice_items_billing_items_billing_item_id",
                table: "invoice_items",
                column: "billing_item_id",
                principalTable: "billing_items",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_invoice_items_billing_items_billing_item_id",
                table: "invoice_items");

            migrationBuilder.RenameColumn(
                name: "billing_item_id",
                table: "invoice_items",
                newName: "BillingItemId");

            migrationBuilder.RenameIndex(
                name: "IX_invoice_items_billing_item_id",
                table: "invoice_items",
                newName: "IX_invoice_items_BillingItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_invoice_items_billing_items_BillingItemId",
                table: "invoice_items",
                column: "BillingItemId",
                principalTable: "billing_items",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
