import apiClient from '@/shared/lib/api-client'
import type { ApiResponse } from '@/shared/types/api'
import type { TikTokConnectionDto, TikTokAuthUrlResponse } from '../types'

export const tikTokShopsApi = {
  getAuthUrl: (redirectAfter?: string) =>
    apiClient
      .get<ApiResponse<TikTokAuthUrlResponse>>('/api/tiktok-shops/auth-url', {
        params: redirectAfter ? { redirectAfter } : undefined,
      })
      .then((r) => r.data),

  list: () =>
    apiClient.get<ApiResponse<TikTokConnectionDto[]>>('/api/tiktok-shops').then((r) => r.data),

  getById: (id: string) =>
    apiClient.get<ApiResponse<TikTokConnectionDto>>(`/api/tiktok-shops/${id}`).then((r) => r.data),

  delete: (id: string) =>
    apiClient.delete<ApiResponse<null>>(`/api/tiktok-shops/${id}`).then((r) => r.data),

  refreshToken: (id: string) =>
    apiClient.post<ApiResponse<null>>(`/api/tiktok-shops/${id}/refresh-token`).then((r) => r.data),
}
