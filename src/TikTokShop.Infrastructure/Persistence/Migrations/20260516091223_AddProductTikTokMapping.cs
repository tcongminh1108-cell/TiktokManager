using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TikTokShop.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProductTikTokMapping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "product_tik_tok_mappings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    connection_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tik_tok_product_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    tik_tok_sku_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    tik_tok_sku_name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
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
                    table.PrimaryKey("pk_product_tik_tok_mappings", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_product_tik_tok_mappings_tenant_id_connection_id_tik_tok_pr",
                table: "product_tik_tok_mappings",
                columns: new[] { "tenant_id", "connection_id", "tik_tok_product_id", "tik_tok_sku_id" },
                unique: true,
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_product_tik_tok_mappings_tenant_id_connection_id_tik_tok_sk",
                table: "product_tik_tok_mappings",
                columns: new[] { "tenant_id", "connection_id", "tik_tok_sku_id" });

            migrationBuilder.CreateIndex(
                name: "ix_product_tik_tok_mappings_tenant_id_product_id",
                table: "product_tik_tok_mappings",
                columns: new[] { "tenant_id", "product_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "product_tik_tok_mappings");
        }
    }
}
