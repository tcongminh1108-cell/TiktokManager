using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TikTokShop.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddStockMovements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "stock_movements",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    source = table.Column<int>(type: "integer", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    unit_cost = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    occurred_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    stock_in_id = table.Column<Guid>(type: "uuid", nullable: true),
                    stock_out_id = table.Column<Guid>(type: "uuid", nullable: true),
                    tik_tok_order_item_id = table.Column<Guid>(type: "uuid", nullable: true),
                    tik_tok_return_line_id = table.Column<Guid>(type: "uuid", nullable: true),
                    idempotency_key = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    note = table.Column<string>(type: "text", nullable: true),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_stock_movements", x => x.id);
                    table.CheckConstraint("ck_stock_movements_quantity_positive", "quantity > 0");
                });

            migrationBuilder.CreateIndex(
                name: "ix_stock_movements_tenant_id_idempotency_key",
                table: "stock_movements",
                columns: new[] { "tenant_id", "idempotency_key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_stock_movements_tenant_id_product_id_occurred_at",
                table: "stock_movements",
                columns: new[] { "tenant_id", "product_id", "occurred_at" });

            migrationBuilder.CreateIndex(
                name: "ix_stock_movements_tenant_id_source_occurred_at",
                table: "stock_movements",
                columns: new[] { "tenant_id", "source", "occurred_at" });

            migrationBuilder.CreateIndex(
                name: "ix_stock_movements_tik_tok_order_item_id",
                table: "stock_movements",
                column: "tik_tok_order_item_id",
                filter: "tik_tok_order_item_id IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "stock_movements");
        }
    }
}
