# Plan.md — TikTok Shop Management System

> Tài liệu này mô tả trạng thái hiện tại của toàn bộ hệ thống (BE + FE), liệt kê các lỗi cần sửa so với TikTok API thực tế (đối chiếu từ `Postman/API 2.0.postman_collection.json` và `Postman/Test Environment.postman_environment.json`), và lộ trình triển khai tiếp theo.

---

## 1. TỔNG QUAN KIẾN TRÚC

| Layer | Công nghệ | Trạng thái |
|-------|-----------|-----------|
| Backend | ASP.NET Core Web API (.NET 9) | ✅ Đã có |
| ORM | EF Core 9 + PostgreSQL 16 | ✅ Đã có |
| Frontend | React 18 + TypeScript + Ant Design + Vite | 🟡 Có khung, thiếu trang TikTok |
| Auth | JWT (access + refresh token) | ✅ Đã có |
| TikTok API | Open API v202309 | 🔴 Có bugs nghiêm trọng |
| Containerization | Docker + docker-compose | ✅ Đã có |
| Background Jobs | IHostedService | ✅ Đã có |

---

## 2. TRẠNG THÁI HIỆN TẠI — BACKEND

### 2.1 Domain Layer (`TikTokShop.Domain`)

**Entities đã có (20 entities):**
- `BaseEntity` — Id (Guid), TenantId, CreatedBy, CreatedAt, UpdatedBy, UpdatedAt, IsDeleted, DeletedAt, DeletedBy ✅
- `Tenant`, `User`, `RefreshToken` ✅
- `Product`, `Supplier` ✅
- `StockIn`, `StockOut`, `StockMovement` (unified model, append-only) ✅
- `InventoryReservation` ✅
- `TikTokShopConnection` — ShopId, ShopName, ShopCipher (encrypted), Region, BaseApiUrl, AccessToken (encrypted), RefreshToken (encrypted), TokenExpiresAt ✅
- `ProductTikTokMapping` ✅
- `TikTokOrder`, `TikTokOrderItem`, `TikTokOrderFinance` ✅
- `TikTokReturn`, `TikTokReturnLine` ✅
- `TikTokFinanceStatement` ✅
- `TikTokVideo`, `TikTokVideoMetric` ✅
- `OutboxMessage` ✅
- `WebhookEvent` ✅

**Enums đã có (11 enums, mỗi file riêng):**
`UserRole`, `TenantStatus`, `StockMovementType`, `StockMovementSource`,
`InventoryReservationStatus`, `TikTokShopConnectionStatus`,
`TikTokOrderStatus`, `TikTokOrderSyncStatus`, `TikTokReturnStatus`,
`OutboxStatus`, `WebhookEventStatus` ✅

**Exceptions:** `AppException` hierarchy đầy đủ (NotFoundException, ValidationException, ForbiddenException, UnauthorizedException, ConflictException, BusinessRuleException) ✅

---

### 2.2 Application Layer (`TikTokShop.Application`)

**Services đã triển khai:**

| Feature | Service | Trạng thái |
|---------|---------|-----------|
| Auth | `AuthService` | ✅ |
| Users | `UserService` | ✅ |
| Products | `ProductService` | ✅ |
| Suppliers | `SupplierService` | ✅ |
| StockIns | `StockInService` | ✅ |
| StockOuts | `StockOutService` | ✅ |
| Inventory | `InventoryService` (PhysicalStock, ReservedQty, AvailableStock) | ✅ |
| Reservations | `ReservationService` | ✅ |
| StockMovements | `StockMovementService` | ✅ |
| Dashboard nội bộ | `DashboardService` | ✅ |
| Dashboard TikTok | `TikTokDashboardService` | ✅ |
| TikTok Connections | `TikTokConnectionService` | 🔴 Có bugs auth |
| TikTok Orders | `TikTokOrderService` + `OrderEventHandler` | ✅ |
| TikTok Returns | `TikTokReturnService` + `ReturnEventHandler` | ✅ |
| TikTok Finance | `TikTokFinanceService` | ✅ |
| Product Mappings | `ProductMappingService` | ✅ |
| TikTok Videos | `TikTokVideoService` | ✅ |
| Webhooks | `WebhookEventService` | ✅ |
| Dev Seed | `DevSeedService` | ✅ |

