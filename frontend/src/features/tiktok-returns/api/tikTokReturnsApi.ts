import apiClient from '@/shared/lib/api-client'
import type { ApiResponse, PaginatedResult } from '@/shared/types/api'
import type { TikTokReturnDto, TikTokReturnQueryParams } from '../types'

export const tikTokReturnsApi = {
  list: (params: TikTokReturnQueryParams) =>
    apiClient
      .get<ApiResponse<PaginatedResult<TikTokReturnDto>>>('/api/tiktok-returns', { params })
      .then((r) => r.data),
}
