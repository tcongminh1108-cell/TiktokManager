using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TikTokShop.Application.Common.Models;
using TikTokShop.Application.Features.Users;
using TikTokShop.Application.Features.Users.Dtos;

namespace TikTokShop.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Roles = "Admin")]
public class UsersController(
    IUserService userService,
    IValidator<CreateUserRequest> createValidator,
    IValidator<UpdateUserRequest> updateValidator,
    IValidator<ChangePasswordRequest> changePasswordValidator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResult<UserDto>>>> GetUsers(
        [FromQuery] UserQueryParams query)
    {
        var result = await userService.GetUsersAsync(query);
        return Ok(ApiResponse.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetUser(Guid id)
    {
        var result = await userService.GetUserByIdAsync(id);
        return Ok(ApiResponse.Ok(result));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<UserDto>>> CreateUser(
        [FromBody] CreateUserRequest request)
    {
        await createValidator.ValidateAndThrowAsync(request);
        var result = await userService.CreateUserAsync(request);
        return CreatedAtAction(nameof(GetUser), new { id = result.Id }, ApiResponse.Ok(result));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<UserDto>>> UpdateUser(
        Guid id, [FromBody] UpdateUserRequest request)
    {
        await updateValidator.ValidateAndThrowAsync(request);
        var result = await userService.UpdateUserAsync(id, request);
        return Ok(ApiResponse.Ok(result));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteUser(Guid id)
    {
        await userService.DeleteUserAsync(id);
        return Ok(ApiResponse.Ok<object>(null!, "User deleted."));
    }

    [HttpPut("{id:guid}/activate")]
    public async Task<ActionResult<ApiResponse<object>>> ActivateUser(Guid id)
    {
        await userService.ActivateUserAsync(id);
        return Ok(ApiResponse.Ok<object>(null!, "User activated."));
    }

    [HttpPut("{id:guid}/deactivate")]
    public async Task<ActionResult<ApiResponse<object>>> DeactivateUser(Guid id)
    {
        await userService.DeactivateUserAsync(id);
        return Ok(ApiResponse.Ok<object>(null!, "User deactivated."));
    }

    [HttpPut("{id:guid}/change-password")]
    public async Task<ActionResult<ApiResponse<object>>> ChangePassword(
        Guid id, [FromBody] ChangePasswordRequest request)
    {
        await changePasswordValidator.ValidateAndThrowAsync(request);
        await userService.ChangePasswordAsync(id, request);
        return Ok(ApiResponse.Ok<object>(null!, "Password changed."));
    }
}
