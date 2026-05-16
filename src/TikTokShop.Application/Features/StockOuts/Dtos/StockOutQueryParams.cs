using TikTokShop.Application.Common.Models;

namespace TikTokShop.Application.Features.StockOuts.Dtos;

public class StockOutQueryParams : PageRequest
{
    public Guid? ProductId { get; set; }
    public DateTimeOffset? DateFrom { get; set; }
    public DateTimeOffset? DateTo { get; set; }
}
