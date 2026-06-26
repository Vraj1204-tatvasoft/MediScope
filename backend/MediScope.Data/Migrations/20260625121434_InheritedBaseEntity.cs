using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediScope.Data.Migrations
{
    /// <inheritdoc />
    public partial class InheritedBaseEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                table: "payments",
                type: "uuid",
                nullable: false,
                defaultValueSql: "gen_random_uuid()",
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "payments",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()");

            migrationBuilder.AddColumn<Guid>(
                name: "created_by",
                table: "payments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "payments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "deleted_by",
                table: "payments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "payments",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "payments",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()");

            migrationBuilder.AddColumn<Guid>(
                name: "updated_by",
                table: "payments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at",
                table: "invoices",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "invoices",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                table: "invoices",
                type: "uuid",
                nullable: false,
                defaultValueSql: "gen_random_uuid()",
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "created_by",
                table: "invoices",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "invoices",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "deleted_by",
                table: "invoices",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "invoices",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "updated_by",
                table: "invoices",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                table: "invoice_items",
                type: "uuid",
                nullable: false,
                defaultValueSql: "gen_random_uuid()",
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "invoice_items",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()");

            migrationBuilder.AddColumn<Guid>(
                name: "created_by",
                table: "invoice_items",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "invoice_items",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "deleted_by",
                table: "invoice_items",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "invoice_items",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "invoice_items",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()");

            migrationBuilder.AddColumn<Guid>(
                name: "updated_by",
                table: "invoice_items",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "created_at",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "created_by",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "deleted_by",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "updated_by",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "created_by",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "deleted_by",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "updated_by",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "invoice_items");

            migrationBuilder.DropColumn(
                name: "created_by",
                table: "invoice_items");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "invoice_items");

            migrationBuilder.DropColumn(
                name: "deleted_by",
                table: "invoice_items");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "invoice_items");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "invoice_items");

            migrationBuilder.DropColumn(
                name: "updated_by",
                table: "invoice_items");

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                table: "payments",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValueSql: "gen_random_uuid()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at",
                table: "invoices",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "now()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "invoices",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "now()");

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                table: "invoices",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValueSql: "gen_random_uuid()");

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                table: "invoice_items",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValueSql: "gen_random_uuid()");
        }
    }
}
