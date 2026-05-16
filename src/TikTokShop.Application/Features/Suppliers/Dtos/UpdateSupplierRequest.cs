namespace TikTokShop.Application.Features.Suppliers.Dtos;

public record UpdateSupplierRequest(
    string Name,
    string? Phone,
    string? Email,
    string? Address,
    string? Note
);
