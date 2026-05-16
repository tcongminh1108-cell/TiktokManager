# CLAUDE.md - TikTok Shop Management System

> File này cung cấp ngữ cảnh và quy tắc làm việc cho Claude (qua VS Code Claude Extension) khi vibe coding dự án này. Đọc kỹ trước khi sinh/chỉnh sửa code.

---

## 1. TỔNG QUAN DỰ ÁN

**Tên dự án:** TikTok Shop Management System  
**Mục tiêu:** Hệ thống multi-tenant quản lý nhiều shop TikTok, tồn kho, chi phí, doanh thu và dashboard phân tích.

### Đối tượng người dùng
- **Owner/Admin Tenant:** Chủ shop, quản lý toàn bộ dữ liệu tenant
- **Manager:** Quản lý vận hành (nhập/xuất hàng, xem báo cáo)
- **Staff:** Nhân viên (nhập liệu cơ bản, xem hạn chế)

### Phạm vi chức năng
1. **Multi-tenant:** Mỗi tenant có nhiều TikTok Shop accounts, dữ liệu cô lập theo `TenantId`
2. **User & Role Management:** RBAC với 3 role (Admin, Manager, Staff) trong mỗi tenant
3. **Inventory & Cost Management:** Sản phẩm, Nhà cung cấp, Nhập hàng, Bán hàng, Tồn kho
4. **Dashboard & Analytics:** Tích hợp TikTok API + thống kê nội bộ

---

## 2. KIẾN TRÚC TỔNG QUAN

### Tech Stack
| Layer | Công nghệ | Phiên bản |
|-------|-----------|-----------|
| Backend | ASP.NET Core Web API | .NET 9 |
| ORM | Entity Framework Core | 9.x |
| Database | PostgreSQL | 16+ |
| Frontend | React + TypeScript | React 18, TS 5.x |
| Build tool FE | Vite | latest |
| UI Library | Ant Design (antd) hoặc shadcn/ui | latest |
| State management | Zustand + TanStack Query | latest |
| Auth | JWT (access + refresh token) | - |
| Containerization | Docker + docker-compose | - |
| API Documentation | Swagger / OpenAPI (Scalar UI) | - |
| Logging | Serilog (console + file + Seq optional) | - |
| Mapping | Mapster (nhẹ hơn AutoMapper) | - |
| Validation | FluentValidation | - |

### Kiến trúc Backend - Clean Architecture (4 layers)

```
TikTokShop.sln
├── src/
│   ├── TikTokShop.Domain/          # Entities, Enums, Domain Exceptions, Interfaces
│   ├── TikTokShop.Application/     # Services, DTOs, Validators, Use Cases, Interfaces
│   ├── TikTokShop.Infrastructure/  # DbContext, Repositories, External services (TikTok API), JWT
│   └── TikTokShop.Api/             # Controllers, Middlewares, Filters, DI, Program.cs
└── tests/
    ├── TikTokShop.UnitTests/
    └── TikTokShop.IntegrationTests/
```

**Quy tắc phụ thuộc (Dependency Rule):**
- `Domain` → không phụ thuộc lib nào
- `Application` → chỉ phụ thuộc `Domain`
- `Infrastructure` → phụ thuộc `Application` + `Domain`
- `Api` → phụ thuộc tất cả (chỉ wire up DI)

### Kiến trúc Frontend - Feature-based

```
frontend/
├── src/
│   ├── app/                  # App shell, router, providers
│   ├── features/             # Mỗi feature 1 folder (auth, products, suppliers, ...)
│   │   └── products/
│   │       ├── api/          # Hooks gọi API (TanStack Query)
│   │       ├── components/   # Components riêng của feature
│   │       ├── pages/        # Route components
│   │       ├── types/        # TypeScript types
│   │       └── index.ts
│   ├── shared/               # Components/utils dùng chung
│   │   ├── components/
│   │   ├── hooks/
│   │   ├── lib/              # axios instance, helpers
│   │   ├── types/
│   │   └── constants/
│   ├── layouts/              # AuthLayout, MainLayout
│   └── main.tsx
```

---

## 3. QUY ƯỚC CODE BACKEND

### 3.1 Đặt tên (Naming Conventions)
- **PascalCase:** Class, Interface (có prefix `I`), Method, Property, Enum
- **camelCase:** Local variable, parameter, private field (có prefix `_` cho field)
- **UPPER_SNAKE_CASE:** Constants
- **Tên Controller:** `ProductsController` (số nhiều)
- **Tên Service:** `IProductService` / `ProductService`
- **Tên Repository:** `IProductRepository` / `ProductRepository`
- **Tên DTO:** `ProductDto`, `CreateProductRequest`, `UpdateProductRequest`, `ProductListItemDto`

### 3.2 Cấu trúc thư mục bên trong từng layer

**TikTokShop.Domain**
```
Domain/
├── Common/              # BaseEntity, IAuditable, ISoftDelete, ITenantEntity
├── Entities/            # Product, Supplier, StockIn, StockOut, User, Tenant, ...
├── Enums/               # ⚠️ ENUM RIÊNG 1 FOLDER (yêu cầu bắt buộc)
│   ├── UserRole.cs
│   ├── StockMovementType.cs
│   └── ...
├── Exceptions/          # NotFoundException, ValidationException, ForbiddenException, ...
└── Interfaces/          # IRepository, IUnitOfWork, ICurrentUser
```

**TikTokShop.Application**
```
Application/
├── Common/
│   ├── Behaviors/       # ValidationBehavior, LoggingBehavior (nếu dùng MediatR)
│   ├── Mappings/        # Mapster config
│   └── Models/          # PaginatedResult<T>, ApiResponse<T>, PageRequest
├── Features/            # Theo feature
│   └── Products/
│       ├── Dtos/
│       ├── Validators/
│       ├── IProductService.cs
│       └── ProductService.cs
└── Interfaces/          # IUnitOfWork, ITikTokApiClient (interface), IJwtService
```

