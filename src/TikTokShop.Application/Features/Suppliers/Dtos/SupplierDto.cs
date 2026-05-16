namespace TikTokShop.Application.Features.Suppliers.Dtos;

public record SupplierDto(
    Guid Id,
    string Code,
    string Name,
    string? Phone,
    string? Email,
    string? Address,
    string? Note,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
);
