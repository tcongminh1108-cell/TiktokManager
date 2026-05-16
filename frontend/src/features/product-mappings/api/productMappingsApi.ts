import apiClient from '@/shared/lib/api-client'
import type { ApiResponse, PaginatedResult } from '@/shared/types/api'
import type {
  CreateProductMappingRequest,
  ProductMappingDto,
  ProductMappingQueryParams,
  TikTokProductListResponse,
} from '../types'

export const productMappingsApi = {
  list: (params: ProductMappingQueryParams) =>
    apiClient
      .get<ApiResponse<PaginatedResult<ProductMappingDto>>>('/api/product-mappings', { params })
      .then((r) => r.data),

  create: (data: CreateProductMappingRequest) =>
    apiClient
      .post<ApiResponse<ProductMappingDto>>('/api/product-mappings', data)
      .then((r) => r.data),

  delete: (id: string) =>
    apiClient.delete<ApiResponse<null>>(`/api/product-mappings/${id}`).then((r) => r.data),

  getTikTokSkus: (connectionId: string, search?: string, nextPageToken?: string) =>
    apiClient
      .get<ApiResponse<TikTokProductListResponse>>(
        `/api/product-mappings/tiktok-skus/${connectionId}`,
        { params: { search, nextPageToken } },
      )
      .then((r) => r.data),

  getSuggestions: (connectionId: string, productId: string) =>
    apiClient
      .get<ApiResponse<TikTokProductListResponse['products']>>(
        `/api/product-mappings/suggestions/${connectionId}/${productId}`,
      )
      .then((r) => r.data),
}
