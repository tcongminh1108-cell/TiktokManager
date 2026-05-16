using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TikTokShop.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddStockIn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "stock_ins",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    supplier_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    unit_price = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    total_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    transaction_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    note = table.Column<string>(type: "text", nullable: true),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_stock_ins", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_stock_movements_stock_in_id",
                table: "stock_movements",
                column: "stock_in_id");

            migrationBuilder.CreateIndex(
                name: "ix_stock_ins_tenant_id_product_id",
                table: "stock_ins",
                columns: new[] { "tenant_id", "product_id" });

            migrationBuilder.CreateIndex(
                name: "ix_stock_ins_tenant_id_supplier_id",
                table: "stock_ins",
                columns: new[] { "tenant_id", "supplier_id" });

            migrationBuilder.CreateIndex(
                name: "ix_stock_ins_tenant_id_transaction_date",
                table: "stock_ins",
                columns: new[] { "tenant_id", "transaction_date" });

            migrationBuilder.AddForeignKey(
                name: "fk_stock_movements_stock_ins_stock_in_id",
                table: "stock_movements",
                column: "stock_in_id",
                principalTable: "stock_ins",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_stock_movements_stock_ins_stock_in_id",
                table: "stock_movements");

            migrationBuilder.DropTable(
                name: "stock_ins");

            migrationBuilder.DropIndex(
                name: "ix_stock_movements_stock_in_id",
                table: "stock_movements");
        }
    }
}
