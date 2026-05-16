using Scalar.AspNetCore;
using Serilog;
using TikTokShop.Api.Extensions;
using TikTokShop.Api.Middlewares;
using TikTokShop.Application;
using TikTokShop.Domain.Enums;
using TikTokShop.Infrastructure;

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
    app.MapOpenApi();
    app.MapScalarApiReference();
    app.UseHttpsRedirection();
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
