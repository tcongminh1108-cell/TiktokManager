# Plan.md - Kế hoạch triển khai TikTok Shop Management System

> Tài liệu này chia dự án thành các **Phase** → **Task** → **Sub-task** có thể vibe code tuần tự với Claude. Mỗi task có **Definition of Done (DoD)** để biết khi nào hoàn thành.

> **Cách dùng:** Khi vào VS Code Claude Extension, chỉ định "Làm Phase X — Task Y theo Plan.md", Claude sẽ đọc CLAUDE.md + đoạn tương ứng trong Plan.md để sinh code.

---

## TIẾN ĐỘ HIỆN TẠI

> Cập nhật lần cuối: **2026-05-15**

| Task | Trạng thái | Ghi chú |
|------|-----------|---------|
| Task 0.1 — Solution Structure | ✅ DONE | Solution, 4 projects, references, Directory.Build.props, .editorconfig, .gitignore |
| Task 0.2 — NuGet Packages | ✅ DONE | Đã cài đủ packages; thêm `Microsoft.AspNetCore.Authentication.JwtBearer` vào Infrastructure |
| Task 0.3 — Base Classes | ✅ DONE | BaseEntity, ITenantEntity, IAuditable, ISoftDelete, ICurrentUser, PaginatedResult, PageRequest, ApiResponse, tất cả exceptions |
| Task 0.4 — DbContext | ✅ DONE | ApplicationDbContext + global query filter, AuditableEntityInterceptor, DesignTimeFactory, CurrentUser, DI wiring, migration InitialCreate |
| Task 0.5 — Exception Middleware | ✅ DONE | ExceptionHandlingMiddleware + TestController |
| Task 0.6 — Serilog & CorrelationId | ✅ DONE | CorrelationIdMiddleware, UserContextEnrichmentMiddleware, Serilog config từ appsettings |
| Task 0.7 — Swagger/Scalar | ✅ DONE | Scalar UI tại /scalar/v1, JWT Bearer security definition, operation transformer tự thêm lock icon |
| Task 0.8 — Docker | ✅ DONE | Dockerfile multi-stage, docker-compose.yml (postgres + backend + pgadmin profile), .env.example, .dockerignore |
| Task 1.1 — Entity Tenant/User/RefreshToken | ✅ DONE | 3 entities, 2 enums, 3 EF configs, migration AddTenantUserRefreshToken, snake_case naming |
| Task 1.2 — JWT Service & Password Hasher | ✅ DONE | JwtService (access token tuple), IPasswordHasher (BCrypt), JwtSettings, DI wiring |
| Task 1.3 — Auth Endpoints | ✅ DONE | RegisterTenant, Login, Refresh, Logout, Me + AuthService + DTOs + validators |
| Task 1.4 — User Management | ✅ DONE | Admin CRUD, activate/deactivate, change-password, last-admin guard + UserService |
| Task 1.5 — Authorization Policies | ✅ DONE | RequireAdmin, RequireManagerOrAbove, RequireAuthenticated policies + IApplicationDbContext |
| Phase 1 — Auth & Tenant | ✅ DONE | All tasks complete |
| Task 2.1 — Entity Supplier | ✅ DONE | Supplier entity, EF config, partial unique index (TenantId, Code) |
| Task 2.2 — Supplier Service & Controller | ✅ DONE | CRUD + restore, Manager+ write, Admin restore, pagination/filter/sort |
| Task 2.3 — Entity Product | ✅ DONE | Product entity (Code, Name, SellingPrice numeric 18,4, Unit, ImageUrl, IsActive), EF config |
| Task 2.4 — Product Service & Controller | ✅ DONE | CRUD + restore + activate/deactivate, filter by isActive/minPrice/maxPrice |
| Phase 2 — Catalog | ✅ DONE | Migration AddSupplierProduct applied |
| Task 3.1 — Entity StockMovement | ✅ DONE | Entity, 2 enums, EF config, migration AddStockMovements, IStockMovementService + impl |
| Task 3.2 — Entity StockIn | ✅ DONE | Entity, EF config (FK → StockMovement, 3 indexes), migration AddStockIn |
| Task 3.3 — StockIn Service & Endpoints | ✅ DONE | DTOs, validators, IStockInService, StockInService (transaction-safe), StockInsController |
| Task 3.4 — Entity StockOut | ✅ DONE | Entity, EF config (FK → StockMovement, 2 indexes), migration AddStockOut |
| Task 3.5 — StockOut Service & Endpoints | ✅ DONE | DTOs, validators, IStockOutService, StockOutService (explicit tx + advisory lock), StockOutsController |
| Phase 3 — Stock Movements | ✅ DONE | |
| Phase 3.5 — Inventory & Reservation | ❌ TODO | |
| Phase 4 — Frontend Foundation | ❌ TODO | |
| Phase 5 — Frontend CRUD | ❌ TODO | |
| Phase 6 — Dashboard | ❌ TODO | |
| Phase 7.1–7.9 — TikTok Integration | ❌ TODO | |
| Phase 8 — Testing & Polish | ✅ DONE | 35 unit tests pass; integration test infra (Testcontainers) compiles |

> **👉 BƯỚC TIẾP THEO:** Phase 3 — Stock Movements (StockIn, StockOut, StockMovement unified ledger).

---

## MỤC LỤC