**TikTokShop.Infrastructure**
```
Infrastructure/
├── Persistence/
│   ├── ApplicationDbContext.cs
│   ├── Configurations/  # IEntityTypeConfiguration<T> cho mỗi entity
│   ├── Migrations/
│   ├── Repositories/
│   └── Interceptors/    # AuditableEntityInterceptor, SoftDeleteInterceptor
├── Identity/
│   ├── JwtService.cs
│   └── PasswordHasher.cs
├── ExternalServices/
│   └── TikTok/          # TikTokApiClient, TikTokAuthService, DTOs từ TikTok
└── DependencyInjection.cs
```

**TikTokShop.Api**
```
Api/
├── Controllers/         # Mỗi controller mỏng, chỉ điều phối tới Service
├── Middlewares/         # ExceptionHandlingMiddleware, TenantResolutionMiddleware
├── Filters/             # Validation filter, Authorization filter
├── Extensions/          # ServiceCollectionExtensions, WebApplicationExtensions
├── appsettings.json
├── appsettings.Development.json
└── Program.cs
```

### 3.3 Base Entity (BẮT BUỘC)

Mọi entity nghiệp vụ kế thừa `BaseEntity`:

```csharp
public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }
}
```

**Ngoại lệ:** `Tenant`, `User` (User vẫn có TenantId nhưng quản lý hơi khác — xem chi tiết trong Plan.md).

### 3.4 Quy tắc về Tenant Isolation

- **TUYỆT ĐỐI KHÔNG** query bất cứ entity nào mà không filter `TenantId`.
- Cài đặt **Global Query Filter** trong `OnModelCreating` cho mọi entity kế thừa `BaseEntity`:
  ```csharp
  modelBuilder.Entity<Product>().HasQueryFilter(p => 
      p.TenantId == _currentUser.TenantId && !p.IsDeleted);
  ```
- `ICurrentUser` được inject vào DbContext qua DI để lấy `TenantId` từ JWT claim.
- Khi tạo entity mới, `TenantId` được tự động gán qua `AuditableEntityInterceptor` (đọc từ `ICurrentUser`).
- Endpoint super-admin (cross-tenant) phải đánh dấu rõ và bypass filter bằng `IgnoreQueryFilters()`.

### 3.5 Soft Delete

- Field `IsDeleted` mặc định `false`.
- Khi xóa: set `IsDeleted = true`, `DeletedAt`, `DeletedBy` thay vì xóa thật.
- Global Query Filter tự động loại bỏ record `IsDeleted = true`.
- Nếu cần restore: API riêng `POST /api/{resource}/{id}/restore` (chỉ Admin).
- Hard delete chỉ dùng cho dữ liệu test hoặc job dọn dẹp sau N ngày.

### 3.6 API Response Format (THỐNG NHẤT)

```json
// Success
{
  "success": true,
  "data": { ... },
  "message": null,
  "errors": null
}

// Success with pagination
{
  "success": true,
  "data": {
    "items": [...],
    "pageNumber": 1,
    "pageSize": 20,
    "totalCount": 153,
    "totalPages": 8,
    "hasPrevious": false,
    "hasNext": true
  }
}

// Error
{
  "success": false,
  "data": null,
  "message": "Validation failed",
  "errors": {
    "Name": ["Name is required"],
    "Price": ["Price must be greater than 0"]
  }
}
```

Triển khai bằng class `ApiResponse<T>` và `ApiResponseFactory` ở Application layer.

### 3.7 Pagination, Filtering, Sorting

Mọi endpoint list dùng query parameters:
```
GET /api/products?pageNumber=1&pageSize=20&sortBy=name&sortDirection=asc&search=abc&supplierId=...&minPrice=...&maxPrice=...
```

- Mặc định: `pageNumber=1`, `pageSize=20`, max `pageSize=100`.
- `sortBy` whitelist các field cho phép (chống SQL injection / lộ field).
- `sortDirection` chỉ `asc` | `desc`.
- Filter động: dùng predicate builder hoặc Specification pattern.

### 3.8 Global Exception Handling

- Custom exceptions kế thừa từ `AppException` (đặt ở `Domain/Exceptions`):
  - `NotFoundException` → 404
  - `ValidationException` → 400
  - `ForbiddenException` → 403
  - `UnauthorizedException` → 401
  - `ConflictException` → 409
  - `BusinessRuleException` → 422
- `ExceptionHandlingMiddleware` bắt mọi exception và trả `ApiResponse` chuẩn.
- Log full stack trace ở môi trường Dev, ẩn chi tiết ở Prod.
- Dùng `IExceptionHandler` (built-in .NET 8+) hoặc middleware custom.

### 3.9 Authentication & Authorization

**JWT:**
- Access token: 15-60 phút, chứa claims: `sub` (UserId), `tenant_id`, `role`, `email`, `name`.
- Refresh token: 7-30 ngày, lưu DB (bảng `RefreshTokens`), có thể revoke.
- Endpoint: `POST /api/auth/login`, `POST /api/auth/refresh`, `POST /api/auth/logout`, `POST /api/auth/register` (tenant signup).

**Authorization:**
- Đánh dấu controller/action bằng `[Authorize(Roles = "Admin,Manager")]`.
- Hoặc policy-based: `[Authorize(Policy = "RequireAdmin")]`.
- Resource-based authorization cho các thao tác đặc biệt (ví dụ Staff chỉ sửa được record mình tạo).

**Password:**
- Hash bằng BCrypt (work factor ≥ 12) hoặc ASP.NET Core Identity hasher.
- KHÔNG BAO GIỜ log password / token.

### 3.10 Quy tắc khi gọi TikTok API

- Mọi gọi TikTok đi qua `ITikTokApiClient` (Infrastructure).
- Lưu `access_token`, `refresh_token` của TikTok Shop trong bảng `TikTokShopConnections` (mã hóa bằng Data Protection API).
- Có background job (Hangfire / IHostedService) tự refresh token TikTok trước khi hết hạn.
- Retry policy bằng Polly: 3 lần, exponential backoff, circuit breaker khi TikTok lỗi.
- Rate limit: implement token bucket per shop để tránh bị TikTok block.

