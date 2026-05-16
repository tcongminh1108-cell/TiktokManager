using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TikTokShop.Application.Common.Models;
using TikTokShop.Application.Features.Auth;
using TikTokShop.Application.Features.Auth.Dtos;

namespace TikTokShop.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService, IValidator<RegisterTenantRequest> registerValidator,
    IValidator<LoginRequest> loginValidator, IValidator<RefreshTokenRequest> refreshValidator) : ControllerBase
{
    [HttpPost("register-tenant")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> RegisterTenant(
        [FromBody] RegisterTenantRequest request)
    {
        await registerValidator.ValidateAndThrowAsync(request);
        var result = await authService.RegisterTenantAsync(request, GetIpAddress());
        return Ok(ApiResponse.Ok(result));
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Login(
        [FromBody] LoginRequest request)
    {
        await loginValidator.ValidateAndThrowAsync(request);
        var result = await authService.LoginAsync(request, GetIpAddress());
        return Ok(ApiResponse.Ok(result));
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Refresh(
        [FromBody] RefreshTokenRequest request)
    {
        await refreshValidator.ValidateAndThrowAsync(request);
        var result = await authService.RefreshTokenAsync(request, GetIpAddress());
        return Ok(ApiResponse.Ok(result));
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<ActionResult<ApiResponse<object>>> Logout(
        [FromBody] RefreshTokenRequest request)
    {
        await authService.LogoutAsync(request.RefreshToken);
        return Ok(ApiResponse.Ok<object>(null!, "Logged out successfully."));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<CurrentUserDto>>> Me()
    {
        var result = await authService.GetCurrentUserAsync();
        return Ok(ApiResponse.Ok(result));
    }

    private string? GetIpAddress() =>
        HttpContext.Connection.RemoteIpAddress?.ToString();
}