**Interfaces đã định nghĩa:** `ITikTokApiClient`, `IJwtService`, `IPasswordHasher`, `IOAuthStateCache`, `IOutboxService`, `ITikTokTokenProtector`, `ITikTokWebhookSignatureVerifier` ✅

---

### 2.3 Infrastructure Layer (`TikTokShop.Infrastructure`)

**Database:**
- `ApplicationDbContext` với Global Query Filter (TenantId + IsDeleted) ✅
- `AuditableEntityInterceptor` — tự gán TenantId, CreatedBy, UpdatedBy ✅
- 13 EF Core Migrations đã apply ✅
- 21 `IEntityTypeConfiguration<T>` ✅

**Identity:** `JwtService`, `PasswordHasher` (BCrypt), `CurrentUser` ✅

**TikTok External Services:**
- `TikTokApiClient` — Polly retry/circuit-breaker, per-shop rate limiter ✅
- `TikTokRateLimiter` (token bucket per shop) ✅
- `TikTokTokenProtector` (ASP.NET Data Protection) ✅
- `TikTokWebhookSignatureVerifier` ✅
- `MemoryOAuthStateCache` ✅

**Background Services (7 jobs):**
- `OutboxDispatcherService` — push inventory lên TikTok ✅
- `WebhookProcessorService` — xử lý webhook events async ✅
- `TikTokTokenRefreshService` — auto-refresh token trước hạn ✅
- `OrderReconciliationService` — polling TikTok orders ✅
- `ReservationExpiryService` — auto-expire reservations ✅
- `FinanceSyncService` — sync finance statements ✅
- `VideoSyncService` — sync TikTok videos ✅

---

### 2.4 API Layer (`TikTokShop.Api`)

**Controllers đã có (18 controllers):**

| Controller | Endpoints chính | Trạng thái |
|-----------|----------------|-----------|
| `AuthController` | login, refresh, logout, register, me | ✅ |
| `UsersController` | CRUD + change-password | ✅ |
| `ProductsController` | CRUD + pagination + search | ✅ |
| `SuppliersController` | CRUD | ✅ |
| `StockInsController` | CRUD | ✅ |
| `StockOutsController` | CRUD | ✅ |
| `InventoryController` | list, detail, movements, reservations | ✅ |
| `DashboardController` | overview, revenue, profit | ✅ |
| `TikTokShopsController` | auth-url, callback, list, delete, refresh-token | 🔴 Có bugs |
| `ProductMappingsController` | CRUD mapping, list TikTok SKUs | ✅ |
| `TikTokOrdersController` | list, detail, manual sync | ✅ |
| `TikTokReturnsController` | list, detail | ✅ |
| `TikTokFinanceController` | statements, transactions | ✅ |
| `TikTokVideosController` | list, sync | ✅ |
| `WebhooksController` | POST /api/webhooks/tiktok | ✅ |
| `WebhookEventsController` | list webhook events | ✅ |
| `DevController` | seed, dev tools | ✅ |
| `TestController` | health check | ✅ |

**Middlewares:** ExceptionHandlingMiddleware, CorrelationIdMiddleware, UserContextEnrichmentMiddleware ✅

---

## 3. TRẠNG THÁI HIỆN TẠI — FRONTEND

### 3.1 Trang và component đã có

| Feature | Page / Component | Trạng thái |
|---------|-----------------|-----------|
| Auth | `LoginPage`, `RegisterTenantPage` | ✅ |
| Dashboard | `DashboardPage` (biểu đồ nội bộ) | ✅ |
| Products | `ProductListPage`, `ProductFormModal` | ✅ |
| Suppliers | `SupplierListPage`, `SupplierFormModal` | ✅ |
| Stock In | `StockInListPage`, `StockInFormModal` | ✅ |
| Stock Out | `StockOutListPage`, `StockOutFormModal` | ✅ |
| Inventory | `InventoryPage`, `InventoryDetailDrawer` | ✅ |
| Users | `UserListPage`, `UserFormModal`, `ChangePasswordModal` | ✅ |
| Shared | `DataTable`, `ProductSelect`, `SupplierSelect` | ✅ |

**State management:** Zustand (auth store), TanStack Query (server state), React Router v6 ✅

### 3.2 Trang CHƯA có (TikTok-related)