---

## 4. QUY ƯỚC CODE FRONTEND

### 4.1 Đặt tên
- **PascalCase:** Components, Types, Enums (`ProductListPage.tsx`, `UserRole`)
- **camelCase:** Functions, variables, hooks (`useProducts`, `fetchProducts`)
- **kebab-case:** Tên file non-component (`api-client.ts`, `format-date.ts`)
- **UPPER_SNAKE_CASE:** Constants (`API_BASE_URL`)

### 4.2 Quy tắc TypeScript
- **STRICT mode bật toàn bộ** (`strict: true` trong tsconfig).
- KHÔNG dùng `any`, dùng `unknown` rồi narrow type.
- Định nghĩa rõ type cho mọi prop, return value của hook, response API.
- Mỗi feature có file `types/index.ts` chứa interface chung.

### 4.3 Cấu trúc Component
- Functional component + hooks.
- Tách presentation và logic: container component gọi hook, presentation component nhận props.
- Tránh component > 200 dòng — tách nhỏ.
- Mỗi component 1 file, file index.ts re-export nếu cần.

### 4.4 Gọi API
- Dùng **TanStack Query (React Query) v5** cho data fetching/caching/mutation.
- Tạo `axios` instance trong `shared/lib/api-client.ts` với interceptor:
  - Request: gắn `Authorization: Bearer <access_token>`.
  - Response: nếu 401 → call refresh token → retry request, fail thì logout.
- Mỗi feature có folder `api/` chứa các hook query/mutation:
  ```ts
  // features/products/api/useProducts.ts
  export const useProducts = (params: ProductQueryParams) => 
    useQuery({ queryKey: ['products', params], queryFn: () => productApi.list(params) });
  ```

### 4.5 State Management
- **TanStack Query**: server state (data từ API, caching, sync).
- **Zustand**: client state nhẹ (theme, sidebar collapsed, current tenant context).
- **React Hook Form + Zod**: form state + validation.
- KHÔNG dùng Redux trừ khi thật cần (project size hiện tại không cần).

### 4.6 Routing
- React Router v6+, dùng nested routes.
- Tách `PublicRoutes` và `ProtectedRoutes`.
- `ProtectedRoute` HOC check JWT + role.
- Route definition tập trung trong `app/router.tsx`.

### 4.7 Styling
- Ưu tiên Ant Design components cho admin dashboard (table, form, modal sẵn có).
- Nếu chọn shadcn/ui: cần config Tailwind, design system tùy biến hơn.
- Tránh CSS inline trừ giá trị động.

---

## 5. QUY ƯỚC DATABASE

### 5.1 Đặt tên
- **Bảng:** snake_case, số nhiều (`products`, `stock_ins`, `tiktok_shop_connections`).
- **Cột:** snake_case (`created_at`, `tenant_id`).
- **FK:** `{table}_id` (ví dụ `product_id`, `supplier_id`).
- **Index:** `ix_{table}_{column}` (ví dụ `ix_products_tenant_id`).
- **Unique:** `ux_{table}_{column}`.

### 5.2 Index bắt buộc
- `TenantId` trên mọi bảng nghiệp vụ (composite với các cột thường filter).
- `IsDeleted` (filtered index where `is_deleted = false` nếu PG hỗ trợ — PG có partial index).
- FK cột.
- Cột thường dùng sort (CreatedAt, UpdatedAt).

### 5.3 Kiểu dữ liệu
- ID: `uuid` (Guid) — gọn, không lộ thứ tự, dễ merge.
- Tiền: `numeric(18,4)` — KHÔNG dùng float/double.
- Thời gian: `timestamptz` (DateTimeOffset) — luôn lưu UTC.
- Text dài: `text`. Text ngắn ràng buộc độ dài cụ thể.

### 5.4 Migration
- Mọi thay đổi schema phải qua EF Core Migration: `dotnet ef migrations add <Name>`.
- Tên migration mô tả rõ: `AddStockMovementTable`, `AddIndexOnProductsTenantId`.
- Review SQL sinh ra trước khi apply: `dotnet ef migrations script`.
- KHÔNG sửa migration đã apply lên môi trường shared — tạo migration mới.

---

## 6. ENUM CONVENTIONS

> **YÊU CẦU BẮT BUỘC:** Tất cả enum đặt trong `TikTokShop.Domain/Enums/`, mỗi enum 1 file.

### Quy tắc:
- Khai báo giá trị int rõ ràng (tránh shift số khi insert giữa).
  ```csharp
  public enum UserRole
  {
      Admin = 1,
      Manager = 2,
      Staff = 3
  }
  ```
- Lưu DB dạng `int` (hiệu năng + index tốt), nhưng có thể cấu hình EF Core convert sang string nếu cần đọc DB dễ.
- Frontend: định nghĩa enum song song trong `shared/constants/enums.ts` — giữ đồng bộ thủ công hoặc generate từ OpenAPI.

### Danh sách enum dự kiến:
- `UserRole` (Admin, Manager, Staff)
- `TenantStatus` (Active, Suspended, Trial, Expired)
- `StockMovementType` (In = 1, Out = 2, ReturnIn = 3, ReturnOut = 4, Adjustment = 5)
- `StockMovementSource` (Manual = 1, TikTokOrder = 2, Adjustment = 3, Import = 4, TikTokReturn = 5)
- `InventoryReservationStatus` (Active = 1, Committed = 2, Released = 3, Expired = 4)
- `OrderStatus` (Pending, Confirmed, Shipping, Completed, Cancelled, Refunded)
- `PaymentStatus` (Unpaid, Paid, PartiallyPaid, Refunded)
- `TikTokShopConnectionStatus` (Active, Expired, Revoked, Error)
- `TikTokOrderStatus` numeric code chuẩn TikTok 202309+:
  - `Unpaid = 100`
  - `AwaitingShipment = 111` (paid, chưa ship → tạo reservation)
  - `AwaitingCollection = 112` (đã giao shipper) 
  - `InTransit = 121`
  - `Delivered = 122`
  - `Completed = 130` (giao thành công, hết thời gian return)
  - `Cancelled = 140` (release reservation)
