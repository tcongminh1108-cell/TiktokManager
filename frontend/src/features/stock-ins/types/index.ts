export interface StockInDto {
  id: string
  productId: string
  productCode: string
  productName: string
  supplierId: string
  supplierName: string
  quantity: number
  unitPrice: number
  totalAmount: number
  transactionDate: string
  note?: string
  createdAt: string
  updatedAt?: string
}

export interface CreateStockInRequest {
  productId: string
  supplierId: string
  quantity: number
  unitPrice: number
  transactionDate: string
  note?: string
}

export interface UpdateStockInRequest {
  transactionDate: string
  note?: string
}

export interface StockInQueryParams {
  pageNumber?: number
  pageSize?: number
  sortBy?: string
  sortDirection?: string
  productId?: string
  supplierId?: string
  dateFrom?: string
  dateTo?: string
}
