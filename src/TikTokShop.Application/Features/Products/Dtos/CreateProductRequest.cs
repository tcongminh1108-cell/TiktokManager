namespace TikTokShop.Application.Features.Products.Dtos;

public record CreateProductRequest(
    string Code,
    string Name,
    string? Description,
    decimal SellingPrice,
    string Unit,
    string? ImageUrl
);