- `TikTokOrderSyncStatus` (Synced, MappingPending, Reserved, StockApplied, StockReversed, Failed)
- `TikTokReturnStatus` (Requested, Approved, Rejected, ReturnReceived, Refunded, Closed)
- `WebhookEventStatus` (Received, Processing, Processed, Failed, Skipped)
- `OutboxStatus` (Pending, Processed, Failed)
- `AuditAction` (Create, Update, Delete, Restore, Login, Logout)

---

## 6.1 STOCK MOVEMENT UNIFIED MODEL (QUAN TRỌNG)

> **Quy tắc bất di bất dịch:** Tồn kho ở hệ thống này chỉ được tính từ **MỘT NGUỒN DUY NHẤT** là bảng `StockMovements`. Không có ngoại lệ.

### Lý do
Hệ thống có 3 nguồn ảnh hưởng tồn kho: nhập hàng thủ công (StockIn), bán hàng thủ công/tay (StockOut), và đơn hàng TikTok (TikTokOrderItem). Nếu mỗi nguồn lưu tách riêng và view Inventory phải UNION 3 nguồn → dễ sai số liệu, khó debug, khó audit, race condition khi tính tồn lúc bán.

### Mô hình
- **`StockMovement`** là bảng append-only ghi lại MỌI biến động kho (mỗi event = 1 row).
- Các bảng nghiệp vụ (`StockIn`, `StockOut`, `TikTokOrderItem`) là **document** (chứa thông tin nghiệp vụ: giá, khách hàng, ghi chú, ...).
- Khi document được tạo/cập nhật/hủy → sinh ra **StockMovement tương ứng** trong **cùng 1 transaction**.
- View `Inventory` chỉ aggregate trên `StockMovements`.

### Schema StockMovement
```csharp
// LƯU Ý ĐẶC BIỆT: Entity này KHÔNG hỗ trợ soft delete.
// Ghi đè field IsDeleted trong configuration để KHÔNG tham gia query filter soft delete.
// Lý do: append-only ledger — record sai sửa bằng compensating movement, không xóa.
public class StockMovement : BaseEntity
{
    public Guid ProductId { get; set; }
    public StockMovementType Type { get; set; }       // In/Out/ReturnIn/ReturnOut/Adjustment
    public StockMovementSource Source { get; set; }   // Manual/TikTokOrder/Adjustment/Import/TikTokReturn
    public int Quantity { get; set; }                 // LUÔN dương; chiều xác định bởi Type
    public decimal UnitCost { get; set; }             // Giá nhập (cho In) hoặc giá bán (cho Out)
    public DateTimeOffset OccurredAt { get; set; }    // Thời điểm thực tế (không phải CreatedAt)
    
    // Reference back to source document (chỉ 1 trong các field này được set)
    public Guid? StockInId { get; set; }
    public Guid? StockOutId { get; set; }
    public Guid? TikTokOrderItemId { get; set; }
    public Guid? TikTokReturnLineId { get; set; }
    
    // Idempotency key — bắt buộc unique trong tenant
    // Format chuẩn:
    //   "manual-in:{stockInId}"
    //   "manual-in-reverse:{stockInId}"   (compensating khi xóa stockIn)
    //   "manual-out:{stockOutId}"
    //   "manual-out-reverse:{stockOutId}"
    //   "tiktok-out:{shopCipher}:{orderId}:{lineItemId}"
    //   "tiktok-cancel-reverse:{shopCipher}:{orderId}:{lineItemId}"  
    //   "tiktok-return-in:{shopCipher}:{returnId}:{returnLineId}"
    //   "adjustment:{adjustmentId}"
    public string IdempotencyKey { get; set; } = null!;
    
    public string? Note { get; set; }
}
```

**Configuration override:**
```csharp
public void Configure(EntityTypeBuilder<StockMovement> builder)
{
    // ... fields config ...
    
    // KHÔNG apply query filter IsDeleted. Override BaseEntity filter:
    builder.HasQueryFilter(sm => sm.TenantId == _currentUser.TenantId);
    // Bỏ điều kiện !sm.IsDeleted vì append-only.
    
    // IsDeleted luôn = false cho movements
    builder.Property(sm => sm.IsDeleted).HasDefaultValue(false);
}
```

### Quy ước Type vs ảnh hưởng tồn kho
| Type | Hiệu ứng | Use case |
|------|----------|----------|
| `In` | +Quantity | StockIn (nhập hàng), TikTok refund hoàn kho |
| `Out` | -Quantity | StockOut (bán tay), TikTok order Shipped |
| `ReturnIn` | +Quantity | Khách trả hàng (hoàn về kho) |
| `ReturnOut` | -Quantity | Trả lại nhà cung cấp |
| `Adjustment` | ±Quantity (lấy dấu từ Quantity field) | Kiểm kê chênh lệch |

> Quantity LUÔN lưu giá trị dương. Chiều +/- xác định bởi Type. Khi aggregate: 
> `StockOnHand = SUM(CASE WHEN Type IN (In, ReturnIn) THEN Quantity ELSE -Quantity END)`

### Index bắt buộc
- `(TenantId, ProductId, OccurredAt DESC)` — index chính để aggregate
- `(TenantId, IdempotencyKey)` UNIQUE — chống ghi trùng
- `(TenantId, Source, OccurredAt)` — query theo nguồn
- `TikTokOrderItemId` (FK)

### Quy tắc nghiêm ngặt khi sinh StockMovement
1. **Idempotency:** Trước khi insert, check xem `IdempotencyKey` đã tồn tại chưa. Nếu có → bỏ qua (không throw error, không duplicate). Cài đặt bằng unique constraint + `ON CONFLICT DO NOTHING` hoặc try-catch `DbUpdateException` với PG error code 23505.
2. **Cùng transaction:** Sinh StockMovement và lưu document phải nằm trong cùng `IDbContextTransaction` (hoặc cùng `SaveChangesAsync` nếu dùng EF tracking).
3. **Không sửa, không xóa cứng:** StockMovement là append-only. Nếu document gốc bị sửa Quantity → sinh thêm `Adjustment` movement bù trừ, KHÔNG update row cũ. Nếu document bị soft delete → sinh movement ngược chiều (compensating movement).
4. **Source-of-truth là OccurredAt, không phải CreatedAt:** OccurredAt = thời điểm nghiệp vụ thực tế (TikTok shipped_time, ngày nhập hàng user chọn), CreatedAt = lúc insert vào DB.