| Feature | Trang cần tạo | Mức ưu tiên |
|---------|--------------|-------------|
| TikTok Shops | Quản lý kết nối, nút Connect Shop, trạng thái token | 🔴 Cao |
| Product Mappings | Map sản phẩm nội bộ ↔ TikTok SKU | 🔴 Cao |
| TikTok Orders | Danh sách đơn, filter theo status/shop, detail | 🔴 Cao |
| TikTok Returns | Danh sách yêu cầu hoàn trả, detail | 🟡 Trung |
| TikTok Finance | Statements, transactions, settlement | 🟡 Trung |
| TikTok Dashboard | Doanh thu TikTok vs Manual, top sản phẩm | 🟡 Trung |
| TikTok Videos | Danh sách video + metrics | 🟢 Thấp |
| Webhook Events | Log webhook, trạng thái xử lý | 🟢 Thấp |

---

## 4. LỖI NGHIÊM TRỌNG — TIKTOK API CLIENT

> **PHẢI sửa các bugs này TRƯỚC KHI test với TikTok API thực tế.** Tất cả đều gây thất bại 100%.

---

### BUG 1 — Auth: Sai URL base, sai HTTP method, sai tên param, dư sign

**Files:** `TikTokApiClient.cs`, `TikTokSettings.cs`, `appsettings.json`

**So sánh:**

| Mục | Code hiện tại | Đúng theo Postman |
|-----|--------------|-------------------|
| `AuthBaseUrl` (config) | `https://services.tiktokshop.com` | `https://auth.tiktok-shops.com` |
| Base URL dùng trong code | `_settings.ApiBaseUrl` (shop API) | Phải dùng `_settings.AuthBaseUrl` |
| HTTP method | `PostAsync` | `GET` |
| Tên param code | `code` | `auth_code` |
| Sign | Có (gọi `ComputeSign`) | **Không cần sign** |

**Postman chuẩn:**
```
GET https://auth.tiktok-shops.com/api/v2/token/get
  ?app_key={{app_key}}&app_secret={{app_secret}}&auth_code={{auth_code}}&grant_type=authorized_code

GET https://auth.tiktok-shops.com/api/v2/token/refresh
  ?app_key={{app_key}}&app_secret={{app_secret}}&refresh_token={{refresh_token}}&grant_type=refresh_token
```

**Lưu ý:** OAuth redirect URL dùng `https://services.tiktokshop.com/open/authorize` → `AuthBaseUrl` trong config vẫn là `https://services.tiktokshop.com`. Cần tách `AuthBaseUrl` (OAuth page) và `TokenBaseUrl` (API exchange) riêng.

**Fix gợi ý:**
```csharp
// Thêm field mới trong TikTokSettings:
public string TokenBaseUrl { get; set; } = "https://auth.tiktok-shops.com";

// ExchangeCodeAsync:
var url = $"{_settings.TokenBaseUrl}/api/v2/token/get" +
          $"?app_key={_settings.AppKey}&app_secret={_settings.AppSecret}" +
          $"&auth_code={code}&grant_type=authorized_code";
using var resp = await _http.GetAsync(url, ct);

// RefreshTokenAsync:
var url = $"{_settings.TokenBaseUrl}/api/v2/token/refresh" +
          $"?app_key={_settings.AppKey}&app_secret={_settings.AppSecret}" +
          $"&refresh_token={refreshToken}&grant_type=refresh_token";
using var resp = await _http.GetAsync(url, ct);
```

---

### BUG 2 — Signing Algorithm thiếu request body

**File:** `TikTokApiClient.cs` → method `ComputeSign` (line ~415) và `BuildSignedPostRequest`

**Công thức ký của TikTok (từ Postman pre-request script):**
```javascript
signstring = secret + path + sorted_query_params_KV_concat + request_body_string + secret
sign = HMAC-SHA256(secret, signstring)
```

**Code hiện tại (sai — thiếu body):**
```csharp
sb.Append(_settings.AppSecret);
sb.Append(path);
foreach (var kv in sortedParams) { sb.Append(kv.Key); sb.Append(kv.Value); }
sb.Append(_settings.AppSecret);  // ← body bị bỏ qua
```

