using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TikTokShop.Application.Common.Models;
using TikTokShop.Application.Features.DevSeed;
using TikTokShop.Domain.Exceptions;

namespace TikTokShop.Api.Controllers;

[ApiController]
[Route("api/dev")]
[Authorize(Policy = "RequireAdmin")]
public class DevController(
    IDevSeedService devSeedService,
    IWebHostEnvironment env) : ControllerBase
{
    [HttpPost("seed")]
    public async Task<ActionResult<ApiResponse<DevSeedResult>>> Seed(CancellationToken ct)
    {
        if (!env.IsDevelopment())
            throw new ForbiddenException("Seed endpoint is only available in Development environment.");

        var result = await devSeedService.SeedAsync(ct);
        return Ok(ApiResponse.Ok(result, "Seed complete."));
    }
}
