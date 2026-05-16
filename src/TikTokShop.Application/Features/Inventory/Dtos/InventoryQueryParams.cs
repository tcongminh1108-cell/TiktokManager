using TikTokShop.Application.Common.Models;

namespace TikTokShop.Application.Features.Inventory.Dtos;

public class InventoryQueryParams : PageRequest
{
    /// <summary>Filter by active/inactive products. Null = all.</summary>
    public bool? IsActive { get; set; }
}
