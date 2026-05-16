using Microsoft.EntityFrameworkCore;
using TikTokShop.Application.Common.Models;
using TikTokShop.Application.Features.Suppliers.Dtos;
using TikTokShop.Application.Interfaces;
using TikTokShop.Domain.Entities;
using TikTokShop.Domain.Exceptions;
using TikTokShop.Domain.Interfaces;

namespace TikTokShop.Application.Features.Suppliers;

public sealed class SupplierService(
    IApplicationDbContext db,
    ICurrentUser currentUser) : ISupplierService
{
    public async Task<PaginatedResult<SupplierDto>> GetSuppliersAsync(SupplierQueryParams query)
    {
        var q = db.Suppliers.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.ToLower();
            q = q.Where(s => s.Name.ToLower().Contains(search) ||
                              s.Code.ToLower().Contains(search));
        }

        var totalCount = await q.CountAsync();

        q = query.SortBy?.ToLower() switch
        {
            "code" => query.SortDirection == "desc" ? q.OrderByDescending(s => s.Code) : q.OrderBy(s => s.Code),
            "name" => query.SortDirection == "desc" ? q.OrderByDescending(s => s.Name) : q.OrderBy(s => s.Name),
            _ => query.SortDirection == "desc" ? q.OrderByDescending(s => s.CreatedAt) : q.OrderBy(s => s.CreatedAt)
        };

        var items = await q
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(s => ToDto(s))
            .ToListAsync();

        return new PaginatedResult<SupplierDto>(items, totalCount, query.PageNumber, query.PageSize);
    }

    public async Task<SupplierDto> GetSupplierByIdAsync(Guid id)
    {
        var supplier = await FindOrThrowAsync(id);
        return ToDto(supplier);
    }

    public async Task<SupplierDto> CreateSupplierAsync(CreateSupplierRequest request)
    {
        await EnsureCodeUniqueAsync(request.Code);

        var supplier = new Supplier
        {
            TenantId = currentUser.TenantId,
            Code = request.Code.Trim(),
            Name = request.Name.Trim(),
            Phone = request.Phone?.Trim(),
            Email = request.Email?.Trim().ToLower(),
            Address = request.Address?.Trim(),
            Note = request.Note?.Trim()
        };

        db.Suppliers.Add(supplier);
        await db.SaveChangesAsync();

        return ToDto(supplier);
    }

    public async Task<SupplierDto> UpdateSupplierAsync(Guid id, UpdateSupplierRequest request)
    {
        var supplier = await FindOrThrowAsync(id);

        supplier.Name = request.Name.Trim();
        supplier.Phone = request.Phone?.Trim();
        supplier.Email = request.Email?.Trim().ToLower();
        supplier.Address = request.Address?.Trim();
        supplier.Note = request.Note?.Trim();

        await db.SaveChangesAsync();

        return ToDto(supplier);
    }

    public async Task DeleteSupplierAsync(Guid id)
    {
        var supplier = await FindOrThrowAsync(id);

        supplier.IsDeleted = true;
        supplier.DeletedAt = DateTimeOffset.UtcNow;
        supplier.DeletedBy = currentUser.UserId;

        await db.SaveChangesAsync();
    }

    public async Task RestoreSupplierAsync(Guid id)
    {
        // IgnoreQueryFilters to find soft-deleted records; explicitly filter by TenantId for security
        var supplier = await db.Suppliers
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.Id == id && s.TenantId == currentUser.TenantId)
            ?? throw new NotFoundException("Supplier", id);

        if (!supplier.IsDeleted)
            return;

        await EnsureCodeUniqueAsync(supplier.Code, excludeId: id);

        supplier.IsDeleted = false;
        supplier.DeletedAt = null;
        supplier.DeletedBy = null;

        await db.SaveChangesAsync();
    }

    private async Task<Supplier> FindOrThrowAsync(Guid id) =>
        await db.Suppliers.FirstOrDefaultAsync(s => s.Id == id)
            ?? throw new NotFoundException("Supplier", id);

    private async Task EnsureCodeUniqueAsync(string code, Guid? excludeId = null)
    {
        var exists = await db.Suppliers
            .AnyAsync(s => s.Code == code.Trim() && (excludeId == null || s.Id != excludeId));

        if (exists)
            throw new ConflictException("Supplier", "code", code);
    }

    private static SupplierDto ToDto(Supplier s) =>
        new(s.Id, s.Code, s.Name, s.Phone, s.Email, s.Address, s.Note, s.CreatedAt, s.UpdatedAt);
}
