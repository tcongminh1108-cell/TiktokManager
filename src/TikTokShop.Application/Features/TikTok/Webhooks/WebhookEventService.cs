using Microsoft.EntityFrameworkCore;
using TikTokShop.Application.Common.Models;
using TikTokShop.Application.Features.TikTok.Webhooks.Dtos;
using TikTokShop.Application.Interfaces;
using TikTokShop.Domain.Enums;
using TikTokShop.Domain.Exceptions;

namespace TikTokShop.Application.Features.TikTok.Webhooks;

public sealed class WebhookEventService(
    IApplicationDbContext db) : IWebhookEventService
{
    public async Task<PaginatedResult<WebhookEventDto>> GetEventsAsync(
        WebhookEventQueryParams query, CancellationToken ct = default)
    {
        var q = db.WebhookEvents.AsQueryable();

        if (query.Status.HasValue)
            q = q.Where(e => e.Status == query.Status.Value);

        if (!string.IsNullOrWhiteSpace(query.EventType))
            q = q.Where(e => e.EventType == query.EventType);

        if (query.ConnectionId.HasValue)
            q = q.Where(e => e.ConnectionId == query.ConnectionId.Value);

        var totalCount = await q.CountAsync(ct);

        q = query.SortBy?.ToLower() switch
        {
            "eventtype" => query.SortDirection == "desc"
                ? q.OrderByDescending(e => e.EventType) : q.OrderBy(e => e.EventType),
            "status" => query.SortDirection == "desc"
                ? q.OrderByDescending(e => e.Status) : q.OrderBy(e => e.Status),
            _ => query.SortDirection == "desc"
                ? q.OrderByDescending(e => e.ReceivedAt) : q.OrderByDescending(e => e.ReceivedAt)
        };

        var items = await q
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(e => new WebhookEventDto(
                e.Id, e.ConnectionId, e.EventId, e.EventType, e.Source, e.Status,
                e.ReceivedAt, e.ProcessedAt, e.RetryCount, e.LastError))
            .ToListAsync(ct);

        return new PaginatedResult<WebhookEventDto>(items, totalCount, query.PageNumber, query.PageSize);
    }

    public async Task RetryEventAsync(Guid id, CancellationToken ct = default)
    {
        var ev = await db.WebhookEvents
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(e => e.Id == id, ct)
            ?? throw new NotFoundException("WebhookEvent", id);

        if (ev.Status != WebhookEventStatus.Failed)
            throw new BusinessRuleException("Only Failed events can be manually retried.");

        ev.Status = WebhookEventStatus.Received;
        ev.RetryCount = 0;
        ev.LastError = null;

        await db.SaveChangesAsync(ct);
    }
}
