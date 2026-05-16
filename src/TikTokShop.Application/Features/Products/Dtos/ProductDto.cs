namespace TikTokShop.Application.Features.Products.Dtos;

public record ProductDto(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    decimal SellingPrice,
    string Unit,
    string? ImageUrl,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
);
