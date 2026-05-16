using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TikTokShop.Application.Common.Models;
using TikTokShop.Application.Features.TikTok.Webhooks;
using TikTokShop.Application.Features.TikTok.Webhooks.Dtos;

namespace TikTokShop.Api.Controllers;

[ApiController]
[Route("api/webhook-events")]
[Authorize(Policy = "RequireAdmin")]
public class WebhookEventsController(IWebhookEventService webhookEventService) : ControllerBase
{
    // GET /api/webhook-events?pageNumber=1&pageSize=20&status=...&eventType=...
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResult<WebhookEventDto>>>> GetEvents(
        [FromQuery] WebhookEventQueryParams query, CancellationToken ct)
    {
        var result = await webhookEventService.GetEventsAsync(query, ct);
        return Ok(ApiResponse.Ok(result));
    }

    // POST /api/webhook-events/{id}/retry
    [HttpPost("{id:guid}/retry")]
    public async Task<ActionResult<ApiResponse<object>>> RetryEvent(Guid id, CancellationToken ct)
    {
        await webhookEventService.RetryEventAsync(id, ct);
        return Ok(ApiResponse.Ok<object>(null!, "Event queued for retry."));
    }
}
