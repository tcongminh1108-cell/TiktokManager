import apiClient from '@/shared/lib/api-client'
import type { ApiResponse, PaginatedResult } from '@/shared/types/api'
import type { CreateProductRequest, ProductDto, ProductQueryParams, UpdateProductRequest } from '../types'

export const productApi = {
  list: (params: ProductQueryParams) =>
    apiClient
      .get<ApiResponse<PaginatedResult<ProductDto>>>('/api/products', { params })
      .then((r) => r.data),

  getById: (id: string) =>
    apiClient.get<ApiResponse<ProductDto>>(`/api/products/${id}`).then((r) => r.data),

  create: (data: CreateProductRequest) =>
    apiClient.post<ApiResponse<ProductDto>>('/api/products', data).then((r) => r.data),

  update: (id: string, data: UpdateProductRequest) =>
    apiClient.put<ApiResponse<ProductDto>>(`/api/products/${id}`, data).then((r) => r.data),

  delete: (id: string) =>
    apiClient.delete<ApiResponse<null>>(`/api/products/${id}`).then((r) => r.data),

  restore: (id: string) =>
    apiClient.post<ApiResponse<null>>(`/api/products/${id}/restore`).then((r) => r.data),

  activate: (id: string) =>
    apiClient.put<ApiResponse<null>>(`/api/products/${id}/activate`).then((r) => r.data),

  deactivate: (id: string) =>
    apiClient.put<ApiResponse<null>>(`/api/products/${id}/deactivate`).then((r) => r.data),
}
