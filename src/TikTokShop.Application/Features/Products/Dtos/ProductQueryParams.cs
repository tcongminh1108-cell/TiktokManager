using TikTokShop.Application.Common.Models;

namespace TikTokShop.Application.Features.Products.Dtos;

public class ProductQueryParams : PageRequest
{
    public bool? IsActive { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
}
