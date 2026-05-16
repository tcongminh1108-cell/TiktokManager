export type TikTokOrderSyncStatus =
  | 'Synced'
  | 'MappingPending'
  | 'Reserved'
  | 'StockApplied'
  | 'StockReversed'
  | 'Failed'

export interface TikTokOrderDto {
  id: string
  orderId: string
  shopName: string
  statusCode: number
  statusLabel: string
  syncStatus: TikTokOrderSyncStatus
  totalAmount: number
  currency: string
  buyerUsername: string | null
  createdAt: string
  updatedAt: string
}

export interface TikTokOrderQueryParams {
  pageNumber?: number
  pageSize?: number
  connectionId?: string
  statusCode?: number
  search?: string
}