### Concurrency control khi bán
Khi tạo StockOut hoặc khi handler xử lý TikTok order Shipped:
- Mở transaction với `IsolationLevel.ReadCommitted`.
- Lock product row: `SELECT 1 FROM products WHERE id = @id FOR UPDATE` (advisory lock) trước khi tính tồn kho hiện tại.
- Tính `currentStock` qua aggregate StockMovements (đã được lock chuỗi qua FK).
- Nếu `currentStock < requestedQuantity` → throw `BusinessRuleException` ("Insufficient stock for product X: available=Y, requested=Z").
- Nếu OK → insert StockMovement + document, commit.

> Pattern này tránh được oversell khi 2 đơn TikTok sync song song hoặc bán tay đụng với sync TikTok.

### Hệ quả thiết kế
- **View `Inventory` query siêu đơn giản**: aggregate `StockMovements` group by ProductId. Không UNION 3 bảng.
- **Lịch sử kho trong suốt**: lịch sử biến động 1 sản phẩm = `SELECT * FROM stock_movements WHERE product_id = X ORDER BY occurred_at`.
- **Audit dễ**: chỉ cần kiểm tra movement nào không có document gốc → bug.
- **Dashboard linh hoạt**: filter theo `Source = TikTokOrder` để tách doanh thu/lợi nhuận TikTok vs Manual.

---

## 6.2 INVENTORY RESERVATION MODEL (Chống oversell)

> **Vấn đề:** Khi khách đặt đơn TikTok (`AwaitingShipment = 111`), kho thực tế vẫn còn hàng nhưng cam kết bán rồi. Nếu chỉ trừ kho khi `Shipped`, trong khoảng giữa đó (vài giờ đến vài ngày) hệ thống vẫn hiển thị còn hàng → bán tay hoặc đơn TikTok khác có thể oversell.

### Mô hình: 2 con số tồn kho
- **`PhysicalStock`** = aggregate StockMovements như trước (đây là tồn kho **vật lý** thực sự trong kho).
- **`ReservedQuantity`** = SUM của các Reservation đang Active cho product này.
- **`AvailableStock`** = `PhysicalStock - ReservedQuantity` (đây là số có thể bán tiếp).

### Entity InventoryReservation
```csharp
public class InventoryReservation : BaseEntity
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public InventoryReservationStatus Status { get; set; }
    // Active: đang giữ chỗ
    // Committed: đã chuyển thành StockMovement Out (đơn shipped)
    // Released: hủy reservation (đơn cancel trước ship) — KHÔNG sinh movement
    // Expired: reservation quá hạn không có action → auto release (job dọn)
    
    public Guid? TikTokOrderItemId { get; set; }  // 1 reservation gắn với 1 order item
    public DateTimeOffset ReservedAt { get; set; }
    public DateTimeOffset? ResolvedAt { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }  // default ReservedAt + 7 ngày, configurable
    
    // Idempotency: 1 order item chỉ tạo 1 reservation
    public string IdempotencyKey { get; set; } = null!;
    // Format: "reservation:{shopCipher}:{orderId}:{lineItemId}"
}
```

### Lifecycle
```
┌──────────────────────────────────────────────────────────────────┐
│  TikTok event: AwaitingShipment (111)                            │
│  → Tạo Reservation (Active), giảm AvailableStock                 │
│  → KHÔNG sinh StockMovement                                      │
│  → KHÔNG giảm PhysicalStock                                      │
└──────────────────────────────────────────────────────────────────┘
                              │
                ┌─────────────┴────────────┐
                ▼                          ▼
┌──────────────────────────────┐  ┌──────────────────────────────┐
│ TikTok event: InTransit (121)│  │ TikTok event: Cancelled (140)│
│ hoặc Delivered (122)         │  │                              │
│ → Reservation.Committed      │  │ → Reservation.Released       │
│ → Sinh StockMovement Out     │  │ → KHÔNG sinh movement        │
│ → PhysicalStock giảm         │  │ → AvailableStock tự tăng lại │
│ → AvailableStock không đổi   │  │                              │
│   (vì ReservedQty cũng giảm) │  │                              │
└──────────────────────────────┘  └──────────────────────────────┘
                │
                ▼ (nếu sau đó khách return)
┌──────────────────────────────┐
│ TikTok event: Return approved│
│ → Sinh StockMovement In      │
│   (Type=ReturnIn, Src=...)   │
│ → PhysicalStock tăng lại     │
│ → KHÔNG tạo reservation mới  │
└──────────────────────────────┘
```

### Quy tắc check khi bán
- **Bán tay (StockOut.Create):**
  - Kiểm `AvailableStock >= requestedQty` → mới cho bán.
  - Lock product row (advisory lock) trước khi tính.
- **Đơn TikTok mới về (AwaitingShipment):**
  - Kiểm `AvailableStock >= orderItem.Quantity`.
  - Nếu không đủ: vẫn tạo reservation nhưng đánh dấu `Status = Active` + log warning. Đây là case "oversold" — TikTok đã nhận đơn, hệ thống phải hiển thị âm để admin biết và xử lý (ưu tiên đơn TikTok hơn bán tay).
  - Ưu tiên này hợp lý vì đơn TikTok đã có khách hàng cụ thể; bán tay có thể từ chối.

### Index bắt buộc
- `(TenantId, ProductId, Status)` — query reservation active của product
- `(TenantId, IdempotencyKey)` UNIQUE
- `(TenantId, ExpiresAt) WHERE status = 1` (partial index cho job dọn)

