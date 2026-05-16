export interface StockOutDto {
  id: string
  productId: string
  productCode: string
  productName: string
  quantity: number
  unitPrice: number
  totalAmount: number
  transactionDate: string
  note?: string
  createdAt: string
  updatedAt?: string
}

export interface CreateStockOutRequest {
  productId: string
  quantity: number
  unitPrice: number
  transactionDate: string
  note?: string
}

export interface UpdateStockOutRequest {
  transactionDate: string
  note?: string
}

export interface StockOutQueryParams {
  pageNumber?: number
  pageSize?: number
  sortBy?: string
  sortDirection?: string
  productId?: string
  dateFrom?: string
  dateTo?: string
  search?: string
}
