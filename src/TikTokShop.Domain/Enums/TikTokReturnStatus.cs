namespace TikTokShop.Domain.Enums;

public enum TikTokReturnStatus
{
    Requested = 1,      // Buyer submitted return request
    Approved = 2,       // Seller approved the return
    Rejected = 3,       // Seller rejected the return
    ReturnReceived = 4, // Physical goods received back → triggers StockMovement ReturnIn
    Refunded = 5,       // Refund issued to buyer
    Closed = 6          // Return case closed
}
