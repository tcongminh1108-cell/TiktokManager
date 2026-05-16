using Microsoft.AspNetCore.HttpOverrides;
using Scalar.AspNetCore;
using Serilog;
using TikTokShop.Api.Extensions;
using TikTokShop.Api.Middlewares;
using TikTokShop.Application;
using TikTokShop.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using TikTokShop.Infrastructure;
using TikTokShop.Infrastructure.Persistence;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((ctx, services, config) =>
        config.ReadFrom.Configuration(ctx.Configuration)
              .ReadFrom.Services(services)
              .Enrich.FromLogContext());

    // CORS — đọc danh sách origins từ config, Railway override qua env var Cors__Origins
    var corsOrigins = (builder.Configuration["Cors:Origins"] ?? "http://localhost:5173")
        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    builder.Services.AddCors(opts =>
        opts.AddDefaultPolicy(p =>
            p.WithOrigins(corsOrigins)
             .AllowAnyHeader()
             .AllowAnyMethod()
             .AllowCredentials()));

    builder.Services.AddControllers()
        .AddJsonOptions(o =>
            o.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddOpenApiWithJwt();
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddHealthChecks()
        .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection")!);
    builder.Services.AddHostedService<TikTokShop.Infrastructure.BackgroundServices.ReservationExpiryService>();
    builder.Services.AddHostedService<TikTokShop.Infrastructure.BackgroundServices.TikTokTokenRefreshService>();
    builder.Services.AddHostedService<TikTokShop.Infrastructure.BackgroundServices.WebhookProcessorService>();
    builder.Services.AddHostedService<TikTokShop.Infrastructure.BackgroundServices.OrderReconciliationService>();
    builder.Services.AddHostedService<TikTokShop.Infrastructure.BackgroundServices.OutboxDispatcherService>();
    builder.Services.AddHostedService<TikTokShop.Infrastructure.BackgroundServices.FinanceSyncService>();
    builder.Services.AddHostedService<TikTokShop.Infrastructure.BackgroundServices.VideoSyncService>();

    var app = builder.Build();

    // Auto-migrate on startup — Railway sẽ retry nếu DB chưa sẵn sàng
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync();
        Log.Information("Database migration completed");
    }

    // Đọc X-Forwarded-* headers từ Railway/proxy trước mọi middleware khác
    app.UseForwardedHeaders(new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
    });

    app.UseMiddleware<CorrelationIdMiddleware>();
    app.UseSerilogRequestLogging(opts =>
    {
        opts.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            if (httpContext.Items["CorrelationId"] is string correlationId)
                diagnosticContext.Set("CorrelationId", correlationId);
        };
    });
    app.UseMiddleware<ExceptionHandlingMiddleware>();

    // Chỉ redirect HTTPS khi chạy local dev, Railway tự xử lý TLS ở proxy
    if (app.Environment.IsDevelopment())
        app.UseHttpsRedirection();

    app.UseCors();
    app.MapOpenApi();
    app.MapScalarApiReference();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseMiddleware<UserContextEnrichmentMiddleware>();
    app.MapControllers();
    app.MapHealthChecks("/health");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// Expose Program class to integration test project
public partial class Program { }
