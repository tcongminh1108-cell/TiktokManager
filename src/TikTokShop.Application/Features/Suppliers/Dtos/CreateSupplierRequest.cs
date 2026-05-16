namespace TikTokShop.Application.Features.Suppliers.Dtos;

public record CreateSupplierRequest(
    string Code,
    string Name,
    string? Phone,
    string? Email,
    string? Address,
    string? Note
);