- [Phase Pre: TikTok App Registration (làm song song từ ngày 1)](#phase-pre-tiktok-app-registration)
- [Phase 0: Khởi tạo & Infrastructure](#phase-0-khởi-tạo--infrastructure)
- [Phase 1: Multi-Tenant, User, Authentication & Authorization](#phase-1-multi-tenant-user-authentication--authorization)
- [Phase 2: Catalog (Products & Suppliers)](#phase-2-catalog-products--suppliers)
- [Phase 3: Stock Movements + StockIn + StockOut](#phase-3-stock-movements--stockin--stockout)
- [Phase 3.5: Inventory View + Reservation Model](#phase-35-inventory-view--reservation-model)
- [Phase 4: Frontend Foundation](#phase-4-frontend-foundation)
- [Phase 5: Frontend CRUD Pages](#phase-5-frontend-crud-pages)
- [Phase 6: Internal Dashboard (Profit, Cost Analytics)](#phase-6-internal-dashboard)
- [Phase 7.1: TikTok Connection (OAuth, shop_cipher, region)](#phase-71-tiktok-connection)
- [Phase 7.2: Webhook Receiver + HMAC Verify + Queue](#phase-72-webhook-receiver)
- [Phase 7.3: Polling Reconciliation + Per-Shop Rate Limiter](#phase-73-polling-reconciliation)
- [Phase 7.4: Product Mapping](#phase-74-product-mapping)
- [Phase 7.5: Order Handler (Reservation + Commit)](#phase-75-order-handler)
- [Phase 7.6: Cancel & Return Handler](#phase-76-cancel--return-handler)
- [Phase 7.7: Push Inventory to TikTok (Outbox)](#phase-77-push-inventory-to-tiktok)
- [Phase 7.8: Finance API Integration (Lợi nhuận thực)](#phase-78-finance-api-integration)
- [Phase 7.9: Video Sync + TikTok Dashboard](#phase-79-video-sync--tiktok-dashboard)
- [Phase 8: Testing, Docker, Polish, Go-live](#phase-8-testing-docker-polish)

---

# PHASE PRE: TikTok App Registration

**Mục tiêu:** Có đầy đủ credentials để code Phase 7 không bị kẹt vì chờ duyệt app. Bắt đầu **NGAY ngày 1** vì TikTok review có thể mất 3-7 ngày.

## Task Pre.1 — Tạo tài khoản Partner

- Đăng ký tại https://partner.tiktokshop.com (Global) hoặc https://partner.us.tiktokshop.com (US-only).
- Verify business email / documents.
- **Chọn đúng region** (không đổi được sau khi tạo).

## Task Pre.2 — Tạo App

- Trong Partner Center → Apps → Create App.
- Chọn loại **Custom App** (nếu chỉ dùng nội bộ) hoặc **Public App** (publish lên store).
- Điền:
  - **Redirect URL:** `https://yourdomain.com/api/tiktok-shops/callback` (Phase 7.1) — có thể dùng `https://localhost:5001/...` cho dev đầu tiên, đổi sau.
  - **Webhook URL:** `https://yourdomain.com/api/webhooks/tiktok` (Phase 7.2) — TikTok BẮT BUỘC HTTPS public. Local dev dùng `ngrok` để expose.

## Task Pre.3 — Xin scopes

Apply for các permissions sau (qua "Manage API" trong app):
- [ ] `seller.shop.read`
- [ ] `product.list`, `product.read`, `product.detail`
- [ ] `product.inventory.update` ← cho push inventory
- [ ] `order.list`, `order.detail`
- [ ] `fulfillment.shipping_doc.read`
- [ ] `return.list`, `return.detail`
- [ ] `finance.statement.read`, `finance.payment.read` ← cho lợi nhuận thực
- [ ] `webhook.subscribe` (nếu cần programmatic subscribe, mặc định auto khi cấu hình callback URL)

> **Lưu ý:** mỗi scope cần review riêng của TikTok, có thể mất vài ngày. Một số scope sensitive (finance) review lâu hơn.

## Task Pre.4 — Setup Sandbox (chỉ US/UK có sandbox)

- Trong Developer Portal, enable Sandbox mode.
- TikTok cấp app_key, app_secret riêng cho sandbox.
- Có shop test để gọi API mà không ảnh hưởng shop thật.

> **Nếu region của bạn không có sandbox:** dev trên shop test cá nhân, dùng product giá trị thấp.

## Task Pre.5 — Lưu credentials an toàn

Lưu vào secret manager (chưa commit code):
```
TIKTOK_APP_KEY=...
TIKTOK_APP_SECRET=...
TIKTOK_SANDBOX_APP_KEY=...    (nếu có)
TIKTOK_SANDBOX_APP_SECRET=... (nếu có)
TIKTOK_REDIRECT_URI=https://...
TIKTOK_WEBHOOK_URL=https://...
TIKTOK_REGION=GLOBAL|US|EU
```

### DoD Pre
- App đã được TikTok approve các scope cần thiết.
- Có thể login Partner Center và thấy app trong trạng thái Active.
- Webhook URL đã được verify (gửi test event qua Developer Tools → Webhook → Send Test).

---

# PHASE 0: Khởi tạo & Infrastructure

**Mục tiêu:** Có solution chạy được, kết nối DB, Swagger OK, Docker hoạt động.

## Task 0.1 — Tạo Solution & Project Structure

### Sub-tasks
- Tạo solution `TikTokShop.sln`.
- Tạo 4 project: `TikTokShop.Domain`, `TikTokShop.Application`, `TikTokShop.Infrastructure`, `TikTokShop.Api`.
- Wire reference giữa các project theo Clean Architecture (xem CLAUDE.md §2).
- Tạo `Directory.Build.props` để chuẩn hóa: `<TargetFramework>net9.0</TargetFramework>`, `<Nullable>enable</Nullable>`, `<ImplicitUsings>enable</ImplicitUsings>`, `<TreatWarningsAsErrors>false</TreatWarningsAsErrors>`.
- Tạo `.editorconfig` chuẩn .NET.
- Tạo `.gitignore` cho .NET + Node + VS Code.

### DoD
- `dotnet build` chạy không lỗi.
- Project reference đúng chiều, không có circular reference.

## Task 0.2 — Cài đặt NuGet packages cốt lõi

### Packages theo từng project

**Domain:** (không cần package)

**Application:**
- `FluentValidation` + `FluentValidation.DependencyInjectionExtensions`
- `Mapster` + `Mapster.DependencyInjection`

**Infrastructure:**
- `Microsoft.EntityFrameworkCore` (9.x)
- `Npgsql.EntityFrameworkCore.PostgreSQL`
- `Microsoft.EntityFrameworkCore.Design`
- `Microsoft.AspNetCore.Authentication.JwtBearer`
- `BCrypt.Net-Next`
- `Polly` (cho retry TikTok API)

**Api:**
- `Serilog.AspNetCore`
- `Serilog.Sinks.Console`
- `Serilog.Sinks.File`
- `Swashbuckle.AspNetCore` hoặc `Scalar.AspNetCore` (UI đẹp hơn)
- `Microsoft.AspNetCore.OpenApi`

### DoD
- `dotnet restore` OK.
- Project chạy được `dotnet run` ra trang Swagger trắng (chưa có endpoint).

## Task 0.3 — Base classes & Common types

### Sub-tasks
- `Domain/Common/BaseEntity.cs` (theo CLAUDE.md §3.3).
- `Domain/Common/ITenantEntity.cs`, `IAuditable.cs`, `ISoftDelete.cs`.
- `Domain/Interfaces/ICurrentUser.cs` (đọc TenantId, UserId, Role từ HttpContext).
- `Application/Common/Models/PaginatedResult<T>.cs`.
- `Application/Common/Models/PageRequest.cs` (PageNumber, PageSize, SortBy, SortDirection, Search).
- `Application/Common/Models/ApiResponse<T>.cs` + factory.
- `Domain/Exceptions/AppException.cs` (abstract) + các exception con (NotFoundException, ValidationException, ForbiddenException, ConflictException, BusinessRuleException).

### DoD
- Build pass.
- Có thể `throw new NotFoundException(nameof(Product), id)` ở bất kỳ đâu trong Application.

## Task 0.4 — DbContext, Interceptors, Connection

### Sub-tasks
- `Infrastructure/Persistence/ApplicationDbContext.cs`:
  - Inject `ICurrentUser`.
  - Apply all `IEntityTypeConfiguration<>` qua `ApplyConfigurationsFromAssembly`.
  - `OnModelCreating` setup Global Query Filter cho tenant + soft delete.
- `Infrastructure/Persistence/Interceptors/AuditableEntityInterceptor.cs`:
  - On `SavingChanges`: set CreatedAt/CreatedBy/TenantId cho Added; UpdatedAt/UpdatedBy cho Modified; convert Delete sang soft delete.
- Đăng ký trong `Infrastructure/DependencyInjection.cs`:
  ```csharp
  services.AddDbContext<ApplicationDbContext>(opt =>
      opt.UseNpgsql(connectionString)
         .AddInterceptors(provider.GetRequiredService<AuditableEntityInterceptor>()));
  ```
- `appsettings.json` thêm `ConnectionStrings:DefaultConnection`.
- `CurrentUser.cs` (Api hoặc Infrastructure) đọc claim từ `IHttpContextAccessor`.

### DoD
- `dotnet ef migrations add InitialCreate` chạy được (chưa cần entity nghiệp vụ).
- `dotnet ef database update` tạo DB rỗng trên Postgres.

## Task 0.5 — Global Exception Handling Middleware

### Sub-tasks
- `Api/Middlewares/ExceptionHandlingMiddleware.cs`:
  - Bắt `AppException` → map sang HTTP status tương ứng.
  - Bắt `FluentValidation.ValidationException` → 400 + chi tiết errors.
  - Bắt `Exception` chung → 500 + log full, ẩn detail ở Prod.
  - Trả format `ApiResponse` chuẩn.
- Đăng ký đầu pipeline trong `Program.cs`.

### DoD
- Tạo endpoint test `/api/test/throw` throw NotFoundException → trả 404 đúng format.

## Task 0.6 — Serilog, CorrelationId

### Sub-tasks
- Cấu hình Serilog đọc từ `appsettings.json`.
- Middleware `CorrelationIdMiddleware` gắn header `X-Correlation-Id` (sinh GUID nếu request chưa có).
- Enrich log với CorrelationId, TenantId (nếu có), UserId.

### DoD
- Mỗi request có log entry kèm correlation id.

## Task 0.7 — Swagger / Scalar

### Sub-tasks
- Cấu hình Swagger có:
  - JWT Bearer security definition.
  - Group endpoints theo controller.
  - XML comments (bật `GenerateDocumentationFile` ở .csproj).
- Optional: Scalar UI thay vì Swagger UI (đẹp hơn).

### DoD
- Truy cập `/swagger` hoặc `/scalar` thấy doc.

## Task 0.8 — Dockerfile & docker-compose (sơ bộ)

### Sub-tasks
- `src/TikTokShop.Api/Dockerfile` (multi-stage).
- `docker-compose.yml` ở root:
  - `postgres:16-alpine` (volume, healthcheck).
  - `backend` build từ Dockerfile, depends_on postgres healthy.
  - `pgadmin` (optional).
- `.env.example` ở root.

### DoD
- `docker compose up --build` chạy → backend tạo migration tự động (hoặc chạy thủ công lần đầu), trả Swagger.

---

# PHASE 1: Multi-Tenant, User, Authentication & Authorization

**Mục tiêu:** Có thể đăng ký tenant, đăng nhập, nhận JWT, gọi API có authorize.

## Task 1.1 — Entity: Tenant, User, RefreshToken

### Sub-tasks
- `Domain/Entities/Tenant.cs`: Id, Name, Code (unique), Status (enum TenantStatus), ContactEmail, ContactPhone, CreatedAt, ...
  - Tenant KHÔNG kế thừa BaseEntity (vì nó là root tenant).
- `Domain/Entities/User.cs`:
  - Id, TenantId, Email (unique trong tenant), PasswordHash, FullName, Role (enum UserRole), IsActive, LastLoginAt.
  - Có audit nhưng login flow đặc biệt → kế thừa BaseEntity, riêng PasswordHash không bao giờ trả về DTO.
- `Domain/Entities/RefreshToken.cs`: Id, UserId, TenantId, TokenHash, ExpiresAt, RevokedAt, ReplacedByTokenId, CreatedByIp.
- `Domain/Enums/UserRole.cs`, `TenantStatus.cs` (trong folder `Enums/`).
- EF Configuration cho từng entity (`Infrastructure/Persistence/Configurations/`).
- Tạo migration `AddTenantUserRefreshToken`.

### DoD
- Migration apply OK.
- Bảng `tenants`, `users`, `refresh_tokens` có sẵn trong DB với index đúng.

## Task 1.2 — JWT Service & Password Hasher

### Sub-tasks
- `Application/Interfaces/IJwtService.cs`: `GenerateAccessToken(User)`, `GenerateRefreshToken()`, `ValidateAccessToken(string)`.
- `Infrastructure/Identity/JwtService.cs` implement (đọc settings từ `JwtSettings` Options).
- `Infrastructure/Identity/PasswordHasher.cs` dùng BCrypt: `Hash(string)`, `Verify(string, string)`.
- Đăng ký DI.
- Cấu hình `AddAuthentication().AddJwtBearer(...)` trong `Api/Program.cs`.

### DoD
- Unit test: hash password rồi verify đúng/sai.
- Generate token, decode được trên jwt.io với secret tương ứng.

## Task 1.3 — Auth Endpoints

### Endpoints
- `POST /api/auth/register-tenant`: tạo tenant + admin user đầu tiên.
- `POST /api/auth/login`: trả access + refresh token.
- `POST /api/auth/refresh`: dùng refresh token lấy access mới (rotate refresh token).
- `POST /api/auth/logout`: revoke refresh token hiện tại.
- `GET /api/auth/me`: thông tin user hiện tại (cần auth).

### Sub-tasks
- DTOs: `RegisterTenantRequest`, `LoginRequest`, `RefreshTokenRequest`, `AuthResponse`, `CurrentUserDto`.
- Validators (FluentValidation) cho từng request.
- `IAuthService` + `AuthService` (Application).
- `AuthController` (Api).
- Lưu refresh token dưới dạng hash (SHA256) trong DB, client giữ token thô.
- Rotation: mỗi lần refresh tạo token mới, mark cũ là Revoked + ReplacedByTokenId.

### DoD
- Postman/curl chạy full flow: register → login → gọi `/me` thành công → refresh → logout.

## Task 1.4 — User Management (cho Admin tenant)

### Endpoints (đều require role Admin)
- `GET /api/users` (paginated, filter by role, search by email/name).
- `GET /api/users/{id}`.
- `POST /api/users` (Admin tạo user mới cho tenant của mình).
- `PUT /api/users/{id}` (sửa thông tin, đổi role).
- `PUT /api/users/{id}/change-password` (Admin reset, hoặc user tự đổi).
- `DELETE /api/users/{id}` (soft delete, không cho xóa chính mình).
- `POST /api/users/{id}/activate` / `deactivate`.

### Sub-tasks
- DTOs, Validators.
- `IUserService` + `UserService`.
- `UsersController`.
- Business rule: KHÔNG được thay đổi role của Admin cuối cùng trong tenant → throw `BusinessRuleException`.

### DoD
- Tạo user mới → đăng nhập được bằng user mới.
- Test: Admin xóa Admin cuối cùng → bị reject 422.

## Task 1.5 — Role-based Authorization Policies

### Sub-tasks
- Đăng ký các policy trong `Program.cs`:
  - `RequireAdmin`: role = Admin.
  - `RequireManagerOrAbove`: role ∈ { Admin, Manager }.
  - `RequireAuthenticated`: chỉ cần login.
- Optional: Resource-based authorization handler cho trường hợp Staff chỉ xem record mình tạo.

### DoD
- Endpoint admin-only test với user Staff → 403.

---

# PHASE 2: Catalog (Products & Suppliers)

**Mục tiêu:** CRUD đầy đủ Product, Supplier có pagination/filter/sort/soft-delete.

## Task 2.1 — Entity Supplier

### Fields
- Id, TenantId, Code (unique trong tenant), Name, Phone, Email, Address, Note.
- Kế thừa BaseEntity.

### Sub-tasks
- Entity + Configuration + Migration.
- Index unique (TenantId, Code) where IsDeleted = false (partial index).

## Task 2.2 — Supplier Service & Controller

### Endpoints
- `GET /api/suppliers?pageNumber=...&pageSize=...&search=...&sortBy=...&sortDirection=...`
- `GET /api/suppliers/{id}`
- `POST /api/suppliers` (Manager+)
- `PUT /api/suppliers/{id}` (Manager+)
- `DELETE /api/suppliers/{id}` (Manager+) — soft delete.
- `POST /api/suppliers/{id}/restore` (Admin) — undo soft delete.

### Sub-tasks
- DTOs: `SupplierDto`, `CreateSupplierRequest`, `UpdateSupplierRequest`, `SupplierQueryParams`.
- Validators.
- `ISupplierService` + `SupplierService`.
- Helper `IQueryable<T>.ApplySort<T>` whitelist field theo enum hoặc dictionary.
- Helper `ApplyPagination` trả `PaginatedResult<T>`.

### DoD
- Test full CRUD + filter + sort.
- Test xóa rồi list không thấy nữa, restore lại thấy lại.

## Task 2.3 — Entity Product

### Fields
- Id, TenantId, Code (unique trong tenant), Name, Description, SellingPrice (numeric 18,4), Unit (cái/hộp/...), ImageUrl, IsActive.
- Kế thừa BaseEntity.
- (Note: không có SupplierId trên Product vì 1 sản phẩm có thể nhập từ nhiều nhà cung cấp — quan hệ qua StockIn.)

### Sub-tasks giống Task 2.1.

## Task 2.4 — Product Service & Controller

### Endpoints — tương tự Supplier.
### Filter bổ sung: `minPrice`, `maxPrice`, `isActive`.

### DoD: tương tự Supplier.

---

# PHASE 3: Stock Movements + StockIn + StockOut

**Mục tiêu:** Có model `StockMovement` thống nhất, các nghiệp vụ nhập/bán tay sinh movement đúng. Đây là **nền móng quan trọng nhất** vì TikTok sync ở Phase 7 sẽ tái sử dụng pattern này.

> **⚠️ Đọc lại CLAUDE.md §6.1 (Stock Movement Unified Model) trước khi bắt đầu phase này.**

## Task 3.1 — Entity StockMovement (làm TRƯỚC StockIn/StockOut)

### Fields (xem chi tiết trong CLAUDE.md §6.1)
- Id, TenantId, ProductId, Type (enum), Source (enum), Quantity, UnitCost, OccurredAt
- StockInId?, StockOutId?, TikTokOrderItemId? (chỉ 1 được set)
- IdempotencyKey (unique trong tenant)
- Note
- Kế thừa BaseEntity

### Sub-tasks
- Entity + Configuration + Migration `AddStockMovements`.
- Unique constraint `(TenantId, IdempotencyKey)`.
- Index `(TenantId, ProductId, OccurredAt DESC)`, `(TenantId, Source, OccurredAt)`.
- Check constraint `Quantity > 0` ở DB.
- Enum `StockMovementType` và `StockMovementSource` trong `Domain/Enums/`.

### Service interface
```csharp
public interface IStockMovementService
{
    Task<StockMovement> RecordAsync(
        Guid productId,
        StockMovementType type,
        StockMovementSource source,
        int quantity,
        decimal unitCost,
        DateTimeOffset occurredAt,
        string idempotencyKey,
        StockMovementReference reference,  // class chứa StockInId/StockOutId/TikTokOrderItemId
        string? note = null,
        CancellationToken ct = default);

    Task<int> GetStockOnHandAsync(Guid productId, CancellationToken ct = default);
    Task<int> GetStockOnHandWithLockAsync(Guid productId, CancellationToken ct = default);
}
```

### Logic `RecordAsync` bắt buộc
1. Validate quantity > 0, unitCost ≥ 0.
2. Validate đúng 1 reference được set.
3. Try insert. Nếu trùng `IdempotencyKey` (DbUpdateException, PG SqlState 23505) → return movement đã tồn tại (idempotent, không throw).
4. Không gọi SaveChanges nếu đang trong UnitOfWork bên ngoài — để caller control transaction.

### DoD
- Insert 1 movement OK.
- Insert lại với cùng IdempotencyKey → không sinh thêm row, return movement cũ.
- GetStockOnHand aggregate đúng (manual SQL test với data seed).

## Task 3.2 — Entity StockIn (Nhập hàng)

### Fields
- Id, TenantId, ProductId (FK), SupplierId (FK), Quantity (int), UnitPrice (numeric 18,4), TotalAmount (numeric 18,4), TransactionDate (timestamptz), Note.
- TotalAmount tự tính server-side trước khi save.

### Sub-tasks
- Entity + Configuration + Migration.
- Index (TenantId, ProductId), (TenantId, SupplierId), (TenantId, TransactionDate desc).

## Task 3.3 — StockIn Service & Endpoints

### Endpoints
- `GET /api/stock-ins` (filter: productId, supplierId, dateFrom, dateTo).
- `GET /api/stock-ins/{id}`.
- `POST /api/stock-ins`.
- `PUT /api/stock-ins/{id}`.
- `DELETE /api/stock-ins/{id}` (soft delete).

### Business rules & integration với StockMovement
- **Khi Create:** trong cùng transaction:
  1. Insert StockIn document.
  2. Gọi `_stockMovementService.RecordAsync(...)` với:
     - Type = `In`, Source = `Manual`
     - IdempotencyKey = `$"stockin:{stockIn.Id}"`
     - Reference: StockInId = stockIn.Id
     - OccurredAt = stockIn.TransactionDate
- **Khi Update:** chỉ cho phép sửa Note, TransactionDate. Quantity/UnitPrice → đề xuất tạo `Adjustment` movement bù trừ thay vì sửa, hoặc lock không cho sửa (chính sách an toàn nhất). 
  - **Khuyến nghị:** không cho sửa Quantity/UnitPrice; nếu sai → xóa rồi tạo lại.
- **Khi Soft Delete:** trong cùng transaction:
  1. Soft delete StockIn.
  2. Sinh compensating movement: Type = `Out`, Source = `Adjustment`, Quantity = stockIn.Quantity, IdempotencyKey = `$"stockin-reverse:{stockIn.Id}"`.

### DoD
- Tạo StockIn 10 cái → GetStockOnHand = 10.
- Xóa StockIn đó → GetStockOnHand = 0.
- Tạo lại cùng StockIn (giả sử cùng Id) → idempotent, không cộng thêm.

## Task 3.4 — Entity StockOut (Bán hàng thủ công, không qua TikTok)

### Fields
- Id, TenantId, ProductId (FK), CustomerName (string, không FK), Quantity, UnitPrice, TotalAmount, TransactionDate, Note.

### Sub-tasks: tương tự StockIn.

## Task 3.5 — StockOut Service & Endpoints

### Endpoints: tương tự StockIn.

### Business rules & integration với StockMovement
- **Khi Create:** trong cùng transaction với isolation level `ReadCommitted`:
  1. Gọi `_stockMovementService.GetStockOnHandWithLockAsync(productId)` — lock product row.
  2. Nếu `stockOnHand < request.Quantity` → throw `BusinessRuleException("Insufficient stock: available=X, requested=Y")`.
  3. Insert StockOut document.
  4. Gọi `_stockMovementService.RecordAsync(...)` với:
     - Type = `Out`, Source = `Manual`
     - IdempotencyKey = `$"stockout:{stockOut.Id}"`
     - Reference: StockOutId = stockOut.Id
- **Khi Soft Delete:** sinh compensating movement Type = `In`, Source = `Adjustment`, IdempotencyKey = `$"stockout-reverse:{stockOut.Id}"`.

### DoD
- Bán quantity > tồn → 422 với message rõ ràng.
- Bán quantity ≤ tồn → tồn giảm đúng.
- Test concurrency: 2 request bán song song khi tồn vừa đủ 1 → 1 thành công, 1 fail (không oversell).

---

# PHASE 3.5: Inventory View + Reservation Model

**Mục tiêu:** Inventory aggregate đúng từ StockMovement + Reservation entity để chống oversell khi đơn TikTok chưa ship.

> **⚠️ Đọc lại CLAUDE.md §6.2 (Inventory Reservation Model) trước khi bắt đầu phase này.**

## Task 3.5.1 — Entity InventoryReservation

### Fields (chi tiết trong CLAUDE.md §6.2)
- Id, TenantId, ProductId, Quantity, Status (enum InventoryReservationStatus)
- TikTokOrderItemId? (FK nullable)
- ReservedAt, ResolvedAt?, ExpiresAt
- IdempotencyKey (unique)
- Kế thừa BaseEntity (có soft delete bình thường — khác StockMovement).

### Sub-tasks
- Entity + Configuration + Migration `AddInventoryReservations`.
- Unique constraint `(TenantId, IdempotencyKey)`.
- Index `(TenantId, ProductId, Status)`, partial index `(TenantId, ExpiresAt) WHERE status = 1`.

## Task 3.5.2 — IReservationService

### Interface
```csharp
public interface IReservationService
{
    Task<InventoryReservation> CreateAsync(
        Guid productId,
        int quantity,
        Guid? tikTokOrderItemId,
        string idempotencyKey,
        DateTimeOffset? expiresAt = null,
        CancellationToken ct = default);
    
    Task CommitAsync(string idempotencyKey, CancellationToken ct = default);
    Task ReleaseAsync(string idempotencyKey, CancellationToken ct = default);
    
    Task<int> GetActiveReservedQuantityAsync(Guid productId, CancellationToken ct = default);
}
```

### Logic
- `CreateAsync`: idempotent check trước (nếu IdempotencyKey đã có Active → return). Insert mới với Status = Active.
- `CommitAsync`: tìm reservation theo IdempotencyKey, set Status = Committed, ResolvedAt = now. **KHÔNG sinh StockMovement ở đây** — caller (OrderHandler) sẽ làm.
- `ReleaseAsync`: tương tự CommitAsync nhưng set Status = Released.

### DoD
- Create reservation 5 → ActiveReservedQuantity = 5.
- Commit → ActiveReservedQuantity = 0 (chỉ count Active).
- Create lại cùng IdempotencyKey → không tạo thêm.

## Task 3.5.3 — Inventory aggregate query (cập nhật)

### Endpoint
- `GET /api/inventory?pageNumber=...&pageSize=...&search=...&sortBy=...`

### Logic — trả 3 con số
```csharp
public record InventoryItemDto(
    Guid ProductId, string ProductCode, string ProductName,
    decimal SellingPrice, 
    int TotalIn, int TotalOut,
    int PhysicalStock,     // từ StockMovements aggregate (như cũ)
    int ReservedQuantity,  // từ InventoryReservations Status=Active
    int AvailableStock,    // = PhysicalStock - ReservedQuantity
    decimal? AvgCostPrice, decimal? EstimatedValue);
```

### SQL aggregate
```sql
WITH movements AS (
  SELECT product_id,
         SUM(CASE WHEN type IN (1, 3) THEN quantity ELSE 0 END) AS total_in,
         SUM(CASE WHEN type IN (2, 4) THEN quantity ELSE 0 END) AS total_out,
         SUM(CASE WHEN type IN (1, 3) THEN quantity ELSE -quantity END) AS physical_stock,
         SUM(CASE WHEN type IN (1, 3) THEN quantity * unit_cost ELSE 0 END) 
           / NULLIF(SUM(CASE WHEN type IN (1, 3) THEN quantity ELSE 0 END), 0) AS avg_cost
  FROM stock_movements
  WHERE tenant_id = @tenantId
  GROUP BY product_id
),
reservations AS (
  SELECT product_id, SUM(quantity) AS reserved_qty
  FROM inventory_reservations
  WHERE tenant_id = @tenantId AND status = 1 AND is_deleted = false
  GROUP BY product_id
)
SELECT p.id, p.code, p.name, p.selling_price,
       COALESCE(m.total_in, 0), COALESCE(m.total_out, 0),
       COALESCE(m.physical_stock, 0) AS physical_stock,
       COALESCE(r.reserved_qty, 0) AS reserved,
       COALESCE(m.physical_stock, 0) - COALESCE(r.reserved_qty, 0) AS available,
       m.avg_cost
FROM products p
LEFT JOIN movements m ON m.product_id = p.id
LEFT JOIN reservations r ON r.product_id = p.id
WHERE p.tenant_id = @tenantId AND p.is_deleted = false;
```

### DoD
- Tạo đơn TikTok mới về (AwaitingShipment) → ReservedQuantity tăng, PhysicalStock không đổi, AvailableStock giảm.
- Đơn ship → PhysicalStock giảm, ReservedQuantity giảm (về 0), AvailableStock không đổi (đã giảm sẵn).

## Task 3.5.4 — Inventory detail (lịch sử + reservations active)

### Endpoint
- `GET /api/inventory/{productId}?pageNumber=...`
- Trả: summary (3 số) + list movements paginated + list active reservations (with TikTok order info nếu có).

## Task 3.5.5 — Cập nhật StockOut.Create để dùng AvailableStock

### Logic cũ (Phase 3.3): check PhysicalStock.
### Logic mới: check AvailableStock = PhysicalStock - ReservedQuantity.

Lý do: đơn TikTok đã đặt giữ chỗ trước → bán tay không được "ăn" vào.

## Task 3.5.6 — PostgreSQL advisory lock helper (như cũ)

(Giữ nguyên Task 3.5.3 cũ)

## Task 3.5.7 — Auto-expire reservation job

### Sub-tasks
- `ReservationExpiryService : BackgroundService` chạy mỗi 1 giờ.
- Tìm reservations `Status = Active AND ExpiresAt < now`.
- Set Status = Expired, ResolvedAt = now.
- Log warning với context (orderId, productId, expiredAfterDays) — đây là tín hiệu sync TikTok lag.
- Optional: gửi notification cho admin tenant.

### DoD
- Tạo reservation expires_at = past → job chạy → Status = Expired.
- AvailableStock tăng lại tương ứng.

## Task 3.5.8 — Seed data dev

- Endpoint dev `POST /api/dev/seed` (chỉ Development).
- Tạo 50 products, 10 suppliers, 200 stock-ins, 150 stock-outs, 20 reservations active.

---

# PHASE 4: Frontend Foundation

**Mục tiêu:** Có khung React + Vite + TS chạy được, có login, layout, route protected.

## Task 4.1 — Khởi tạo project

### Sub-tasks
- `npm create vite@latest frontend -- --template react-ts`.
- Cài: `react-router-dom`, `axios`, `@tanstack/react-query`, `@tanstack/react-query-devtools`, `zustand`, `antd` (hoặc `tailwindcss` + `shadcn/ui`), `dayjs`, `react-hook-form`, `zod`, `@hookform/resolvers`.
- Cấu hình ESLint + Prettier.
- Tạo cấu trúc thư mục theo CLAUDE.md §2.

### DoD
- `npm run dev` chạy, render trang trắng "Hello".

## Task 4.2 — Axios instance + interceptors

### Sub-tasks
- `shared/lib/api-client.ts`: tạo instance với baseURL từ env.
- Request interceptor: gắn `Authorization: Bearer <token>` từ Zustand auth store.
- Response interceptor: handle 401 → call refresh → retry. Fail thì redirect `/login`.

### DoD
- Console log thấy request có header Authorization sau khi login.

## Task 4.3 — Auth store + types

### Sub-tasks
- `features/auth/store/useAuthStore.ts` (Zustand): accessToken, refreshToken, user, login(), logout(), setTokens(), hydrate từ localStorage.
- `features/auth/types/index.ts`: User, LoginRequest, AuthResponse.
- Persist token vào localStorage (Zustand persist middleware), hoặc httpOnly cookie nếu backend hỗ trợ.

### DoD
- Refresh trang vẫn còn đăng nhập.

## Task 4.4 — Pages Login / Register

### Sub-tasks
- `features/auth/pages/LoginPage.tsx`: form (email, password) với React Hook Form + Zod.
- `features/auth/pages/RegisterTenantPage.tsx`: form đăng ký tenant + admin user.
- Sau login thành công: redirect về `/dashboard`.
- Hiển thị error đẹp khi sai password (đọc từ ApiResponse).

### DoD
- Login thành công vào dashboard, login sai hiện toast lỗi.

## Task 4.5 — Layout & Routing

### Sub-tasks
- `layouts/AuthLayout.tsx`: layout giản đơn cho login/register.
- `layouts/MainLayout.tsx`: sidebar (menu theo role) + header (user info, logout) + content area.
- `app/router.tsx`: định nghĩa routes.
- `app/ProtectedRoute.tsx`: HOC check token + role.
- Sidebar menu hide item nếu user không có role tương ứng.

### Routes dự kiến:
```
/login                       (public)
/register-tenant             (public)
/dashboard                   (auth)
/products                    (auth, all roles)
/suppliers                   (auth, Manager+)
/stock-ins                   (auth, Manager+)
/stock-outs                  (auth, all roles)
/inventory                   (auth, all roles)
/users                       (auth, Admin only)
/settings/tiktok-shops       (auth, Admin only)
/profile                     (auth)
```

### DoD
- Truy cập `/dashboard` chưa login → redirect `/login`.
- Staff không thấy menu Users.

---

# PHASE 5: Frontend CRUD Pages

**Mục tiêu:** Trang quản lý cho mỗi resource đã có ở Backend Phase 2 & 3.

> **Template pattern:** Mỗi resource có 1 page List (table + filter + pagination + actions) + 1 modal hoặc page Create/Edit. Reuse component `DataTable` chung.

## Task 5.1 — Shared component DataTable

### Sub-tasks
- `shared/components/DataTable/DataTable.tsx`: wrapper antd Table tích hợp:
  - Pagination (server-side).
  - Sorter (server-side).
  - Filter (slot tùy chỉnh).
  - Search box.
  - Action column slot.
- `shared/hooks/useDataTable.ts`: quản lý state pageNumber/pageSize/sortBy/sortDirection/filters.

### DoD
- Demo trang sample dùng được DataTable với mock data.

## Task 5.2 — Trang Products

### Sub-tasks
- `features/products/api/productApi.ts`: list, getById, create, update, delete, restore.
- `features/products/api/useProducts.ts`, `useProductMutations.ts` (TanStack Query).
- `features/products/pages/ProductListPage.tsx`: dùng DataTable.
- `features/products/components/ProductFormModal.tsx`: form create/edit.
- Validation FE (zod) khớp với BE (FluentValidation).
- Toast success/error.

### DoD
- Tạo/sửa/xóa/khôi phục product OK, refresh list tự động.

## Task 5.3 — Trang Suppliers

Tương tự Products.

## Task 5.4 — Trang Stock-In

### Lưu ý
- Form chọn Product (autocomplete search by code/name, debounce 300ms).
- Form chọn Supplier (autocomplete).
- Auto-calc TotalAmount khi nhập Quantity hoặc UnitPrice (UI feedback).
- Submit server vẫn tự tính lại — đừng tin FE.

## Task 5.5 — Trang Stock-Out

Tương tự Stock-In, nhưng:
- Hiển thị tồn kho hiện tại của Product được chọn (gọi API `/api/inventory/{productId}`).
- Cảnh báo (đỏ) nếu quantity > tồn kho.

## Task 5.6 — Trang Inventory

### Sub-tasks
- Bảng: ProductCode, ProductName, TotalIn, TotalOut, StockOnHand, AvgCostPrice, EstValue (= stockOnHand * avgCostPrice).
- Click row → mở Drawer xem lịch sử (paginated).
- Filter: search by product name/code, range stockOnHand.
- Export CSV (client-side đủ, server-side sau).

## Task 5.7 — Trang Users (Admin only)

### Sub-tasks
- CRUD users + đổi role + activate/deactivate + reset password.
- Confirm modal cho action nhạy cảm.

---

# PHASE 6: Internal Dashboard

**Mục tiêu:** Tổng quan kinh doanh hợp nhất từ MỌI nguồn (Manual + TikTok).

> **Lưu ý:** Vì dashboard query đều aggregate trên `StockMovements`, dữ liệu sẽ tự động bao gồm cả đơn TikTok sau khi Phase 7 sinh ra movement. Phase 6 làm trước, Phase 7 tự "rót" data vào.
> 
> **Net revenue (lợi nhuận thực) tính được sau Phase 7.8** (khi có Finance API). Phase 6 chỉ làm gross revenue trước.

## Task 6.1 — KPI cards (gross only)

### Backend endpoints
- `GET /api/dashboard/overview?from=...&to=...&source=All|Manual|TikTokOrder` trả:
  - **Gross revenue** (sum movements Type=Out, Source theo filter, value = quantity * unitCost).
  - **Total cost** (sum movements Type=In, value = quantity * unitCost) — chỉ tính các In có source = Manual (giá vốn nhập từ supplier).
  - **Gross profit** = revenue - cost.
  - Total products, suppliers, physical stock value, reserved stock value.
  - Số lượng giao dịch nhập/xuất trong kỳ, tách theo Source.

> **Sau Phase 7.8:** thêm field `netRevenue`, `totalFees`, `netProfit` (= netRevenue - cost) khi user toggle "Net mode".

### Sub-tasks
- `IDashboardService` aggregate query, có param Source.
- DTO `OverviewDto` (thêm field breakdown by source).
- FE: `DashboardPage.tsx` với grid KPI + filter source.

### DoD
- Đổi date range hoặc source → số liệu thay đổi tương ứng.
- Sau Phase 7, đơn TikTok shipped → tự xuất hiện trong dashboard ngay.

## Task 6.2 — Charts

### Sub-tasks
- Chart doanh thu theo ngày (line chart, stack theo Source: Manual / TikTok per shop) — endpoint `/api/dashboard/revenue-by-day?source=...`.
- Chart top 10 sản phẩm bán chạy (bar) — `/api/dashboard/top-products`, có toggle xem theo source.
- Chart phân bổ doanh thu theo Source (pie: Manual vs TikTok shop nào) — `/api/dashboard/revenue-by-source`.
- FE dùng `recharts` hoặc Ant Design Charts.

## Task 6.3 — Cost & Profit table

- Bảng per-product: tổng bán (Manual + TikTok), gross revenue, giá vốn (avg), gross profit, biên lợi nhuận %.
- Cột bổ sung: tách số bán Manual vs TikTok.
- Sau Phase 7.8: thêm column "Phí TikTok", "Net revenue", "Net profit".
- Export CSV.

---

# PHASE 7.1: TikTok Connection

**Mục tiêu:** Kết nối shop, lưu credentials đầy đủ (access_token, refresh_token, shop_cipher, region).

> **⚠️ Đọc lại CLAUDE.md §12.5 trước khi bắt đầu.**

## Task 7.1.1 — Entity TikTokShopConnection (cập nhật so với plan cũ)

### Fields (đầy đủ)
- Id, TenantId
- **ShopId** (TikTok shop id, dùng để hiển thị)
- **ShopCipher** (BẮT BUỘC, encrypted) — string TikTok cấp riêng mỗi {app, shop} pair
- ShopName
- **Region** (string: "GLOBAL", "US", "EU"...)
- **BaseApiUrl** (string, derive từ Region khi connect)
- AccessToken (encrypted via IDataProtectionProvider)
- RefreshToken (encrypted)
- TokenExpiresAt
- Status (enum TikTokShopConnectionStatus)
- LastSyncedAt (cho polling reconciliation)
- LastWebhookAt (timestamp event cuối nhận được)
- Kế thừa BaseEntity.

### Unique constraint
- `(TenantId, ShopId)` — 1 tenant không connect 1 shop 2 lần.

### Sub-tasks
- Entity + Configuration + Migration.
- Encrypt tokens bằng IDataProtectionProvider, key persist trong volume Docker.

## Task 7.1.2 — OAuth flow

### Endpoints
- `GET /api/tiktok-shops/auth-url?redirectAfter=...` 
  - Tạo state (GUID), lưu vào cache (Redis hoặc in-memory với expiry 10 phút).
  - Trả URL: `https://services.tiktokshop.com/open/authorize?app_key={key}&state={state}` (verify exact URL từ doc).
- `GET /api/tiktok-shops/callback?code=...&state=...`
  - Verify state khớp cache (chống CSRF) + chưa expire.
  - Exchange code → access_token + refresh_token (call `POST /api/v2/token/get`).
  - **Quan trọng:** sau đó gọi `GET /authorization/202309/shops` để lấy danh sách shops + **shop_cipher** cho từng shop.
  - Lưu mỗi shop thành 1 TikTokShopConnection.
  - Redirect FE về `{redirectAfter}?connected=true&shopCount=N`.
- `GET /api/tiktok-shops` — list connection của tenant.
- `DELETE /api/tiktok-shops/{id}` — soft delete + log audit.
- `POST /api/tiktok-shops/{id}/refresh-token` (manual trigger, Admin only).

### DoD
- Click "Kết nối shop" → redirect TikTok → grant → quay về app, thấy shop với shop_cipher đã lưu (mã hóa trong DB).

## Task 7.1.3 — Refresh token background job

### Sub-tasks
- `TikTokTokenRefreshService : BackgroundService` chạy mỗi 30 phút.
- Tìm connection có `TokenExpiresAt < now + 2 hour` và `Status = Active`.
- Gọi refresh endpoint, update token mới.
- Fail (token revoked phía TikTok) → set Status = Expired, gửi notification cho admin tenant.

---

# PHASE 7.2: Webhook Receiver

**Mục tiêu:** Nhận event TikTok real-time, verify signature, queue async để xử lý sau.

## Task 7.2.1 — Entity WebhookEvent

### Fields
- Id, TenantId (resolve từ shop_id trong payload), ConnectionId (FK, nullable nếu không match được shop)
- EventId (TikTok trả về, dùng làm idempotency key)
- EventType (string: "ORDER_STATUS_CHANGE", "RETURN_STATUS_CHANGE", ...)
- Payload (jsonb chứa full body)
- ReceivedAt
- ProcessedAt?
- Status (enum WebhookEventStatus: Received, Processing, Processed, Failed, Skipped)
- RetryCount, LastError?
- Kế thừa BaseEntity (KHÔNG soft delete).

### Index
- Unique `(EventId)` — tránh xử lý trùng.
- `(Status, ReceivedAt)` — query queue.
- `(TenantId, ConnectionId, ReceivedAt DESC)` — query lịch sử.

## Task 7.2.2 — Webhook receiver endpoint

### Endpoint
- `POST /api/webhooks/tiktok` — **PUBLIC** (không [Authorize]) nhưng phải verify HMAC.

### Logic
```csharp
[AllowAnonymous]
[HttpPost("/api/webhooks/tiktok")]
public async Task<IActionResult> Receive(CancellationToken ct)
{
    // 1. Đọc body raw (KHÔNG bind model trước khi verify)
    using var reader = new StreamReader(Request.Body);
    var body = await reader.ReadToEndAsync();
    
    // 2. Verify HMAC signature (xem CLAUDE.md §12.5.6)
    if (!_signatureVerifier.Verify(Request, body))
    {
        _logger.LogWarning("Invalid webhook signature from {Ip}", HttpContext.Connection.RemoteIpAddress);
        return Unauthorized();
    }
    
    // 3. Parse minimal info để route
    var payload = JsonSerializer.Deserialize<TikTokWebhookEnvelope>(body);
    
    // 4. Tìm connection theo shop_id (hoặc shop_cipher trong payload)
    var connection = await _connectionRepo.FindByShopIdAsync(payload.ShopId);
    if (connection is null)
    {
        _logger.LogWarning("Webhook for unknown shop {ShopId}, skipping", payload.ShopId);
        return Ok(); // Trả 200 để TikTok không retry
    }
    
    // 5. Idempotency check: event này đã được nhận chưa?
    if (await _webhookRepo.ExistsAsync(payload.EventId))
    {
        return Ok();
    }
    
    // 6. Persist event
    await _webhookRepo.CreateAsync(new WebhookEvent {
        TenantId = connection.TenantId,
        ConnectionId = connection.Id,
        EventId = payload.EventId,
        EventType = payload.Type,
        Payload = body,
        Status = WebhookEventStatus.Received,
        ReceivedAt = DateTimeOffset.UtcNow
    });
    
    // 7. Update connection.LastWebhookAt
    connection.LastWebhookAt = DateTimeOffset.UtcNow;
    await _db.SaveChangesAsync(ct);
    
    // 8. Trả 200 NGAY (xử lý async ở background)
    return Ok();
}
```

### Sub-tasks
- `ITikTokWebhookSignatureVerifier` (Infrastructure) — implement HMAC-SHA256 verify theo công thức TikTok.
- Allow anonymous + bypass authentication middleware cho route này.

### DoD
- Gửi test webhook từ TikTok Developer Portal → endpoint nhận, verify pass, persist vào DB.
- Gửi cùng event_id 2 lần → chỉ persist 1 lần.
- Gửi với signature sai → 401.

## Task 7.2.3 — Background processor

### Sub-tasks
- `WebhookProcessorService : BackgroundService` chạy mỗi 5 giây hoặc trigger sau khi insert webhook event (channel pattern).
- Lấy 10-20 events `Status = Received`, set thành Processing, gọi handler theo EventType:
  - `ORDER_STATUS_CHANGE` → `IOrderEventHandler.HandleAsync(payload)`
  - `RETURN_STATUS_CHANGE` → `IReturnEventHandler.HandleAsync(payload)`
  - `authorization.removed` → `IAuthorizationEventHandler.HandleAsync(payload)`
  - Khác → log skip.
- Try-catch mỗi event: success → Status = Processed. Fail → RetryCount++, LastError, status quay về Received nếu < maxRetry, sau đó Failed.

### DoD
- Webhook ORDER_STATUS_CHANGE = 111 (AwaitingShipment) → handler tạo Reservation đúng.

## Task 7.2.4 — Webhook events admin page (FE)

### Endpoint
- `GET /api/webhook-events?pageNumber=...&status=...&eventType=...` (Admin only)
- `POST /api/webhook-events/{id}/retry` (Admin only) — retry event Failed.

### FE
- Trang `/admin/webhook-events`: bảng lịch sử, filter status, button retry. Hữu ích khi debug sync issue.

---

# PHASE 7.3: Polling Reconciliation

**Mục tiêu:** Backup cho webhook khi miss event (TikTok có thể fail delivery, hoặc app downtime). Tần suất thấp hơn webhook nhưng đảm bảo eventual consistency.

## Task 7.3.1 — Per-shop rate limiter

### Sub-tasks
- `ITikTokRateLimiter` với token bucket per shop_cipher (50 token/sec, refill 50/sec).
- Mọi call qua `ITikTokApiClient` đều phải `await _rateLimiter.AcquireAsync(shopCipher)` trước khi gọi.
- Implement bằng `System.Threading.RateLimiting` (built-in .NET 7+) hoặc thư viện `AsyncRateLimiter`.

### DoD
- Bắn 100 request song song cho 1 shop → request bị throttle về 50/sec, không bị TikTok trả 429.

## Task 7.3.2 — ITikTokApiClient hoàn chỉnh

### Sub-tasks
- Typed HttpClient với:
  - Polly retry 3 lần exponential backoff (chỉ retry 5xx, 429).
  - Polly circuit breaker.
  - Auto-sign mỗi request (timestamp, sign, app_key, shop_cipher).
  - Rate limiter wrap mỗi call.
  - Auto-refresh access_token nếu detect 401 với code "invalid_access_token".

### Methods
- `GetShopInfoAsync(connectionId)`
- `GetOrdersAsync(connectionId, OrderQueryParams)` — cursor-based, hỗ trợ `update_time_ge`.
- `GetOrderDetailAsync(connectionId, orderId)`
- `GetReturnsAsync(connectionId, ReturnQueryParams)`
- `GetReturnDetailAsync(connectionId, returnId)`
- `GetProductsAsync(connectionId, ProductQueryParams)`
- `UpdateInventoryAsync(connectionId, skuId, quantity)` — Phase 7.7
- `GetFinanceStatementsAsync(connectionId, dateRange)` — Phase 7.8
- `GetVideosAsync(connectionId, params)` — Phase 7.9

## Task 7.3.3 — Polling jobs

### Sub-tasks
- `OrderReconciliationService : BackgroundService` chạy mỗi 30 phút.
- Logic:
  ```
  Foreach connection (Status = Active):
    cursor = null
    orders_updated_after = max(connection.LastSyncedAt - 1 hour, now - 7 days)
    // -1h: overlap để safe net; -7d: max range fallback
    
    do {
      resp = client.GetOrders({page_token: cursor, update_time_ge: orders_updated_after, page_size: 50})
      foreach order in resp.data.orders:
        // Process same as webhook handler (idempotent)
        EnqueueAsWebhookEvent(eventType=ORDER_STATUS_CHANGE, payload=order, source=polling)
      cursor = resp.data.next_page_token
    } while cursor != null
    
    connection.LastSyncedAt = now
  ```
- Polling chỉ enqueue vào `WebhookEvents` table với source = "polling" → reuse processor.
- Idempotent vì processor check EventId (synthesize từ orderId + status code khi từ polling).

### DoD
- Disable webhook 1 giờ, tạo order trên TikTok → sau 30 phút polling chạy → order vẫn được xử lý đúng.

---

# PHASE 7.4: Product Mapping

**Mục tiêu:** UI để map TikTok SKU ↔ Product nội bộ. Backend support fuzzy suggestion.

## Task 7.4.1 — Entity ProductTikTokMapping

(Giữ như đã thiết kế ở plan trước — không thay đổi)

### Fields
- Id, TenantId, ProductId (FK), ConnectionId (FK), TikTokProductId, TikTokSkuId, TikTokSkuName
- Kế thừa BaseEntity.

### Constraints
- Unique `(TenantId, ConnectionId, TikTokProductId, TikTokSkuId)`.
- Index `(TenantId, ConnectionId, TikTokSkuId)` — lookup khi xử lý order.

## Task 7.4.2 — Endpoints

- `GET /api/product-mappings?connectionId=...&productId=...&search=...`
- `POST /api/product-mappings` — body `{productId, connectionId, tikTokProductId, tikTokSkuId, tikTokSkuName}`.
- `DELETE /api/product-mappings/{id}`.
- `GET /api/product-mappings/tiktok-skus?connectionId=...&search=...` — proxy gọi TikTok GetProducts API, trả list SKU với pagination.
- `GET /api/product-mappings/suggest?connectionId=...&productId=...` — fuzzy match by name/code, trả top 5 candidates.

## Task 7.4.3 — UI mapping page

### Sub-tasks
- `/settings/tiktok-shops/{connectionId}/mappings`:
  - 2 cột: Products nội bộ (trái) | TikTok SKUs (phải).
  - Mỗi product hiển thị mappings hiện có + button "Thêm mapping".
  - Click "Thêm mapping" → modal hiện list TikTok SKUs (search được) + suggestion gợi ý theo name.
- Bulk import mappings (optional): upload CSV mapping.

### DoD
- User thấy danh sách SKU TikTok của shop đã connect → map từng cái với product nội bộ → DB lưu đúng.

---

# PHASE 7.5: Order Handler (Reservation + Commit)

**Mục tiêu:** Logic chính: nhận event order → tạo reservation (khi paid) → commit thành StockMovement (khi shipped).

## Task 7.5.1 — Entity TikTokOrder & TikTokOrderItem

(Cập nhật so với plan cũ)

### TikTokOrder
- Id, TenantId, ConnectionId
- TikTokOrderId (string)
- StatusCode (int, dùng enum TikTokOrderStatus)
- StatusUpdatedAt (DateTimeOffset)
- BuyerName, BuyerEmail
- TotalAmount, Currency
- CreatedTime (TikTok), PaidTime?, ShippedTime?, DeliveredTime?, CancelledTime?
- RawJson (jsonb)
- Kế thừa BaseEntity.

### TikTokOrderItem
- Id, TenantId, OrderId (FK)
- TikTokProductId, TikTokSkuId, TikTokProductName
- Quantity, UnitPrice, TotalAmount
- MappedProductId (FK Product, nullable)
- SyncStatus (enum TikTokOrderSyncStatus)
- Kế thừa BaseEntity.

### Constraints
- Unique `(TenantId, ConnectionId, TikTokOrderId)` cho Order.
- Unique `(TenantId, OrderId, TikTokSkuId)` cho Item.

## Task 7.5.2 — IOrderEventHandler

### Pseudo-code
```csharp
public async Task HandleAsync(OrderStatusChangePayload payload, CancellationToken ct)
{
    using var tx = await _db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);
    
    // 1. Fetch order detail từ TikTok (payload thường chỉ có order_id + status)
    var orderDetail = await _tikTokClient.GetOrderDetailAsync(payload.ShopCipher, payload.OrderId, ct);
    
    // 2. Upsert TikTokOrder + Items
    var order = await UpsertOrderAsync(orderDetail);
    
    // 3. Resolve mapping cho từng item (tạo mới hoặc dùng lại)
    foreach (var item in order.Items)
    {
        if (item.MappedProductId is null)
        {
            var mapping = await ResolveMappingAsync(item, order.ConnectionId);
            if (mapping is null)
            {
                item.SyncStatus = TikTokOrderSyncStatus.MappingPending;
                await EnqueueUnresolvedAsync(item);
                continue;  // skip item này, các item khác vẫn xử lý
            }
            item.MappedProductId = mapping.ProductId;
        }
        
        // 4. Quyết định action theo statusCode + currentSyncStatus
        await ProcessItemAsync(item, order, ct);
    }
    
    await _db.SaveChangesAsync(ct);
    await tx.CommitAsync(ct);
}

private async Task ProcessItemAsync(TikTokOrderItem item, TikTokOrder order, CancellationToken ct)
{
    var reservationKey = $"reservation:{order.ConnectionId}:{order.TikTokOrderId}:{item.Id}";
    var outKey = $"tiktok-out:{order.ConnectionId}:{order.TikTokOrderId}:{item.Id}";
    
    switch ((TikTokOrderStatus)order.StatusCode)
    {
        case TikTokOrderStatus.AwaitingShipment:  // 111
            // Tạo reservation, KHÔNG sinh movement
            await _reservationService.CreateAsync(
                productId: item.MappedProductId!.Value,
                quantity: item.Quantity,
                tikTokOrderItemId: item.Id,
                idempotencyKey: reservationKey,
                ct: ct);
            item.SyncStatus = TikTokOrderSyncStatus.Reserved;
            break;
            
        case TikTokOrderStatus.AwaitingCollection:  // 112
            // Vẫn reservation, không action (đợi InTransit)
            break;
            
        case TikTokOrderStatus.InTransit:           // 121
        case TikTokOrderStatus.Delivered:           // 122
        case TikTokOrderStatus.Completed:           // 130
            // Commit reservation + sinh StockMovement Out
            // Idempotency: gọi 2 lần đều OK
            await _reservationService.CommitAsync(reservationKey, ct);
            await _stockMovementService.RecordAsync(
                productId: item.MappedProductId!.Value,
                type: StockMovementType.Out,
                source: StockMovementSource.TikTokOrder,
                quantity: item.Quantity,
                unitCost: item.UnitPrice,
                occurredAt: order.ShippedTime ?? DateTimeOffset.UtcNow,
                idempotencyKey: outKey,
                reference: new(TikTokOrderItemId: item.Id),
                ct: ct);
            item.SyncStatus = TikTokOrderSyncStatus.StockApplied;
            // Enqueue outbox push inventory về TikTok (Phase 7.7)
            await _outbox.EnqueuePushInventoryAsync(item.MappedProductId.Value, ct);
            break;
            
        case TikTokOrderStatus.Cancelled:           // 140
            // Tách 2 case:
            // - Cancel trước khi ship: release reservation
            // - Cancel sau khi ship (rare): sinh compensating In movement
            await HandleCancelAsync(item, order, reservationKey, outKey, ct);
            break;
    }
}
```

### Sub-tasks
- IOrderEventHandler implementation
- ResolveMappingAsync helper
- HandleCancelAsync helper (xem Phase 7.6)

### DoD
- Event AwaitingShipment → Reservation Active, AvailableStock giảm.
- Event InTransit → Reservation Committed + StockMovement Out, PhysicalStock giảm.
- Replay event → idempotent (số liệu không đổi).

## Task 7.5.3 — Unresolved queue + UI

### Endpoints
- `GET /api/unresolved-order-items?pageNumber=...`
- `POST /api/unresolved-order-items/{itemId}/resolve` body `{productId, createMapping: bool}`.
- `GET /api/unresolved-order-items/count`.

### FE
- Trang `/unresolved-orders`: bảng list, mỗi row có select product + checkbox "Tạo mapping" + button Resolve.
- Sau resolve → re-process item (tạo reservation hoặc commit ngay tùy status hiện tại).
- Badge sidebar hiển thị số lượng unresolved.

---

# PHASE 7.6: Cancel & Return Handler

**Mục tiêu:** Xử lý đúng 2 luồng phức tạp: cancel (trước/sau ship) và return (sau khi hoàn thành).

## Task 7.6.1 — Cancel handler

### Logic
```csharp
private async Task HandleCancelAsync(item, order, reservationKey, outKey, ct)
{
    // Trường hợp 1: chưa commit (chưa ship) → release reservation
    var reservation = await _reservationService.FindByKeyAsync(reservationKey);
    if (reservation?.Status == InventoryReservationStatus.Active)
    {
        await _reservationService.ReleaseAsync(reservationKey, ct);
        item.SyncStatus = TikTokOrderSyncStatus.StockReversed;
        return;
    }
    
    // Trường hợp 2: đã commit (đã ship rồi cancel — hiếm, thường chỉ sau khi return)
    // Sinh compensating In movement
    var reverseKey = $"tiktok-cancel-reverse:{order.ConnectionId}:{order.TikTokOrderId}:{item.Id}";
    await _stockMovementService.RecordAsync(
        productId: item.MappedProductId.Value,
        type: StockMovementType.In,
        source: StockMovementSource.TikTokOrder,
        quantity: item.Quantity,
        unitCost: item.UnitPrice,
        occurredAt: order.CancelledTime ?? DateTimeOffset.UtcNow,
        idempotencyKey: reverseKey,
        reference: new(TikTokOrderItemId: item.Id),
        note: $"Reverse for late cancel: order {order.TikTokOrderId}",
        ct: ct);
    item.SyncStatus = TikTokOrderSyncStatus.StockReversed;
    await _outbox.EnqueuePushInventoryAsync(item.MappedProductId.Value, ct);
}
```

## Task 7.6.2 — Return entities

### Entity TikTokReturn
- Id, TenantId, ConnectionId
- TikTokReturnId (unique), TikTokOrderId (ref)
- ReturnStatus (enum TikTokReturnStatus)
- ReturnReason, ReturnReasonCode
- RequestedAt, ApprovedAt?, ReceivedAt?, RefundedAt?
- RefundAmount
- RawJson
- Kế thừa BaseEntity.

### Entity TikTokReturnLine
- Id, TenantId, ReturnId (FK), TikTokOrderItemId (FK to original order item)
- Quantity (số trả về, có thể nhỏ hơn order quantity)
- RefundAmount
- Kế thừa BaseEntity.

## Task 7.6.3 — IReturnEventHandler

### Logic
```csharp
// Khi nhận RETURN_STATUS_CHANGE webhook:
public async Task HandleAsync(ReturnStatusChangePayload payload, CancellationToken ct)
{
    // 1. Fetch return detail
    var returnDetail = await _tikTokClient.GetReturnDetailAsync(...);
    
    // 2. Upsert TikTokReturn + TikTokReturnLine
    
    // 3. Process theo return status
    if (returnDetail.Status == TikTokReturnStatus.ReturnReceived)
    {
        // Hàng đã nhận về kho → sinh StockMovement Type=ReturnIn
        foreach (var line in returnDetail.Lines)
        {
            await _stockMovementService.RecordAsync(
                productId: line.OriginalItem.MappedProductId,
                type: StockMovementType.ReturnIn,
                source: StockMovementSource.TikTokReturn,
                quantity: line.Quantity,
                unitCost: line.OriginalItem.UnitPrice,
                occurredAt: returnDetail.ReceivedAt,
                idempotencyKey: $"tiktok-return-in:{shopCipher}:{returnId}:{lineId}",
                reference: new(TikTokReturnLineId: line.Id),
                ct: ct);
            
            await _outbox.EnqueuePushInventoryAsync(line.OriginalItem.MappedProductId, ct);
        }
    }
    // Status khác (Requested, Approved, Rejected) — chỉ update record, không sinh movement
}
```

### DoD
- Khách return 2/5 sản phẩm → hàng về kho → PhysicalStock tăng 2.
- Return rejected → không sinh movement.

## Task 7.6.4 — UI returns dashboard

### Endpoint
- `GET /api/tiktok-returns?pageNumber=...&status=...&connectionId=...`
- `GET /api/tiktok-returns/{id}` — detail với original order info.

### FE
- Trang `/tiktok-returns`: bảng list returns, filter status, link sang order gốc.

---

# PHASE 7.7: Push Inventory to TikTok (Outbox Pattern)

**Mục tiêu:** Mọi thay đổi tồn kho → tự động đẩy số mới lên TikTok để TikTok ngừng bán khi hết hàng.

> **Đọc lại CLAUDE.md §12.5.10 — Outbox pattern.**

## Task 7.7.1 — Entity OutboxMessage

### Fields
- Id, TenantId
- Type (string: "PushInventory", có thể mở rộng sau)
- Payload (jsonb)
- Status (enum OutboxStatus: Pending, Processed, Failed)
- RetryCount, NextAttemptAt
- LastError?
- ProcessedAt?
- Kế thừa BaseEntity.

### Index
- `(Status, NextAttemptAt)` — query dispatcher.
- `(TenantId, Type, CreatedAt DESC)`.

## Task 7.7.2 — Integration với StockMovementService

### Sub-tasks
- Sửa `StockMovementService.RecordAsync`: sau khi insert movement (cùng transaction), nếu Source ∈ {Manual, TikTokOrder, TikTokReturn, Adjustment} → insert OutboxMessage:
  ```json
  {
    "type": "PushInventory",
    "payload": {
      "productId": "...",
      "tenantId": "..."
    }
  }
  ```
- Cùng transaction đảm bảo: nếu movement save OK thì outbox cũng có; nếu rollback thì cả 2 không có.

## Task 7.7.3 — OutboxDispatcher service

### Sub-tasks
- `OutboxDispatcherService : BackgroundService` chạy mỗi 5 giây.
- Logic:
  ```
  messages = SELECT TOP 50 FROM OutboxMessages 
             WHERE Status = Pending AND (NextAttemptAt IS NULL OR NextAttemptAt < now)
             ORDER BY CreatedAt
             FOR UPDATE SKIP LOCKED  -- chống multiple instance race
  
  foreach msg in messages:
    try {
      switch (msg.Type):
        case "PushInventory":
          await HandlePushInventoryAsync(msg)
      msg.Status = Processed
      msg.ProcessedAt = now
    } catch (Exception ex) {
      msg.RetryCount++
      msg.LastError = ex.Message
      msg.NextAttemptAt = now + ExponentialBackoff(RetryCount)  // 5s, 25s, 2m, 10m, 1h
      if (RetryCount >= 5) msg.Status = Failed
    }
  ```

### HandlePushInventoryAsync
```csharp
private async Task HandlePushInventoryAsync(OutboxMessage msg)
{
    var payload = JsonSerializer.Deserialize<PushInventoryPayload>(msg.Payload);
    
    // 1. Tính AvailableStock hiện tại
    var available = await _inventoryService.GetAvailableStockAsync(payload.ProductId);
    
    // 2. Tìm tất cả mappings của product này → push cho từng shop+SKU
    var mappings = await _mappingRepo.GetByProductIdAsync(payload.ProductId);
    
    foreach (var mapping in mappings)
    {
        var connection = await _connectionRepo.GetByIdAsync(mapping.ConnectionId);
        if (connection.Status != TikTokShopConnectionStatus.Active) continue;
        
        await _tikTokClient.UpdateInventoryAsync(
            connection: connection,
            tiktokSkuId: mapping.TikTokSkuId,
            quantity: Math.Max(0, available),  // không push số âm
            ct: ct);
    }
}
```

### DoD
- Tạo StockMovement Out → trong 5s, OutboxMessage được dispatch → TikTok API được gọi với quantity mới.
- TikTok API fail → retry với backoff → cuối cùng success hoặc Failed sau 5 lần.

## Task 7.7.4 — Outbox admin UI

### Endpoint
- `GET /api/outbox?status=...&type=...&pageNumber=...` (Admin)
- `POST /api/outbox/{id}/retry` — reset RetryCount, Status = Pending.

### FE
- Trang `/admin/outbox`: list, filter, retry button. Đặc biệt hữu ích khi TikTok API down.

---

# PHASE 7.8: Finance API Integration

**Mục tiêu:** Lợi nhuận thực = (doanh thu bán) - (giá vốn) - (phí TikTok). Cần lấy phí từ Finance API.

## Task 7.8.1 — Entities

### TikTokFinanceStatement
- Id, TenantId, ConnectionId
- StatementId (unique từ TikTok), StatementTime, SettlementAmount, Currency
- Status (string từ TikTok: SETTLED, PENDING, ...)
- RawJson
- Kế thừa BaseEntity.

### TikTokOrderFinance (per-order breakdown)
- Id, TenantId, TikTokOrderId (FK)
- StatementId (FK, nullable)
- GrossRevenue, PlatformFee, PaymentFee, ShippingFee, ShippingFeeSubsidy, AffiliateCommission, OtherFees
- NetRevenue (= GrossRevenue - all fees)
- SettledAt?
- RawJson
- Kế thừa BaseEntity.

## Task 7.8.2 — Finance sync job

### Sub-tasks
- `FinanceSyncService : BackgroundService` chạy mỗi 6 giờ (Finance data update chậm, không cần frequent).
- Per connection:
  - `GET /finance/statement/list?statement_time_ge=...` (cursor pagination)
  - Per statement: `GET /finance/statement/transaction/list?statement_id=...` để lấy breakdown per-order.
- Upsert vào tables, idempotent qua StatementId.

### DoD
- Sau khi shop có settlement → DB có TikTokOrderFinance với breakdown chi tiết.

## Task 7.8.3 — Cập nhật Dashboard

### Endpoint
- `GET /api/dashboard/overview?from=...&to=...&useNetRevenue=true`
- Khi useNetRevenue = true:
  - Revenue = SUM(TikTokOrderFinance.NetRevenue) + SUM(Manual StockOut revenue)
  - Profit = Revenue - SUM(StockMovement.UnitCost * Quantity) cho Out movements

### Dashboard mới: Fee breakdown
- `GET /api/dashboard/tiktok/fee-breakdown?from=...&to=...`
- Trả: tổng phí theo loại (Platform, Payment, Shipping, Commission, Other).

### FE
- Dashboard có toggle "Gross/Net" để switch chế độ tính.
- Card mới: "Tổng phí TikTok" với pie chart breakdown.

---

# PHASE 7.9: Video Sync & TikTok Dashboard

**Mục tiêu:** Hoàn thiện dashboard TikTok với phần content analytics.

## Task 7.9.1 — Video entities

### TikTokVideo
- Id, TenantId, ConnectionId
- TikTokVideoId, Title, ThumbnailUrl, VideoUrl
- PostedAt, LastSyncedAt
- Kế thừa BaseEntity.

### TikTokVideoMetric (snapshot theo thời gian — vẽ growth chart)
- Id, VideoId, Views, Likes, Shares, Comments, CapturedAt
- Kế thừa BaseEntity (không soft delete cho metrics — append only).

## Task 7.9.2 — Video sync job

- `VideoSyncService : BackgroundService` mỗi 2 giờ.
- Per connection: fetch list videos, upsert TikTokVideo, insert TikTokVideoMetric snapshot mới.

## Task 7.9.3 — TikTok Dashboard endpoints

- `GET /api/dashboard/tiktok/overview?connectionId=...&from=...&to=...` — KPI tổng hợp (GMV, đơn, AOV, net revenue, fees).
- `GET /api/dashboard/tiktok/orders-by-day`
- `GET /api/dashboard/tiktok/top-videos`
- `GET /api/dashboard/tiktok/video-growth/{videoId}` — chart from TikTokVideoMetric.
- `GET /api/dashboard/tiktok/orders-by-video` (nếu TikTok API hỗ trợ attribution — không phải region nào cũng có).

## Task 7.9.4 — FE TikTok dashboard tab

- Tab "TikTok" trong Dashboard.
- Shop selector (nếu nhiều shop).
- KPI cards: GMV, đơn, AOV, fees, net.
- Charts: revenue-by-day (stacked theo shop), top videos, fee breakdown.
- Bảng video performance.
- Click video → modal/drawer xem growth chart.
- Indicator "Đồng bộ X giờ trước" nếu `LastSyncedAt > 6h`.

---

# PHASE 8: Testing, Docker, Polish

**Mục tiêu:** Sẵn sàng deploy & maintain.

## Task 8.1 — Unit tests

### Sub-tasks
- Test services: Auth, Product, Supplier, StockIn/Out, Inventory, Dashboard.
- Test validators.
- Test mapping (Mapster).

### DoD
- Coverage Application layer ≥ 70%.

## Task 8.2 — Integration tests

### Sub-tasks
- `WebApplicationFactory` + Testcontainers PostgreSQL.
- Test flow: register → login → CRUD products → inventory đúng.

## Task 8.3 — Dockerize Frontend

### Sub-tasks
- `frontend/Dockerfile` multi-stage (node build → nginx serve).
- `nginx.conf`: serve static, proxy `/api` về backend service trong compose.
- Update `docker-compose.yml` thêm service `frontend`.

### DoD
- `docker compose up` chạy cả stack, mở `http://localhost` thấy app.

## Task 8.4 — README & docs

### Sub-tasks
- `README.md` root:
  - Mô tả ngắn.
  - Yêu cầu (Docker, hoặc .NET 9 + Node 20 + Postgres).
  - Hướng dẫn chạy local (3 cách: full Docker, BE local + DB Docker, full local).
  - Hướng dẫn cấu hình `.env`.
  - Trỏ tới `CLAUDE.md` và `Plan.md`.
- API doc (link Swagger).
- Diagram kiến trúc (optional, dùng mermaid trong README).

## Task 8.5 — Hardening

### Sub-tasks
- Rate limit endpoint auth (`AddRateLimiter` built-in .NET 8+).
- CORS cấu hình đúng domain FE.
- HTTPS redirect bật ở Prod.
- Health check endpoint `/health` (DB ping).
- Health check Postgres trong compose.
- Backup DB script (pg_dump cron job — optional).

## Task 8.6 — Polish UI

### Sub-tasks
- Loading skeleton thay vì spinner trắng.
- Empty state đẹp.
- Mobile responsive (chí ít login/dashboard).
- Dark mode toggle (optional, Zustand + antd ConfigProvider).
- i18n VI/EN (optional, `react-i18next`).

## Task 8.7 — Pre-launch checklist

- [ ] Tất cả secret đã chuyển sang env, không hardcode.
- [ ] JWT secret production khác dev, ≥ 64 ký tự random.
- [ ] HTTPS bật ở Prod.
- [ ] Log không leak nhạy cảm (xem lại Serilog config).
- [ ] Global query filter tenant áp dụng đúng (thử query cross-tenant phải fail).
- [ ] Soft delete hoạt động đúng (record bị xóa không xuất hiện).
- [ ] Tất cả endpoint nhạy cảm có `[Authorize]`.
- [ ] CORS chỉ cho domain FE.
- [ ] Test xong end-to-end các user flow chính.
- [ ] Backup DB chiến lược (nếu deploy thật).

---

# PHỤ LỤC A — Sơ đồ Entity Relationship (tóm tắt)

```
Tenant 1 ─── n User
Tenant 1 ─── n Product
Tenant 1 ─── n Supplier
Tenant 1 ─── n StockIn ──→ Product, Supplier
Tenant 1 ─── n StockOut ─→ Product
Tenant 1 ─── n StockMovement (append-only) ──→ Product
                            ↑─── StockIn / StockOut / TikTokOrderItem / TikTokReturnLine (qua FK nullable)
Tenant 1 ─── n InventoryReservation ──→ Product, TikTokOrderItem
Tenant 1 ─── n OutboxMessage (queue async tasks)
Tenant 1 ─── n WebhookEvent (incoming events từ TikTok)
Tenant 1 ─── n TikTokShopConnection (có shop_cipher, region, baseApiUrl)
TikTokShopConnection 1 ─── n TikTokOrder ─── n TikTokOrderItem
TikTokShopConnection 1 ─── n TikTokReturn ─── n TikTokReturnLine ──→ TikTokOrderItem
TikTokShopConnection 1 ─── n TikTokVideo ─── n TikTokVideoMetric (snapshot theo thời gian)
TikTokShopConnection 1 ─── n ProductTikTokMapping ──→ Product
TikTokShopConnection 1 ─── n TikTokFinanceStatement
TikTokFinanceStatement 1 ─── n TikTokOrderFinance ──→ TikTokOrder
TikTokOrderItem n ──── 1 Product (qua MappedProductId, nullable)
User 1 ─── n RefreshToken
```

**Luồng tồn kho hoàn chỉnh:**
```
┌─────────────────────────────────────────────────────────────────────┐
│  WEBHOOK / POLLING                                                  │
└────────────────────────────┬────────────────────────────────────────┘
                             ▼
        ┌────────────────────────────────────┐
        │  WebhookEvent (persisted, async)    │
        └────────────────┬───────────────────┘
                         ▼
        ┌────────────────────────────────────┐
        │  Processor (IOrderEventHandler)    │
        └────────────────┬───────────────────┘
                         ▼
   ┌─────────────────────┴──────────────────────┐
   │                                            │
   ▼ AwaitingShipment (111)        ▼ InTransit/Delivered (121/122)
┌──────────────────┐         ┌─────────────────────────┐
│ Reservation      │         │ StockMovement Out       │
│ (Active)         │         │ + Commit Reservation    │
│ AvailableStock ↓ │         │ PhysicalStock ↓         │
│ PhysicalStock = │         │ + Outbox: PushInventory │
└──────────────────┘         └────────────┬────────────┘
                                          ▼
                              ┌────────────────────────┐
                              │ OutboxDispatcher       │
                              │ → TikTok UpdateInventory│
                              └────────────────────────┘

   ▼ Cancelled (140)                   ▼ Return received
┌──────────────────────────┐    ┌──────────────────────────┐
│ Active → Release         │    │ StockMovement Type=ReturnIn│
│ HOẶC compensating         │    │ PhysicalStock ↑          │
│ StockMovement In (rev)   │    │ + Outbox: PushInventory  │
└──────────────────────────┘    └──────────────────────────┘
```

**Bán tay (Manual):**
```
StockOut Create → check AvailableStock (lock) → StockMovement Out → Outbox PushInventory
StockIn Create → StockMovement In → Outbox PushInventory
```

# PHỤ LỤC B — Lệnh tham khảo

```bash
# Tạo solution & projects
dotnet new sln -n TikTokShop
dotnet new classlib -n TikTokShop.Domain -o src/TikTokShop.Domain
dotnet new classlib -n TikTokShop.Application -o src/TikTokShop.Application
dotnet new classlib -n TikTokShop.Infrastructure -o src/TikTokShop.Infrastructure
dotnet new webapi -n TikTokShop.Api -o src/TikTokShop.Api
dotnet sln add src/**/*.csproj

# References
dotnet add src/TikTokShop.Application reference src/TikTokShop.Domain
dotnet add src/TikTokShop.Infrastructure reference src/TikTokShop.Application
dotnet add src/TikTokShop.Api reference src/TikTokShop.Infrastructure src/TikTokShop.Application

# EF Migration (chạy ở thư mục Api)
dotnet ef migrations add InitialCreate -p ../TikTokShop.Infrastructure -s .
dotnet ef database update -p ../TikTokShop.Infrastructure -s .

# Frontend
npm create vite@latest frontend -- --template react-ts
cd frontend && npm install
```

# PHỤ LỤC C — Thứ tự gọi Claude (gợi ý)

1. **Ngày 1 (parallel):** Bắt đầu Phase Pre đăng ký TikTok app NGAY trên Partner Center (chờ duyệt 3-7 ngày).
2. "Theo CLAUDE.md & Plan.md, làm Phase 0 Task 0.1 — tạo solution structure."
3. Review code → commit.
4. "Tiếp Phase 0 Task 0.2 — cài packages."
5. ... cứ thế tuần tự Phase 0 → 1 → 2 → 3 → 3.5 → 4 → 5 → 6.
6. **Trước Phase 7:** verify TikTok app đã approve scopes cần thiết, đã có sandbox (nếu region hỗ trợ).
7. Phase 7: làm THEO THỨ TỰ 7.1 → 7.2 → 7.3 → 7.4 → 7.5 → 7.6 → 7.7 → 7.8 → 7.9. KHÔNG nhảy bước vì các phase phụ thuộc nhau.
8. Khi sang Phase mới, luôn nhắc Claude: "Đọc lại CLAUDE.md (đặc biệt §6.1, §6.2, §12.5) trước khi bắt đầu để giữ đúng convention."

> Mỗi lần Claude sinh code: bạn nên review (1) tenant filter, (2) soft delete / append-only, (3) authorize attribute, (4) DTO không leak, (5) idempotency key, (6) outbox insert đúng transaction, (7) webhook verify signature.

---

**Kết thúc Plan.md. Chúc vibe code vui vẻ! 🚀**
