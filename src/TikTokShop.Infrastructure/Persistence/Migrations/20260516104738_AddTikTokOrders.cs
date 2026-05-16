using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TikTokShop.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTikTokOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tik_tok_orders",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    connection_id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    status_code = table.Column<int>(type: "integer", nullable: false),
                    buyer_username = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    tik_tok_created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    tik_tok_updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    raw_data = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("pk_tik_tok_orders", x => x.id);
                    table.ForeignKey(
                        name: "fk_tik_tok_orders_tik_tok_shop_connections_connection_id",
                        column: x => x.connection_id,
                        principalTable: "tik_tok_shop_connections",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tik_tok_order_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tik_tok_order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    line_item_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    tik_tok_product_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    tik_tok_sku_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    sku_name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    sale_price = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: true),
                    mapping_id = table.Column<Guid>(type: "uuid", nullable: true),
                    sync_status = table.Column<int>(type: "integer", nullable: false),
                    last_error = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    reservation_key = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    movement_key = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
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
                    table.PrimaryKey("pk_tik_tok_order_items", x => x.id);
                    table.CheckConstraint("ck_tik_tok_order_items_quantity_positive", "quantity > 0");
                    table.ForeignKey(
                        name: "fk_tik_tok_order_items_tik_tok_orders_tik_tok_order_id",
                        column: x => x.tik_tok_order_id,
                        principalTable: "tik_tok_orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_tik_tok_order_items_tenant_id_line_item_id",
                table: "tik_tok_order_items",
                columns: new[] { "tenant_id", "line_item_id" },
                unique: true,
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_tik_tok_order_items_tenant_id_sync_status",
                table: "tik_tok_order_items",
                columns: new[] { "tenant_id", "sync_status" });

            migrationBuilder.CreateIndex(
                name: "ix_tik_tok_order_items_tenant_id_tik_tok_sku_id",
                table: "tik_tok_order_items",
                columns: new[] { "tenant_id", "tik_tok_sku_id" });

            migrationBuilder.CreateIndex(
                name: "ix_tik_tok_order_items_tik_tok_order_id",
                table: "tik_tok_order_items",
                column: "tik_tok_order_id");

            migrationBuilder.CreateIndex(
                name: "ix_tik_tok_orders_connection_id",
                table: "tik_tok_orders",
                column: "connection_id");

            migrationBuilder.CreateIndex(
                name: "ix_tik_tok_orders_tenant_id_connection_id_status_code",
                table: "tik_tok_orders",
                columns: new[] { "tenant_id", "connection_id", "status_code" });

            migrationBuilder.CreateIndex(
                name: "ix_tik_tok_orders_tenant_id_order_id",
                table: "tik_tok_orders",
                columns: new[] { "tenant_id", "order_id" },
                unique: true,
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_tik_tok_orders_tenant_id_tik_tok_updated_at",
                table: "tik_tok_orders",
                columns: new[] { "tenant_id", "tik_tok_updated_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tik_tok_order_items");

            migrationBuilder.DropTable(
                name: "tik_tok_orders");
        }
    }
}