### Concurrency
Tương tự StockMovement: lock product row qua advisory lock trước khi:
1. Tính `AvailableStock`
2. Insert reservation hoặc commit reservation

### Auto-expire job
Background service mỗi 1 giờ:
- Tìm reservations `Status = Active AND ExpiresAt < now`
- Set `Status = Expired`
- Log để admin theo dõi (có thể là dấu hiệu sync TikTok bị lag)

> **Lưu ý quan trọng:** Default ExpiresAt = ReservedAt + 7 ngày. TikTok quy định seller có 3-7 ngày để ship sau khi paid (tùy region). Nếu reservation expire mà chưa committed → 99% là có lỗi sync.

---

## 7. GIT WORKFLOW & COMMIT

- **Branch:** `main` (production), `develop` (integration), `feature/*`, `bugfix/*`, `hotfix/*`.
- **Commit message:** Conventional Commits:
  - `feat: add product CRUD endpoints`
  - `fix: correct stock calculation when refund`
  - `refactor: extract pagination helper`
  - `docs: update CLAUDE.md`
  - `chore: bump packages`
- KHÔNG commit `appsettings.*.json` chứa secret thật, dùng `.env` + User Secrets.
- KHÔNG commit `node_modules/`, `bin/`, `obj/`, `.env`.

---

## 8. SECRETS & CONFIG

- **Dev local:** dùng .NET User Secrets cho backend, `.env.local` cho frontend.
- **Docker:** truyền qua biến môi trường trong `docker-compose.yml` (file `.env` riêng).
- **Production:** dùng secret manager (Azure Key Vault, AWS Secrets Manager, ...).
- Các key bắt buộc:
  - `ConnectionStrings__DefaultConnection`
  - `Jwt__SecretKey` (≥ 64 ký tự random)
  - `Jwt__Issuer`, `Jwt__Audience`
  - `Jwt__AccessTokenMinutes`, `Jwt__RefreshTokenDays`
  - `TikTok__AppKey`, `TikTok__AppSecret`, `TikTok__RedirectUri`

---

## 9. TESTING

- **Unit test:** xUnit + FluentAssertions + NSubstitute (mock).
- Phủ test cho: business logic (services), validators, mapping.
- **Integration test:** WebApplicationFactory + Testcontainers (PostgreSQL container) cho test DB thật.
- Target coverage: ≥ 70% cho Application layer, không stress Coverage cho Controller (đã test qua integration).
- Frontend: Vitest + React Testing Library cho component, hook.

---

## 10. LOGGING & MONITORING

- **Serilog** sink: Console (Dev), File (rolling daily), Seq (optional, dễ filter).
- Log structured, có `TenantId`, `UserId`, `CorrelationId` (middleware sinh GUID đầu mỗi request).
- KHÔNG log: password, JWT token đầy đủ, secret.
- Level:
  - `Information`: business event quan trọng (login, create order, ...)
  - `Warning`: condition bất thường nhưng không lỗi (rate limit, retry).
  - `Error`: exception không recover được.
  - `Debug`: chỉ Dev.

---

## 11. DOCKER

### Backend Dockerfile (multi-stage)
- Stage 1: `mcr.microsoft.com/dotnet/sdk:9.0` — restore + build + publish.
- Stage 2: `mcr.microsoft.com/dotnet/aspnet:9.0` — runtime.
- Non-root user.

### Frontend Dockerfile (multi-stage)
- Stage 1: `node:20-alpine` — `npm ci && npm run build`.
- Stage 2: `nginx:alpine` — serve static + reverse proxy `/api` về backend.

### docker-compose.yml dịch vụ:
- `postgres` (image `postgres:16-alpine`, volume persist)
- `backend` (depends_on postgres healthcheck)
- `frontend` (depends_on backend)
- `seq` (optional cho log)
- `pgadmin` (optional cho dev)

---

## 12. NGUYÊN TẮC LÀM VIỆC VỚI CLAUDE (VS CODE EXTENSION)

> Bạn (user) sẽ vibe code 100% — Claude sinh code, bạn review & nghiệm thu. Để hiệu quả:

### Khi yêu cầu Claude sinh code, luôn nêu rõ:
1. **Layer/feature đang làm** (vd: "Tạo entity Product ở Domain layer").
2. **Tham chiếu CLAUDE.md** mặc định Claude phải đọc file này trước khi sinh code.
3. **Tham chiếu file liên quan** (vd: "Theo pattern của ProductService đã có").

