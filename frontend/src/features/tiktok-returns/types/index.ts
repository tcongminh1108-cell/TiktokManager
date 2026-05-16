export interface TikTokReturnDto {
  id: string
  returnId: string
  orderId: string
  shopName: string
  status: string
  returnReason: string | null
  refundAmount: number
  currency: string
  createdAt: string
  updatedAt: string
}

export interface TikTokReturnQueryParams {
  pageNumber?: number
  pageSize?: number
  connectionId?: string
  status?: string
}
