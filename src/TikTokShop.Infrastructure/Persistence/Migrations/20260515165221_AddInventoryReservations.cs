using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TikTokShop.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddInventoryReservations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "inventory_reservations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    tik_tok_order_item_id = table.Column<Guid>(type: "uuid", nullable: true),
                    reserved_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    resolved_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    idempotency_key = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
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
                    table.PrimaryKey("pk_inventory_reservations", x => x.id);
                    table.CheckConstraint("ck_inventory_reservations_quantity_positive", "quantity > 0");
                });

            migrationBuilder.CreateIndex(
                name: "ix_inventory_reservations_tenant_id_expires_at",
                table: "inventory_reservations",
                columns: new[] { "tenant_id", "expires_at" },
                filter: "status = 1");

            migrationBuilder.CreateIndex(
                name: "ix_inventory_reservations_tenant_id_idempotency_key",
                table: "inventory_reservations",
                columns: new[] { "tenant_id", "idempotency_key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_inventory_reservations_tenant_id_product_id_status",
                table: "inventory_reservations",
                columns: new[] { "tenant_id", "product_id", "status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "inventory_reservations");
        }
    }
}
