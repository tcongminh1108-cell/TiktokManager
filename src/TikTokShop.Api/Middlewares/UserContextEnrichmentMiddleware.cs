using Serilog.Context;
using TikTokShop.Domain.Interfaces;

namespace TikTokShop.Api.Middlewares;

public class UserContextEnrichmentMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, ICurrentUser currentUser)
    {
        if (currentUser.IsAuthenticated)
        {
            using (LogContext.PushProperty("TenantId", currentUser.TenantId))
            using (LogContext.PushProperty("UserId", currentUser.UserId))
            {
                await next(context);
            }
        }
        else
        {
            await next(context);
        }
    }
}