**Hệ quả:** Tất cả POST request có body sẽ có chữ ký sai → TikTok trả lỗi `invalid_signature`.  
Ảnh hưởng đến: `GetOrdersAsync` (body: `{page_size, filter}`), `UpdateInventoryAsync` (body: `{skus}`), mọi POST khác.

**Fix:**
```csharp
private string ComputeSign(string path, SortedDictionary<string, string> sortedParams, string requestBody = "")
{
    var sb = new StringBuilder();
    sb.Append(_settings.AppSecret);
    sb.Append(path);
    foreach (var kv in sortedParams)
    {
        if (kv.Key is "sign" or "access_token") continue;
        sb.Append(kv.Key);
        sb.Append(kv.Value);
    }
    sb.Append(requestBody);  // ← thêm body
    sb.Append(_settings.AppSecret);
    var hash = HMACSHA256.HashData(
        Encoding.UTF8.GetBytes(_settings.AppSecret),
        Encoding.UTF8.GetBytes(sb.ToString()));
    return Convert.ToHexString(hash).ToLowerInvariant();
}
```

Cần refactor `BuildSignedPostRequest` để serialize body thành JSON string trước, tính sign với string đó, rồi mới tạo `HttpContent`.

**Lưu ý:** GET request không có body → truyền `""` (string rỗng) → kết quả giống code cũ → GET requests không bị ảnh hưởng.

---

### BUG 3 — Order Detail: sai endpoint

**File:** `TikTokApiClient.cs` → `GetOrderDetailRawAsync`

| Mục | Code hiện tại | Đúng (Postman "Get Order Details 2.0") |
|-----|--------------|----------------------------------------|
| Path | `/order/202309/orders/detail` | `/order/202309/orders` |
| Cách truyền orderId | `order_id_list` trong body (POST-style) | `ids` trong query string (GET) |

**Postman URL chuẩn:**
```
GET https://open-api.tiktokglobalshop.com/order/202309/orders
  ?app_key=...&shop_cipher=...&sign=...&timestamp=...&ids=["576717916890501341"]
```

**Fix:**
```csharp
var path = "/order/202309/orders";
var (_, baseParams) = BuildShopUrl(ctx, path);
baseParams["ids"] = $"[\"{orderId}\"]";
baseParams["sign"] = ComputeSign(path, baseParams, "");  // GET, body rỗng
var fullUrl = BuildUrl(ctx.BaseApiUrl, path, baseParams);
using var req = new HttpRequestMessage(HttpMethod.Get, fullUrl);
req.Headers.Add("x-tts-access-token", ctx.AccessToken);
```

---

### BUG 4 — Inventory Update: sai endpoint path và body format

**File:** `TikTokApiClient.cs` → `UpdateInventoryAsync`  
**File:** `ITikTokApiClient.cs`  
**File:** `ProductTikTokMapping.cs` (cần thêm fields)

| Mục | Code hiện tại | Đúng (Postman "Update Inventory 2.0") |
|-----|--------------|----------------------------------------|
| Path | `/product/202309/inventory/update` | `/product/202309/products/{productId}/inventory/update` |
| Body key array | `sku_list` | `skus` |
| Body SKU id field | `sku_id` | `id` |
| Body inventory field | `warehouse_type` | `warehouse_id` |

**Postman URL + Body chuẩn:**
```
POST /product/202309/products/{productId}/inventory/update

{
  "skus": [
    {
      "id": "1729465787539296266",
      "inventory": [
        {
          "quantity": 10,
          "warehouse_id": "7333596113700538155"
        }
      ]
    }
  ]
}
```

**Hệ quả:** Outbox pattern push inventory **không hoạt động** hoàn toàn.

**Fix interface:**
```csharp
// ITikTokApiClient.cs — thêm tikTokProductId và warehouseId
Task UpdateInventoryAsync(TikTokApiContext ctx, string tikTokProductId,
    string tikTokSkuId, int quantity, string warehouseId, CancellationToken ct);
```

**Fix entity `ProductTikTokMapping`:**
```csharp
/// <summary>ID sản phẩm trên TikTok (dùng trong URL khi update inventory).</summary>
public string TikTokProductId { get; set; } = null!;

/// <summary>ID kho hàng TikTok (bắt buộc khi update inventory).</summary>
public string WarehouseId { get; set; } = null!;
```

