using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TikTokShop.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTikTokShopConnection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tik_tok_shop_connections",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    shop_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    shop_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    shop_cipher = table.Column<string>(type: "text", nullable: false),
                    region = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    base_api_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    access_token = table.Column<string>(type: "text", nullable: false),
                    refresh_token = table.Column<string>(type: "text", nullable: false),
                    token_expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    last_synced_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_webhook_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("pk_tik_tok_shop_connections", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "webhook_events",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    connection_id = table.Column<Guid>(type: "uuid", nullable: true),
                    event_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    event_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    payload = table.Column<string>(type: "text", nullable: false),
                    source = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "webhook"),
                    received_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    processed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    retry_count = table.Column<int>(type: "integer", nullable: false),
                    last_error = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("pk_webhook_events", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_tik_tok_shop_connections_tenant_id_shop_id",
                table: "tik_tok_shop_connections",
                columns: new[] { "tenant_id", "shop_id" },
                unique: true,
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_tik_tok_shop_connections_tenant_id_status",
                table: "tik_tok_shop_connections",
                columns: new[] { "tenant_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_webhook_events_tenant_id_connection_id_received_at",
                table: "webhook_events",
                columns: new[] { "tenant_id", "connection_id", "received_at" });

            migrationBuilder.CreateIndex(
                name: "ix_webhook_events_tenant_id_event_id",
                table: "webhook_events",
                columns: new[] { "tenant_id", "event_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_webhook_events_tenant_id_status_received_at",
                table: "webhook_events",
                columns: new[] { "tenant_id", "status", "received_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tik_tok_shop_connections");

            migrationBuilder.DropTable(
                name: "webhook_events");
        }
    }
}
