import apiClient from '@/shared/lib/api-client'
import type { ApiResponse, PaginatedResult } from '@/shared/types/api'
import type { TikTokOrderDto, TikTokOrderQueryParams } from '../types'

export const tikTokOrdersApi = {
  list: (params: TikTokOrderQueryParams) =>
    apiClient
      .get<ApiResponse<PaginatedResult<TikTokOrderDto>>>('/api/tiktok-orders', { params })
      .then((r) => r.data),

  getDetail: (id: string) =>
    apiClient.get<ApiResponse<string>>(`/api/tiktok-orders/${id}/raw`).then((r) => r.data),

  syncNow: (connectionId: string) =>
    apiClient
      .post<ApiResponse<null>>(`/api/tiktok-orders/sync/${connectionId}`)
      .then((r) => r.data),
}
