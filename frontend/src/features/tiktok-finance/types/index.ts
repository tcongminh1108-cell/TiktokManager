export interface TikTokStatementDto {
  id: string
  statementId: string
  shopName: string
  statementType: string
  currency: string
  totalAmount: number
  status: string
  statementDate: string
  createdAt: string
}

export interface TikTokStatementQueryParams {
  pageNumber?: number
  pageSize?: number
  connectionId?: string
  fromDate?: string
  toDate?: string
}
