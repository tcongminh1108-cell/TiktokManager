using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TikTokShop.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTikTokFinanceAndVideos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tik_tok_finance_statements",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    connection_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tik_tok_statement_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    statement_time = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    sale_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    tik_tok_fee = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    shipping_fee = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    promotion_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    adjustment_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    settlement_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    statement_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    period_start = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    period_end = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("pk_tik_tok_finance_statements", x => x.id);
                    table.ForeignKey(
                        name: "fk_tik_tok_finance_statements_tik_tok_shop_connections_connect",
                        column: x => x.connection_id,
                        principalTable: "tik_tok_shop_connections",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tik_tok_videos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    connection_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tik_tok_video_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    thumbnail_url = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    video_url = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    video_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    published_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    view_count = table.Column<long>(type: "bigint", nullable: false),
                    like_count = table.Column<long>(type: "bigint", nullable: false),
                    share_count = table.Column<long>(type: "bigint", nullable: false),
                    comment_count = table.Column<long>(type: "bigint", nullable: false),
                    last_synced_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("pk_tik_tok_videos", x => x.id);
                    table.ForeignKey(
                        name: "fk_tik_tok_videos_tik_tok_shop_connections_connection_id",
                        column: x => x.connection_id,
                        principalTable: "tik_tok_shop_connections",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tik_tok_order_finances",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    connection_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tik_tok_finance_statement_id = table.Column<Guid>(type: "uuid", nullable: true),
                    tik_tok_order_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    sale_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    tik_tok_fee = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    shipping_fee = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    promotion_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    adjustment_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    net_revenue = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
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
                    table.PrimaryKey("pk_tik_tok_order_finances", x => x.id);
                    table.ForeignKey(
                        name: "fk_tik_tok_order_finances_tik_tok_finance_statements_tik_tok_f",
                        column: x => x.tik_tok_finance_statement_id,
                        principalTable: "tik_tok_finance_statements",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_tik_tok_order_finances_tik_tok_shop_connections_connection_",
                        column: x => x.connection_id,
                        principalTable: "tik_tok_shop_connections",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tik_tok_video_metrics",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tik_tok_video_id = table.Column<Guid>(type: "uuid", nullable: false),
                    view_count = table.Column<long>(type: "bigint", nullable: false),
                    like_count = table.Column<long>(type: "bigint", nullable: false),
                    share_count = table.Column<long>(type: "bigint", nullable: false),
                    comment_count = table.Column<long>(type: "bigint", nullable: false),
                    captured_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("pk_tik_tok_video_metrics", x => x.id);
                    table.ForeignKey(
                        name: "fk_tik_tok_video_metrics_tik_tok_videos_tik_tok_video_id",
                        column: x => x.tik_tok_video_id,
                        principalTable: "tik_tok_videos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_tik_tok_finance_statements_connection_id",
                table: "tik_tok_finance_statements",
                column: "connection_id");

            migrationBuilder.CreateIndex(
                name: "ix_tik_tok_finance_statements_tenant_id_connection_id_statemen",
                table: "tik_tok_finance_statements",
                columns: new[] { "tenant_id", "connection_id", "statement_time" });

            migrationBuilder.CreateIndex(
                name: "ix_tik_tok_finance_statements_tenant_id_tik_tok_statement_id",
                table: "tik_tok_finance_statements",
                columns: new[] { "tenant_id", "tik_tok_statement_id" },
                unique: true,
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_tik_tok_order_finances_connection_id",
                table: "tik_tok_order_finances",
                column: "connection_id");

            migrationBuilder.CreateIndex(
                name: "ix_tik_tok_order_finances_tenant_id_connection_id",
                table: "tik_tok_order_finances",
                columns: new[] { "tenant_id", "connection_id" });

            migrationBuilder.CreateIndex(
                name: "ix_tik_tok_order_finances_tenant_id_tik_tok_finance_statement_",
                table: "tik_tok_order_finances",
                columns: new[] { "tenant_id", "tik_tok_finance_statement_id" });

            migrationBuilder.CreateIndex(
                name: "ix_tik_tok_order_finances_tenant_id_tik_tok_order_id",
                table: "tik_tok_order_finances",
                columns: new[] { "tenant_id", "tik_tok_order_id" },
                unique: true,
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_tik_tok_order_finances_tik_tok_finance_statement_id",
                table: "tik_tok_order_finances",
                column: "tik_tok_finance_statement_id");

            migrationBuilder.CreateIndex(
                name: "ix_tik_tok_video_metrics_tenant_id_tik_tok_video_id_captured_at",
                table: "tik_tok_video_metrics",
                columns: new[] { "tenant_id", "tik_tok_video_id", "captured_at" });

            migrationBuilder.CreateIndex(
                name: "ix_tik_tok_video_metrics_tik_tok_video_id_captured_at",
                table: "tik_tok_video_metrics",
                columns: new[] { "tik_tok_video_id", "captured_at" });

            migrationBuilder.CreateIndex(
                name: "ix_tik_tok_videos_connection_id",
                table: "tik_tok_videos",
                column: "connection_id");

            migrationBuilder.CreateIndex(
                name: "ix_tik_tok_videos_tenant_id_connection_id_tik_tok_video_id",
                table: "tik_tok_videos",
                columns: new[] { "tenant_id", "connection_id", "tik_tok_video_id" },
                unique: true,
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_tik_tok_videos_tenant_id_connection_id_view_count",
                table: "tik_tok_videos",
                columns: new[] { "tenant_id", "connection_id", "view_count" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tik_tok_order_finances");

            migrationBuilder.DropTable(
                name: "tik_tok_video_metrics");

            migrationBuilder.DropTable(
                name: "tik_tok_finance_statements");

            migrationBuilder.DropTable(
                name: "tik_tok_videos");
        }
    }
}
