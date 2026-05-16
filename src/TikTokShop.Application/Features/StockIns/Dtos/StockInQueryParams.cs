using TikTokShop.Application.Common.Models;

namespace TikTokShop.Application.Features.StockIns.Dtos;

public class StockInQueryParams : PageRequest
{
    public Guid? ProductId { get; set; }
    public Guid? SupplierId { get; set; }
    public DateTimeOffset? DateFrom { get; set; }
    public DateTimeOffset? DateTo { get; set; }
}
