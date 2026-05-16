namespace TikTokShop.Application.Features.StockMovements;

/// <summary>
/// Carries the source-document reference for a StockMovement.
/// Exactly one property must be non-null.
/// </summary>
public sealed class StockMovementReference
{
    public Guid? StockInId { get; init; }
    public Guid? StockOutId { get; init; }
    public Guid? TikTokOrderItemId { get; init; }
    public Guid? TikTokReturnLineId { get; init; }

    public int SetCount =>
        (StockInId.HasValue ? 1 : 0) +
        (StockOutId.HasValue ? 1 : 0) +
        (TikTokOrderItemId.HasValue ? 1 : 0) +
        (TikTokReturnLineId.HasValue ? 1 : 0);
}