Cần tạo thêm EF Core migration cho 2 field mới này.

---

### BUG 5 — Return Detail: sai namespace prefix

**File:** `TikTokApiClient.cs` → `GetReturnDetailRawAsync`

| Mục | Code hiện tại | Đúng (từ Postman - Return and Refund API 2.0) |
|-----|--------------|----------------------------------------------|
| Namespace | `/return/202309/...` | `/return_refund/202309/...` |

TikTok dùng namespace `return_refund`, không phải `return`. Tất cả return/cancel endpoints:
- `POST /return_refund/202309/cancellations` — hủy đơn
- `POST /return_refund/202309/returns` — tạo return
- `POST /return_refund/202309/cancellations/search` — tìm kiếm cancellations
- `GET /return_refund/202309/reject_reasons` — lý do từ chối
- `GET /return_refund/202309/orders/{orderId}/aftersale_eligibility` — kiểm tra eligibility

**Fix:** Đổi tất cả path từ `/return/202309` → `/return_refund/202309`.

---

### BUG 6 — Finance Statement Orders: sai endpoint

**File:** `TikTokApiClient.cs` → `GetFinanceStatementOrdersRawAsync`

| Mục | Code hiện tại | Đúng (Postman "Get Statement Transactions") |
|-----|--------------|---------------------------------------------|
| Path | `/finance/202309/orders` | `/finance/202309/statements/{statementId}/statement_transactions` |
| statementId | query param | path variable |

**Postman URLs chuẩn:**
```
GET /finance/202309/statements/{statementId}/statement_transactions
  ?...&sort_field=order_create_time

GET /finance/202309/orders/{orderId}/statement_transactions
  ?...&sort_field=order_create_time  (lấy theo orderId)

GET /finance/202309/payments
  ?...&sort_field=create_time
```

**Fix:**
```csharp
public async Task<string?> GetFinanceStatementOrdersRawAsync(
    TikTokApiContext ctx, string statementId, string? pageToken, CancellationToken ct)
{
    var path = $"/finance/202309/statements/{statementId}/statement_transactions";
    // ...
}
```

---

## 5. VẤN ĐỀ THIẾU SÓT (KHÔNG PHẢI BUG CRITICAL)

### 5.1 Webhook Auto-Registration sau khi connect shop

**Hiện tại:** Hệ thống có webhook receiver (`WebhooksController`) nhưng không tự đăng ký callback URL với TikTok sau khi shop connect.

**TikTok cung cấp (Postman "Events" section):**
```
GET  /event/202309/webhooks   — xem webhook đã đăng ký
PUT  /event/202309/webhooks   — đăng ký/cập nhật callback URL + event_type
DELETE /event/202309/webhooks — xóa webhook
```

**Cần thêm:**
1. 3 methods vào `ITikTokApiClient` + `TikTokApiClient`
2. Gọi `PUT /event/202309/webhooks` trong `TikTokConnectionService.HandleCallbackAsync` sau khi lưu connection
3. Đăng ký events: `ORDER_STATUS_CHANGE`, `CANCEL_STATUS_CHANGE`, `RETURN_STATUS_CHANGE`, `AUTHORIZATION_REMOVED`

**Ưu tiên: 🟡 Trung** — Không đăng ký thì phải chờ polling, tính năng real-time không có.

---

### 5.2 Thiếu `TikTokProductId` và `WarehouseId` trong ProductTikTokMapping

Đã mô tả trong BUG 4. Đây là prerequisite để push inventory đúng.

---

### 5.3 ProductTikTokMapping thiếu flow lấy warehouse_id

Hiện tại không có logic để lấy `warehouse_id` khi user tạo mapping. TikTok cung cấp:
```
GET /logistics/202309/warehouses?...  — danh sách kho hàng
```

Cần thêm `GetWarehousesAsync` trong `ITikTokApiClient` và UI cho phép chọn kho khi tạo mapping.

---

### 5.4 Finance Payments endpoint chưa được dùng

Postman có `GET /finance/202309/payments` nhưng `ITikTokApiClient` chưa expose method này. Nếu cần hiển thị lịch sử payment/settlement thì thêm.

---

### 5.5 Product Search: nên dùng POST search thay vì GET list

