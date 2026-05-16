using TikTokShop.Application.Common.Models;
using TikTokShop.Domain.Enums;

namespace TikTokShop.Application.Features.Users.Dtos;

public class UserQueryParams : PageRequest
{
    public UserRole? Role { get; set; }
    public bool? IsActive { get; set; }
}
