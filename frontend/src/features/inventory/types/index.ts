export interface InventoryItemDto {
  productId: string
  productCode: string
  productName: string
  sellingPrice: number
  physicalStock: number
  reservedQuantity: number
  availableStock: number
  avgCostPrice?: number
  estimatedValue?: number
}

export interface MovementHistoryDto {
  id: string
  type: string
  source: string
  quantity: number
  unitCost: number
  occurredAt: string
  note?: string
  idempotencyKey: string
}

export interface ActiveReservationDto {
  id: string
  quantity: number
  tikTokOrderItemId?: string
  reservedAt: string
  expiresAt: string
  idempotencyKey: string
}

export interface InventoryDetailDto {
  summary: InventoryItemDto
  movementHistory: {
    items: MovementHistoryDto[]
    pageNumber: number
    pageSize: number
    totalCount: number
    totalPages: number
    hasPrevious: boolean
    hasNext: boolean
  }
  activeReservations: ActiveReservationDto[]
}

export interface InventoryQueryParams {
  pageNumber?: number
  pageSize?: number
  sortBy?: string
  sortDirection?: string
  search?: string
  isActive?: boolean
}

export interface InventoryDetailQueryParams {
  pageNumber?: number
  pageSize?: number
}