| | Code hiện tại | Postman |
|-|--------------|---------|
| Endpoint | `GET /product/202309/products` | `POST /product/202309/products/search` (hỗ trợ filter keyword) |

Không critical, nhưng nếu shop có nhiều sản phẩm, `GET /products` phân trang không có filter keyword sẽ chậm khi tìm kiếm để mapping.

---

## 6. FRONTEND — CÁC TRANG CẦN XÂY DỰNG

### 6.1 TikTok Shops Page (`/tiktok-shops`)
- Bảng danh sách shop: ShopName, Region, Status badge, TokenExpiry, LastSyncedAt
- Nút **Connect New Shop** → gọi `GET /api/tiktok-shops/auth-url` → redirect OAuth TikTok
- Callback handler (FE nhận `?connected=true&shopCount=N`)
- Per-row: nút **Refresh Token**, **Disconnect**
- Status badge: Active (xanh) / Expired (vàng) / Revoked (đỏ)

### 6.2 Product Mappings Page (`/product-mappings`)
- Bảng danh sách mappings hiện có
- Filter theo Shop
- Modal tạo mapping:
  1. Chọn Shop
  2. Load TikTok SKU list → dropdown search
  3. Chọn sản phẩm nội bộ → dropdown search
  4. Chọn Warehouse (khi có BUG 4 fix)
- Edit / Delete mapping

### 6.3 TikTok Orders Page (`/tiktok-orders`)
- Bảng đơn hàng: OrderId, Shop, Status, CreatedAt, TotalAmount, SyncStatus
- Filter: Shop dropdown, Status multi-select, Date range
- Status badges theo TikTok numeric code (111=Awaiting Shipment, 121=In Transit, ...)
- Detail drawer: items list (tên sản phẩm, SKU, qty, price), finance info, reservation status
- Nút **Manual Sync** để trigger đồng bộ từ API

### 6.4 TikTok Returns Page (`/tiktok-returns`)
- Bảng: ReturnId, Shop, OrderId, Status, RequestedAt, Items
- Filter theo Shop, Status, DateRange
- Detail: return lines, refund amount

### 6.5 TikTok Finance Page (`/tiktok-finance`)
- Bảng statements: StatementId, Period, TotalFee, NetSettlement, Status
- Expand row → transaction list
- Summary cards: tổng doanh thu, phí TikTok, lợi nhuận ròng

### 6.6 TikTok Dashboard (bổ sung vào DashboardPage)
- Widget: Doanh thu TikTok vs Manual (bar chart)
- Widget: Top 5 sản phẩm TikTok theo doanh số
- Filter chọn Shop

---

## 7. PRIORITY ROADMAP

### 🔴 PRIORITY 1 — Sửa bugs TikTok API Client (3 ngày)

| # | Bug | Files cần sửa | Effort |
|---|-----|--------------|--------|
| 1 | Auth URL sai + Method sai + Param sai (BUG 1) | `TikTokApiClient.cs`, `TikTokSettings.cs`, `appsettings.json` | 0.5 ngày |
| 2 | Signing thiếu body (BUG 2) | `TikTokApiClient.cs` → `ComputeSign`, `BuildSignedPostRequest` | 0.5 ngày |
| 3 | Order Detail sai endpoint (BUG 3) | `TikTokApiClient.cs` → `GetOrderDetailRawAsync` | 0.25 ngày |
| 4 | Return prefix sai (BUG 5) | `TikTokApiClient.cs` → `GetReturnDetailRawAsync` | 0.25 ngày |
| 5 | Finance endpoint sai (BUG 6) | `TikTokApiClient.cs` → `GetFinanceStatementOrdersRawAsync` | 0.25 ngày |
| 6 | Inventory Update sai path + body (BUG 4) | `TikTokApiClient.cs`, `ITikTokApiClient.cs`, `ProductTikTokMapping.cs` + migration | 1.25 ngày |

**Tổng: ~3 ngày**

---

### 🟡 PRIORITY 2 — Webhook Auto-Registration (1 ngày)

1. Thêm `GetShopWebhooksAsync`, `UpdateShopWebhookAsync`, `DeleteShopWebhookAsync` vào interface + client
2. Gọi `UpdateShopWebhookAsync` từ `TikTokConnectionService.HandleCallbackAsync`
3. Đăng ký: `ORDER_STATUS_CHANGE`, `CANCEL_STATUS_CHANGE`, `RETURN_STATUS_CHANGE`, `AUTHORIZATION_REMOVED`

