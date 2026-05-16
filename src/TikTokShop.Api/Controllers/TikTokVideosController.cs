using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TikTokShop.Application.Common.Models;
using TikTokShop.Application.Features.TikTok.Videos;
using TikTokShop.Application.Features.TikTok.Videos.Dtos;

namespace TikTokShop.Api.Controllers;

[ApiController]
[Route("api/tiktok-videos")]
[Authorize(Policy = "RequireManagerOrAbove")]
public class TikTokVideosController(ITikTokVideoService videoService) : ControllerBase
{
    /// <summary>List synced TikTok videos (paginated, sorted by view count desc).</summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResult<TikTokVideoDto>>>> GetVideos(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] Guid? connectionId = null,
        CancellationToken ct = default)
    {
        var filter = new TikTokVideoQueryParams(pageNumber, pageSize, connectionId);
        var result = await videoService.GetVideosAsync(filter, ct);
        return Ok(ApiResponse.Ok(result));
    }

    /// <summary>Get a single video with its latest metrics.</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<TikTokVideoDto>>> GetVideo(
        Guid id, CancellationToken ct = default)
    {
        var result = await videoService.GetVideoByIdAsync(id, ct);
        if (result is null) return NotFound(ApiResponse.Fail<TikTokVideoDto>("Video not found."));
        return Ok(ApiResponse.Ok(result));
    }

    /// <summary>Get growth chart data (all metric snapshots) for a video.</summary>
    [HttpGet("{id:guid}/growth")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<VideoGrowthPointDto>>>> GetVideoGrowth(
        Guid id, CancellationToken ct = default)
    {
        var result = await videoService.GetVideoGrowthAsync(id, ct);
        return Ok(ApiResponse.Ok(result));
    }
}
