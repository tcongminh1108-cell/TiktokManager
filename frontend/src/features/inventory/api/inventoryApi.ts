import apiClient from '@/shared/lib/api-client'
import type { ApiResponse, PaginatedResult } from '@/shared/types/api'
import type { InventoryDetailDto, InventoryDetailQueryParams, InventoryItemDto, InventoryQueryParams } from '../types'

export const inventoryApi = {
  list: (params: InventoryQueryParams) =>
    apiClient.get<ApiResponse<PaginatedResult<InventoryItemDto>>>('/api/inventory', { params }).then((r) => r.data),

  getDetail: (productId: string, params: InventoryDetailQueryParams) =>
    apiClient.get<ApiResponse<InventoryDetailDto>>(`/api/inventory/${productId}`, { params }).then((r) => r.data),
}
