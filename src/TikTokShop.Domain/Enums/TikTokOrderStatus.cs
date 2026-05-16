namespace TikTokShop.Domain.Enums;

public enum TikTokOrderStatus
{
    Unpaid = 100,
    AwaitingShipment = 111,
    AwaitingCollection = 112,
    InTransit = 121,
    Delivered = 122,
    Completed = 130,
    Cancelled = 140
}
