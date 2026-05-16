namespace TikTokShop.Application.Features.Products.Dtos;

public record UpdateProductRequest(
    string Name,
    string? Description,
    decimal SellingPrice,
    string Unit,
    string? ImageUrl
);
