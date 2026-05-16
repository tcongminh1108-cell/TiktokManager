import apiClient from '@/shared/lib/api-client'
import type { ApiResponse, PaginatedResult } from '@/shared/types/api'
import type { CreateStockOutRequest, StockOutDto, StockOutQueryParams, UpdateStockOutRequest } from '../types'

export const stockOutApi = {
  list: (params: StockOutQueryParams) =>
    apiClient.get<ApiResponse<PaginatedResult<StockOutDto>>>('/api/stock-outs', { params }).then((r) => r.data),

  create: (data: CreateStockOutRequest) =>
    apiClient.post<ApiResponse<StockOutDto>>('/api/stock-outs', data).then((r) => r.data),

  update: (id: string, data: UpdateStockOutRequest) =>
    apiClient.put<ApiResponse<StockOutDto>>(`/api/stock-outs/${id}`, data).then((r) => r.data),

  delete: (id: string) =>
    apiClient.delete<ApiResponse<null>>(`/api/stock-outs/${id}`).then((r) => r.data),
}