### Claude phải tuân thủ:
- **KHÔNG** tự ý đổi tech stack, kiến trúc, conventions đã định ở CLAUDE.md.
- **KHÔNG** sinh code thiếu tenant filter (lỗi bảo mật nghiêm trọng).
- **KHÔNG** dùng pattern khác nhau cho cùng loại task (ví dụ chỗ dùng MediatR chỗ không).
- **PHẢI** giải thích ngắn lý do nếu lệch khỏi quy ước (vd: "Bỏ qua soft delete cho bảng A vì lý do X").
- **PHẢI** kèm using statements đầy đủ, không để code thiếu reference.
- **PHẢI** đặt code đúng folder theo Section 3.2.
- **PHẢI** thêm comment hoặc summary tiếng Việt giải thích ý nghĩa cho:
  - Mọi field trong Entity, DTO, ViewModel (dạng `/// <summary>` cho C# hoặc `//` inline cho TS).
  - Mọi method/function có logic nghiệp vụ không hiển nhiên.
  - Mọi constant, enum value, và magic number.
  - Các đoạn code phức tạp, công thức tính toán, hoặc workaround đặc biệt.
  - Ví dụ C#:
    ```csharp
    /// <summary>Số lượng hàng thực tế đang giữ chờ ship (đã reserve, chưa xuất kho).</summary>
    public int ReservedQuantity { get; set; }
    ```
  - Ví dụ TypeScript:
    ```ts
    /** Tổng doanh thu sau khi trừ phí TikTok và hoàn hàng */
    netRevenue: number;
    ```

### Khi sửa bug:
- Tìm root cause, không chỉ patch surface.
- Cập nhật/thêm test nếu có sẵn test suite.
- Note ngắn vào commit message hoặc PR description.

### Khi review code Claude sinh:
- Check tenant filter có không.
- Check soft delete có không.
- Check exception có throw đúng custom exception không.
- Check DTO không expose entity trực tiếp.
- Check log không lộ secret.

---

## 12.5 TIKTOK INTEGRATION PATTERNS (BẮT BUỘC ĐỌC trước khi làm Phase 7)

> Các thông tin này được tổng hợp từ docs TikTok Shop Partner API v202309+. **Format có thể thay đổi**, hãy verify lại doc chính thức (https://partner.tiktokshop.com) trước khi code.

### 12.5.1 Authentication & shop_cipher (CRỰC KỲ QUAN TRỌNG)
- TikTok Shop API yêu cầu **3 thứ** trong mọi request: `app_key`, `access_token`, **`shop_cipher`**.
- `shop_cipher` ≠ `shop_id`. Là chuỗi mã hóa do TikTok cấp riêng cho mỗi {app, shop} pair sau khi authorize. **Phải lưu cùng access_token**.
- Lấy shop_cipher từ endpoint `GET /authorization/202309/shops` sau khi exchange code → token.
- Mọi request phải có:
  - Header `x-tts-access-token: {access_token}`
  - Query param `app_key={app_key}`
  - Query param `shop_cipher={shop_cipher}` (cho shop-scoped endpoints)
  - Query param `sign={signature}` — HMAC-SHA256 của request, ký bằng app_secret
  - Query param `timestamp={unix_timestamp}`

### 12.5.2 Region cluster routing
TikTok Shop có nhiều cluster region với base URL khác nhau:
- **Non-EU (US, UK, SEA…):** `https://open-api.tiktokglobalshop.com`
- **EU:** `https://open-api.tiktokglobalshop.com` (cùng nhưng route khác — đọc doc)
- **Sandbox US:** `https://open-api-sandbox.tiktokglobalshop.com`

→ `TikTokShopConnection` PHẢI lưu thêm field `Region` và `BaseApiUrl` (resolve lúc connect, không hardcode).

### 12.5.3 Order status codes (TikTok 202309+)
| Code | Tên | Ý nghĩa | Action hệ thống |
|------|-----|---------|-----------------|
| 100 | UNPAID | Khách chưa thanh toán | Bỏ qua, không reserve |
| 111 | AWAITING_SHIPMENT | Đã paid, seller cần ship | **Tạo Reservation** |
| 112 | AWAITING_COLLECTION | Đã handover shipper | Update tracking, giữ reservation |
| 121 | IN_TRANSIT | Đang vận chuyển | **Commit reservation → sinh Out movement** |
| 122 | DELIVERED | Đã giao | Idempotent (đã commit ở 121) |
| 130 | COMPLETED | Hoàn tất (hết hạn return) | Đánh dấu order = final |
| 140 | CANCELLED | Hủy | **Release reservation** hoặc **Reverse movement** |

> **Lý do commit ở 121 (InTransit) chứ không phải 122 (Delivered):** đơn đã ra khỏi kho ở 121 → hàng vật lý đã đi. Đợi đến 122 thì có thể delay 1-5 ngày — trong khoảng đó kho vật lý sai.

### 12.5.4 Rate limits (CRỰC KỲ QUAN TRỌNG)
- **50 requests/second per shop** (không phải per app).
- Vượt → HTTP 429 với error code `rate_limit_exceeded`.
- Implement **per-shop token bucket** trong `ITikTokApiClient` — KHÔNG dùng rate limiter chung cho cả app.
- Polly: retry với exponential backoff khi gặp 429, MAX 3 lần, sau đó queue lại.

### 12.5.5 Pagination — cursor-based
- TikTok dùng `next_page_token` (cursor), KHÔNG dùng pageNumber.
- Pattern fetch all:
  ```csharp
  string? cursor = null;
  do {
      var resp = await client.GetOrdersAsync(new { page_token = cursor, page_size = 50, ... });
      // process resp.data.orders
      cursor = resp.data.next_page_token;
  } while (!string.IsNullOrEmpty(cursor));
  ```
- Khi sync incremental: filter `update_time_ge` (timestamp Unix giây).

### 12.5.6 Webhook
TikTok push event qua HTTPS POST đến callback URL đã đăng ký. **PHẢI verify HMAC-SHA256 signature**.

**Event types quan trọng** (tên có thể khác tùy version doc):
- `order_status_change` — change order status (100→111→...)
- `package_update` — package status change
- `recipient_address_update` — khách đổi địa chỉ
- `cancel_status_change` — order cancel
- `return_status_change` — return request từ khách
- `authorization.removed` — user gỡ app → connection chết
- `product_status_change` — product được duyệt/từ chối (cho product push lên TikTok)

**Verify signature pattern:**
```csharp
// TikTok ký theo công thức: HMAC-SHA256(app_secret, request_url + body)
// Header: Authorization: {signature}
public bool VerifyWebhook(HttpRequest req, string body, string appSecret)
{
    var signature = req.Headers["Authorization"].ToString();
    var timestamp = req.Headers["X-Tts-Timestamp"].ToString();
    var canonicalString = req.GetEncodedUrl() + body;  // chính xác theo doc TikTok
    var expected = ComputeHmacSha256(appSecret, canonicalString);
    return CryptographicOperations.FixedTimeEquals(
        Encoding.UTF8.GetBytes(signature),
        Encoding.UTF8.GetBytes(expected));
}
```

**Webhook processing rule:**
1. Verify signature TRƯỚC khi đọc payload.
2. **Phải respond 200 OK trong vài giây** — nếu không TikTok retry, có thể spam event trùng.
3. Pattern: nhận event → save vào bảng `WebhookEvents` (Status = Received) → trả 200 ngay → background processor xử lý async.
4. **Idempotent processor**: dùng `event_id` của TikTok làm idempotency key, đã xử lý rồi → skip.

### 12.5.7 Authorization lifecycle
- Access token sống ~ 24 giờ. Refresh token sống ~ 30 ngày.
- Khi user gỡ app: webhook `authorization.removed` bắn về → set connection `Status = Revoked`, KHÔNG xóa connection (giữ data lịch sử).
- Connection revoked → mọi sync stop, FE hiển thị "Cần kết nối lại".

### 12.5.8 Sandbox
- US & UK có sandbox riêng — dùng để dev không ảnh hưởng shop thật.
- Setup `TikTokSandbox:Enabled` flag trong appsettings để switch.

### 12.5.9 Scopes cần xin khi tạo app
Liệt kê tối thiểu (verify theo doc khi tạo app):
- `product.list`, `product.read` — đọc product/SKU để map
- `product.inventory.update` — push tồn kho lên TikTok
- `order.list`, `order.detail` — sync orders
- `fulfillment.shipping_doc.read` — đọc trạng thái ship
- `return.list`, `return.detail` — đồng bộ return
- `finance.statement.read`, `finance.payment.read` — Finance API tính lợi nhuận thực
- `seller.shop.read` — thông tin shop

### 12.5.10 Outbox pattern (cho push tồn kho lên TikTok)
Mỗi khi StockMovement được sinh ra → cũng phải đẩy số tồn mới lên TikTok cho mọi SKU đang map sản phẩm đó. Yêu cầu **eventual consistency**, không thể fail.

**Pattern:**
1. Trong cùng transaction với StockMovement: insert row vào bảng `OutboxMessages` (Type = "PushInventory", Payload = {productId, newQuantity, shopCipher, tikTokSkuId}, Status = Pending).
2. Background dispatcher poll bảng `OutboxMessages WHERE Status = Pending` mỗi 5 giây.
3. Dispatch: gọi TikTok API → success: Status = Processed. Fail: increment RetryCount, exponential backoff. Sau N lần → Status = Failed, alert admin.
4. Đảm bảo "at-least-once delivery" + TikTok side idempotent (TikTok overwrite quantity, không tăng dần).

---

## 13. ROADMAP (TÓM TẮT — chi tiết trong Plan.md)

| Phase | Nội dung | Ước lượng |
|-------|----------|-----------|
| **Pre** | **Đăng ký TikTok App, xin scopes, setup sandbox** (làm parallel từ ngày 1) | **3-7 ngày chờ duyệt** |
| 0 | Setup project, infra, base entity, JWT | 2-3 ngày |
| 1 | Multi-tenant + User + Role | 2-3 ngày |
| 2 | CRUD: Product, Supplier | 1-2 ngày |
| 3 | Stock In / Stock Out + StockMovement unified | 3-4 ngày |
| 3.5 | Inventory view + Reservation model + concurrency | 2-3 ngày |
| 4 | Frontend khung: layout, auth, route | 2-3 ngày |
| 5 | Frontend các trang CRUD | 3-5 ngày |
| 6 | Dashboard nội bộ (giá vốn, lợi nhuận, đa nguồn) | 2-3 ngày |
| 7.1 | TikTok Connection + OAuth + shop_cipher + region | 2 ngày |
| 7.2 | Webhook receiver + HMAC verify + queue | 2-3 ngày |
| 7.3 | Polling reconciliation + per-shop rate limiter | 2 ngày |
| 7.4 | Product mapping (UI + flow) | 2 ngày |
| 7.5 | Order handler: AwaitingShipment → Reservation, InTransit → Commit | 3-4 ngày |
| 7.6 | Cancel/Return handler | 2-3 ngày |
| 7.7 | Push inventory lên TikTok (Outbox pattern) | 2-3 ngày |
| 7.8 | Finance API integration (lợi nhuận thực) | 2-3 ngày |
| 7.9 | Video sync + Dashboard TikTok | 3-4 ngày |
| 8 | Testing, Dockerize, polish | 3-4 ngày |

**Tổng ước lượng: ~7-10 tuần (vibe code, 1 dev, full-time).**

> **Tăng so với ước lượng trước (4-6 tuần):** vì đã thêm webhook, reservation, push inventory, return handler, Finance API — đây là phiên bản production-ready cho 1 hệ thống thực sự kết nối TikTok Shop, không phải MVP.

---

## 14. CHECKLIST TRƯỚC KHI MERGE BẤT KỲ FEATURE NÀO

- [ ] Entity kế thừa BaseEntity và có Global Query Filter cho TenantId
- [ ] Endpoint có `[Authorize]` đúng role
- [ ] DTO không leak field nhạy cảm (PasswordHash, RefreshToken raw, access_token TikTok, ...)
- [ ] Validator (FluentValidation) cho mọi request body
- [ ] Service throw custom exception, không throw Exception thường
- [ ] Migration mới đã chạy và commit
- [ ] Swagger doc tự sinh ra rõ ràng (XML comments cho endpoint quan trọng)
- [ ] Test cơ bản (ít nhất happy path)
- [ ] Frontend: không có `any`, không có warning console nghiêm trọng
- [ ] Không hardcode secret, không log nhạy cảm
- [ ] **Stock movement:** đã sinh `StockMovement` với `IdempotencyKey` đúng format
- [ ] **Reservation:** thao tác bán/order qua TikTok đã update reservation đúng (Active/Committed/Released)
- [ ] **Idempotency:** handler async/sync TikTok có test idempotency (gọi 2 lần → kho không bị trừ 2 lần)
- [ ] **Concurrency:** thao tác bán có advisory lock product trong transaction
- [ ] **Webhook:** signature đã verify trước khi xử lý, trả 200 trong vài giây
- [ ] **Webhook:** event đã được persist vào WebhookEvents trước khi response
- [ ] **Outbox:** thay đổi cần đồng bộ lên TikTok đã insert OutboxMessage cùng transaction
- [ ] **TikTok API call:** đã gắn shop_cipher, app_key, sign, timestamp đúng format
- [ ] **TikTok status mapping:** dùng numeric code (111, 121, ...) đúng theo §12.5.3, không dùng chuỗi text

---

**Lưu file Plan.md đi kèm để xem lộ trình triển khai từng bước.**
