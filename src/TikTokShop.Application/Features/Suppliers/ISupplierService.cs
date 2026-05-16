using TikTokShop.Application.Common.Models;
using TikTokShop.Application.Features.Suppliers.Dtos;

namespace TikTokShop.Application.Features.Suppliers;

public interface ISupplierService
{
    Task<PaginatedResult<SupplierDto>> GetSuppliersAsync(SupplierQueryParams query);
    Task<SupplierDto> GetSupplierByIdAsync(Guid id);
    Task<SupplierDto> CreateSupplierAsync(CreateSupplierRequest request);
    Task<SupplierDto> UpdateSupplierAsync(Guid id, UpdateSupplierRequest request);
    Task DeleteSupplierAsync(Guid id);
    Task RestoreSupplierAsync(Guid id);
}