---

### 🟡 PRIORITY 3 — Frontend TikTok Pages (5 ngày)

| Trang | Effort |
|-------|--------|
| TikTok Shops connection page | 1 ngày |
| Product Mappings page | 1.5 ngày |
| TikTok Orders page | 1 ngày |
| TikTok Returns page | 0.5 ngày |
| TikTok Finance page | 1 ngày |

---

### 🟢 PRIORITY 4 — Polish & Testing (3 ngày)

- Integration tests cho TikTok API calls (sandbox)
- Unit tests cho `OrderEventHandler`, `ReturnEventHandler`
- E2E flow: kết nối shop → nhận webhook → kiểm kho
- TikTok Dashboard widgets
- Video sync + metrics page

---

## 8. CHECKLIST KỸ THUẬT TRƯỚC KHI LIVE

### TikTok API Integration
- [ ] `ExchangeCodeAsync` dùng GET + `auth.tiktok-shops.com` + param `auth_code` + không sign
- [ ] `RefreshTokenAsync` dùng GET + `auth.tiktok-shops.com` + không sign
- [ ] `ComputeSign` bao gồm request body JSON string (cho POST requests)
- [ ] `GetOrderDetailRawAsync` dùng `GET /order/202309/orders?ids=[...]`
- [ ] `UpdateInventoryAsync` dùng `POST /product/202309/products/{productId}/inventory/update` với body `{skus:[{id, inventory:[{quantity, warehouse_id}]}]}`
- [ ] `GetReturnDetailRawAsync` dùng namespace `return_refund/202309`
- [ ] `GetFinanceStatementOrdersRawAsync` dùng `/finance/202309/statements/{id}/statement_transactions`
- [ ] Webhook auto-registration sau khi connect shop
- [ ] `ProductTikTokMapping` có field `TikTokProductId` và `WarehouseId`

### Stock & Inventory
- [ ] StockMovement tạo trong cùng transaction với document gốc
- [ ] IdempotencyKey đúng format, unique constraint hoạt động
- [ ] Advisory lock trước khi tính AvailableStock
- [ ] Reservation lifecycle: Active → Committed (InTransit/121) | Released (Cancelled/140)

### Security
- [ ] `appsettings.json` không commit secret thật (dùng User Secrets)
- [ ] Access token/Refresh token được mã hóa trong DB (Data Protection)
- [ ] Webhook HMAC-SHA256 signature verified trước khi xử lý

### Frontend
- [ ] Không có `any` TypeScript type
- [ ] 401 response → auto refresh token → retry
- [ ] TikTok shop OAuth flow hoạt động end-to-end
- [ ] Product mapping flow rõ ràng với Warehouse selection

---

## 9. ĐÁNH GIÁ TỔNG THỂ

### Điểm mạnh
- Kiến trúc Clean Architecture rõ ràng, đúng dependency rule
- Domain model đầy đủ, StockMovement unified model và Reservation model rất vững
- Background services cover đủ use cases (token refresh, order sync, outbox, reservation expiry)
- Security: JWT, BCrypt, Data Protection, Global Query Filter tenancy isolation
- Polly resilience (retry + circuit-breaker) đã cài đặt cho TikTok API
- 13 migrations clean, không breaking change

### Điểm cần cải thiện
1. **TikTok API Client có 6 bugs nghiêm trọng** — toàn bộ TikTok integration sẽ fail khi test thực tế
2. **Frontend chưa có trang TikTok** — phần lớn business value chưa hiển thị
3. **Webhook auto-registration** chưa có → real-time sync không hoạt động
4. **ProductTikTokMapping thiếu fields** để push inventory đúng cách
5. **Auth base URL config sai** — cần verify và tách TokenBaseUrl riêng

### Ưu tiên ngắn hạn (2 tuần tới)
1. **Tuần 1:** Sửa 6 bugs TikTok API + test sandbox (3 ngày) + Webhook registration (1 ngày) + TikTok Shops FE page (1 ngày)
2. **Tuần 2:** Product Mappings FE (1.5 ngày) + TikTok Orders FE (1 ngày) + Finance FE (1 ngày) + Integration testing (1.5 ngày)
