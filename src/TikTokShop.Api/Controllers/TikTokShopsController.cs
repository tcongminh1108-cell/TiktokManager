using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TikTokShop.Application.Common.Models;
using TikTokShop.Application.Features.TikTok.Connections;
using TikTokShop.Application.Features.TikTok.Connections.Dtos;
using TikTokShop.Infrastructure.ExternalServices.TikTok;

namespace TikTokShop.Api.Controllers;

[ApiController]
[Route("api/tiktok-shops")]
[Authorize(Policy = "RequireAuthenticated")]
public class TikTokShopsController(
    ITikTokConnectionService connectionService,
    IOptions<TikTokSettings> tikTokOptions) : ControllerBase
{
    // GET /api/tiktok-shops/auth-url?redirectAfter=...
    // Returns the TikTok OAuth URL for the tenant admin to visit.
    [HttpGet("auth-url")]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<ActionResult<ApiResponse<TikTokAuthUrlResponse>>> GetAuthUrl(
        [FromQuery] string? redirectAfter, CancellationToken ct)
    {
        var result = await connectionService.GetAuthUrlAsync(redirectAfter, ct);
        var settings = tikTokOptions.Value;

        // Build the full TikTok OAuth URL using state from service + app_key from config.
        var fullUrl = $"{settings.AuthBaseUrl}/open/authorize" +
                      $"?app_key={Uri.EscapeDataString(settings.AppKey)}" +
                      $"&state={Uri.EscapeDataString(result.State)}";

        return Ok(ApiResponse.Ok(new TikTokAuthUrlResponse(fullUrl, result.State)));
    }

    // GET /api/tiktok-shops/callback?code=...&state=...
    // TikTok redirects here after user grants access. Exchanges code, saves connections.
    [HttpGet("callback")]
    [AllowAnonymous]
    public async Task<IActionResult> Callback(
        [FromQuery] string code, [FromQuery] string state,
        [FromQuery] string? redirectAfter, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(state))
            return BadRequest("Missing code or state.");

        // NOTE: This endpoint is accessed by the OAuth redirect — user context may not be set.
        // Alternatively, the frontend can handle the redirect and call a POST endpoint instead.
        var count = await connectionService.HandleCallbackAsync(code, state, ct);
        var destination = string.IsNullOrEmpty(redirectAfter)
            ? "/"
            : redirectAfter;

        return Redirect($"{destination}?connected=true&shopCount={count}");
    }

    // GET /api/tiktok-shops
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<TikTokConnectionDto>>>> GetConnections(CancellationToken ct)
    {
        var result = await connectionService.GetConnectionsAsync(ct);
        return Ok(ApiResponse.Ok(result));
    }

    // GET /api/tiktok-shops/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<TikTokConnectionDto>>> GetConnection(Guid id, CancellationToken ct)
    {
        var result = await connectionService.GetConnectionByIdAsync(id, ct);
        return Ok(ApiResponse.Ok(result));
    }

    // DELETE /api/tiktok-shops/{id}
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteConnection(Guid id, CancellationToken ct)
    {
        await connectionService.DeleteConnectionAsync(id, ct);
        return Ok(ApiResponse.Ok<object>(null!, "Connection removed."));
    }

    // POST /api/tiktok-shops/{id}/refresh-token
    [HttpPost("{id:guid}/refresh-token")]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<ActionResult<ApiResponse<object>>> RefreshToken(Guid id, CancellationToken ct)
    {
        await connectionService.RefreshTokenAsync(id, ct);
        return Ok(ApiResponse.Ok<object>(null!, "Token refreshed."));
    }
}
