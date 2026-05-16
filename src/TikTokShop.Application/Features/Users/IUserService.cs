using TikTokShop.Application.Common.Models;
using TikTokShop.Application.Features.Users.Dtos;

namespace TikTokShop.Application.Features.Users;

public interface IUserService
{
    Task<PaginatedResult<UserDto>> GetUsersAsync(UserQueryParams query);
    Task<UserDto> GetUserByIdAsync(Guid id);
    Task<UserDto> CreateUserAsync(CreateUserRequest request);
    Task<UserDto> UpdateUserAsync(Guid id, UpdateUserRequest request);
    Task ChangePasswordAsync(Guid id, ChangePasswordRequest request);
    Task DeleteUserAsync(Guid id);
    Task ActivateUserAsync(Guid id);
    Task DeactivateUserAsync(Guid id);
}
