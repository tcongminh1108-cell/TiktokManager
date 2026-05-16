import apiClient from '@/shared/lib/api-client'
import type { ApiResponse, PaginatedResult } from '@/shared/types/api'
import type { CreateStockInRequest, StockInDto, StockInQueryParams, UpdateStockInRequest } from '../types'

export const stockInApi = {
  list: (params: StockInQueryParams) =>
    apiClient.get<ApiResponse<PaginatedResult<StockInDto>>>('/api/stock-ins', { params }).then((r) => r.data),

  create: (data: CreateStockInRequest) =>
    apiClient.post<ApiResponse<StockInDto>>('/api/stock-ins', data).then((r) => r.data),

  update: (id: string, data: UpdateStockInRequest) =>
    apiClient.put<ApiResponse<StockInDto>>(`/api/stock-ins/${id}`, data).then((r) => r.data),

  delete: (id: string) =>
    apiClient.delete<ApiResponse<null>>(`/api/stock-ins/${id}`).then((r) => r.data),
}
