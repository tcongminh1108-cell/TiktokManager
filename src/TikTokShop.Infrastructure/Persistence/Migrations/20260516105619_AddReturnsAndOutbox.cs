using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TikTokShop.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddReturnsAndOutbox : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "outbox_messages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    payload = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    retry_count = table.Column<int>(type: "integer", nullable: false),
                    next_attempt_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    processed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_error = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
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
                    table.PrimaryKey("pk_outbox_messages", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tik_tok_returns",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    connection_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tik_tok_return_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    tik_tok_order_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    return_status = table.Column<int>(type: "integer", nullable: false),
                    return_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    requested_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    approved_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    received_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    refunded_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    refund_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
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
                    table.PrimaryKey("pk_tik_tok_returns", x => x.id);
                    table.ForeignKey(
                        name: "fk_tik_tok_returns_tik_tok_shop_connections_connection_id",
                        column: x => x.connection_id,
                        principalTable: "tik_tok_shop_connections",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tik_tok_return_lines",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tik_tok_return_id = table.Column<Guid>(type: "uuid", nullable: false),
                    line_item_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    original_order_item_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    tik_tok_sku_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    sku_name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    refund_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: true),
                    mapping_id = table.Column<Guid>(type: "uuid", nullable: true),
                    sync_status = table.Column<int>(type: "integer", nullable: false),
                    last_error = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("pk_tik_tok_return_lines", x => x.id);
                    table.CheckConstraint("ck_tik_tok_return_lines_quantity_positive", "quantity > 0");
                    table.ForeignKey(
                        name: "fk_tik_tok_return_lines_tik_tok_returns_tik_tok_return_id",
                        column: x => x.tik_tok_return_id,
                        principalTable: "tik_tok_returns",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_status_next_attempt_at_created_at",
                table: "outbox_messages",
                columns: new[] { "status", "next_attempt_at", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_tenant_id_status_created_at",
                table: "outbox_messages",
                columns: new[] { "tenant_id", "status", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_tik_tok_return_lines_tenant_id_line_item_id",
                table: "tik_tok_return_lines",
                columns: new[] { "tenant_id", "line_item_id" },
                unique: true,
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_tik_tok_return_lines_tenant_id_sync_status",
                table: "tik_tok_return_lines",
                columns: new[] { "tenant_id", "sync_status" });

            migrationBuilder.CreateIndex(
                name: "ix_tik_tok_return_lines_tik_tok_return_id",
                table: "tik_tok_return_lines",
                column: "tik_tok_return_id");

            migrationBuilder.CreateIndex(
                name: "ix_tik_tok_returns_connection_id",
                table: "tik_tok_returns",
                column: "connection_id");

            migrationBuilder.CreateIndex(
                name: "ix_tik_tok_returns_tenant_id_connection_id_return_status",
                table: "tik_tok_returns",
                columns: new[] { "tenant_id", "connection_id", "return_status" });

            migrationBuilder.CreateIndex(
                name: "ix_tik_tok_returns_tenant_id_tik_tok_order_id",
                table: "tik_tok_returns",
                columns: new[] { "tenant_id", "tik_tok_order_id" });

            migrationBuilder.CreateIndex(
                name: "ix_tik_tok_returns_tenant_id_tik_tok_return_id",
                table: "tik_tok_returns",
                columns: new[] { "tenant_id", "tik_tok_return_id" },
                unique: true,
                filter: "is_deleted = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "outbox_messages");

            migrationBuilder.DropTable(
                name: "tik_tok_return_lines");

            migrationBuilder.DropTable(
                name: "tik_tok_returns");
        }
    }
}
