using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using TikTokShop.Application.Interfaces;
using TikTokShop.Domain.Enums;
using TikTokShop.Domain.Interfaces;
using TikTokShop.Infrastructure.ExternalServices.TikTok;
using TikTokShop.Infrastructure.Identity;
using TikTokShop.Infrastructure.Persistence;
using TikTokShop.Infrastructure.Persistence.Interceptors;
using TikTokShop.Infrastructure.Services;

namespace TikTokShop.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddScoped<AuditableEntityInterceptor>();

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"))
                   .UseSnakeCaseNamingConvention();
            options.AddInterceptors(sp.GetRequiredService<AuditableEntityInterceptor>());
        });
        services.AddScoped<IApplicationDbContext>(sp =>
            sp.GetRequiredService<ApplicationDbContext>());

        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
        services.AddSingleton<IJwtService, JwtService>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();

        var jwtSettings = configuration.GetSection("Jwt").Get<JwtSettings>()!;
        var key = Encoding.UTF8.GetBytes(jwtSettings.SecretKey);

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtSettings.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("RequireAdmin", p =>
                p.RequireAuthenticatedUser().RequireRole(UserRole.Admin.ToString()));
            options.AddPolicy("RequireManagerOrAbove", p =>
                p.RequireAuthenticatedUser().RequireRole(
                    UserRole.Admin.ToString(), UserRole.Manager.ToString()));
            options.AddPolicy("RequireAuthenticated", p =>
                p.RequireAuthenticatedUser());
        });

        // Phase 7 — TikTok integration
        services.AddDataProtection();
        services.AddMemoryCache();
        services.Configure<TikTokSettings>(configuration.GetSection("TikTok"));
        services.AddSingleton<ITikTokTokenProtector, TikTokTokenProtector>();
        services.AddSingleton<ITikTokWebhookSignatureVerifier, TikTokWebhookSignatureVerifier>();
        services.AddSingleton<IOAuthStateCache, MemoryOAuthStateCache>();
        services.AddSingleton<ITikTokRateLimiter, TikTokRateLimiter>();
        services.AddHttpClient<ITikTokApiClient, TikTokApiClient>();

        // Phase 7.7 — Outbox
        services.AddScoped<IOutboxService, OutboxService>();

        return services;
    }
}
