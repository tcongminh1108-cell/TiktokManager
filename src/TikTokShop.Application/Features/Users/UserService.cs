using Microsoft.EntityFrameworkCore;
using TikTokShop.Application.Common.Models;
using TikTokShop.Application.Features.Users.Dtos;
using TikTokShop.Application.Interfaces;
using TikTokShop.Domain.Entities;
using TikTokShop.Domain.Enums;
using TikTokShop.Domain.Exceptions;
using TikTokShop.Domain.Interfaces;

namespace TikTokShop.Application.Features.Users;

public sealed class UserService(
    IApplicationDbContext db,
    IPasswordHasher passwordHasher,
    ICurrentUser currentUser) : IUserService
{
    public async Task<PaginatedResult<UserDto>> GetUsersAsync(UserQueryParams query)
    {
        var q = db.Users.AsQueryable();

        if (query.Role.HasValue)
            q = q.Where(u => u.Role == query.Role.Value);

        if (query.IsActive.HasValue)
            q = q.Where(u => u.IsActive == query.IsActive.Value);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.ToLower();
            q = q.Where(u => u.FullName.ToLower().Contains(search) ||
                              u.Email.ToLower().Contains(search));
        }

        var totalCount = await q.CountAsync();

        var sortBy = query.SortBy?.ToLower();
        q = sortBy switch
        {
            "email" => query.SortDirection == "desc" ? q.OrderByDescending(u => u.Email) : q.OrderBy(u => u.Email),
            "fullname" => query.SortDirection == "desc" ? q.OrderByDescending(u => u.FullName) : q.OrderBy(u => u.FullName),
            "role" => query.SortDirection == "desc" ? q.OrderByDescending(u => u.Role) : q.OrderBy(u => u.Role),
            _ => query.SortDirection == "desc" ? q.OrderByDescending(u => u.CreatedAt) : q.OrderBy(u => u.CreatedAt)
        };

        var items = await q
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(u => ToDto(u))
            .ToListAsync();

        return new PaginatedResult<UserDto>(items, totalCount, query.PageNumber, query.PageSize);
    }

    public async Task<UserDto> GetUserByIdAsync(Guid id)
    {
        var user = await FindOrThrowAsync(id);
        return ToDto(user);
    }

    public async Task<UserDto> CreateUserAsync(CreateUserRequest request)
    {
        var emailExists = await db.Users
            .AnyAsync(u => u.Email == request.Email.ToLower());

        if (emailExists)
            throw new ConflictException("User", "email", request.Email);

        var user = new User
        {
            TenantId = currentUser.TenantId,
            Email = request.Email.ToLower(),
            PasswordHash = passwordHasher.Hash(request.Password),
            FullName = request.FullName,
            Role = request.Role,
            IsActive = true
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        return ToDto(user);
    }

    public async Task<UserDto> UpdateUserAsync(Guid id, UpdateUserRequest request)
    {
        var user = await FindOrThrowAsync(id);

        if (user.Role == UserRole.Admin && request.Role != UserRole.Admin)
            await EnsureNotLastAdminAsync(id);

        user.FullName = request.FullName;
        user.Role = request.Role;

        await db.SaveChangesAsync();

        return ToDto(user);
    }

    public async Task ChangePasswordAsync(Guid id, ChangePasswordRequest request)
    {
        var user = await FindOrThrowAsync(id);
        var callerId = currentUser.UserId;

        // Self-change requires current password; admin changing another user's password does not
        if (callerId == id)
        {
            if (string.IsNullOrEmpty(request.CurrentPassword))
                throw new BusinessRuleException("Current password is required when changing your own password.");

            if (!passwordHasher.Verify(request.CurrentPassword, user.PasswordHash))
                throw new BusinessRuleException("Current password is incorrect.");
        }

        user.PasswordHash = passwordHasher.Hash(request.NewPassword);
        await db.SaveChangesAsync();
    }

    public async Task DeleteUserAsync(Guid id)
    {
        var user = await FindOrThrowAsync(id);

        if (user.Role == UserRole.Admin)
            await EnsureNotLastAdminAsync(id);

        user.IsDeleted = true;
        user.DeletedAt = DateTimeOffset.UtcNow;
        user.DeletedBy = currentUser.UserId;

        await db.SaveChangesAsync();
    }

    public async Task ActivateUserAsync(Guid id)
    {
        var user = await FindOrThrowAsync(id);
        user.IsActive = true;
        await db.SaveChangesAsync();
    }

    public async Task DeactivateUserAsync(Guid id)
    {
        var user = await FindOrThrowAsync(id);

        if (user.Role == UserRole.Admin)
            await EnsureNotLastAdminAsync(id);

        user.IsActive = false;
        await db.SaveChangesAsync();
    }

    private async Task<User> FindOrThrowAsync(Guid id)
    {
        return await db.Users.FirstOrDefaultAsync(u => u.Id == id)
            ?? throw new NotFoundException("User", id);
    }

    private async Task EnsureNotLastAdminAsync(Guid excludeUserId)
    {
        var adminCount = await db.Users
            .CountAsync(u => u.Role == UserRole.Admin && u.Id != excludeUserId && u.IsActive);

        if (adminCount == 0)
            throw new BusinessRuleException("Cannot perform this action: at least one active Admin must remain in the tenant.");
    }

    private static UserDto ToDto(User u) =>
        new(u.Id, u.TenantId, u.Email, u.FullName, u.Role, u.IsActive, u.LastLoginAt, u.CreatedAt);
}
