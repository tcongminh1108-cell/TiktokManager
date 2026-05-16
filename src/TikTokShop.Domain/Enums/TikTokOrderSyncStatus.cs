namespace TikTokShop.Domain.Enums;

public enum TikTokOrderSyncStatus
{
    MappingPending = 1,  // No ProductTikTokMapping found for this SKU
    Reserved = 2,        // Active reservation created (AwaitingShipment)
    StockApplied = 3,    // Out stock movement recorded (InTransit/Delivered/Completed)
    StockReversed = 4,   // Compensating In movement recorded (Cancelled after commit)
    Released = 5,        // Reservation released (Cancelled before shipment)
    Synced = 6,          // No stock action required (Unpaid, AwaitingCollection)
    Failed = 7           // Processing failed permanently after max retries
}
