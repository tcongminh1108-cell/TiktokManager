using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using TikTokShop.Application.Features.Auth;
using TikTokShop.Application.Features.Dashboard;
using TikTokShop.Application.Features.DevSeed;
using TikTokShop.Application.Features.Inventory;
using TikTokShop.Application.Features.Products;
using TikTokShop.Application.Features.Reservations;
using TikTokShop.Application.Features.StockIns;
using TikTokShop.Application.Features.StockMovements;
using TikTokShop.Application.Features.StockOuts;
using TikTokShop.Application.Features.Suppliers;
using TikTokShop.Application.Features.TikTok.Connections;
using TikTokShop.Application.Features.TikTok.Finance;
using TikTokShop.Application.Features.TikTok.Mappings;
using TikTokShop.Application.Features.TikTok.Orders;
using TikTokShop.Application.Features.TikTok.Returns;
using TikTokShop.Application.Features.TikTok.Videos;
using TikTokShop.Application.Features.TikTok.Webhooks;
using TikTokShop.Application.Features.Users;

namespace TikTokShop.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ISupplierService, SupplierService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IStockMovementService, StockMovementService>();
        services.AddScoped<IStockInService, StockInService>();
        services.AddScoped<IStockOutService, StockOutService>();
        services.AddScoped<IReservationService, ReservationService>();
        services.AddScoped<IInventoryService, InventoryService>();
        services.AddScoped<IDevSeedService, DevSeedService>();
        services.AddScoped<IDashboardService, DashboardService>();
        // Phase 7
        services.AddScoped<ITikTokConnectionService, TikTokConnectionService>();
        services.AddScoped<IWebhookEventService, WebhookEventService>();
        services.AddScoped<IProductMappingService, ProductMappingService>();
        // Phase 7.5
        services.AddScoped<ITikTokOrderService, TikTokOrderService>();
        services.AddScoped<IOrderEventHandler, OrderEventHandler>();
        // Phase 7.6
        services.AddScoped<ITikTokReturnService, TikTokReturnService>();
        services.AddScoped<IReturnEventHandler, ReturnEventHandler>();
        // Phase 7.8
        services.AddScoped<ITikTokFinanceService, TikTokFinanceService>();
        // Phase 7.9
        services.AddScoped<ITikTokVideoService, TikTokVideoService>();
        services.AddScoped<ITikTokDashboardService, TikTokDashboardService>();
        return services;
    }
}
