import apiClient from '@/shared/lib/api-client'
import type { ApiResponse, PaginatedResult } from '@/shared/types/api'
import type { CreateSupplierRequest, SupplierDto, SupplierQueryParams, UpdateSupplierRequest } from '../types'

export const supplierApi = {
  list: (params: SupplierQueryParams) =>
    apiClient.get<ApiResponse<PaginatedResult<SupplierDto>>>('/api/suppliers', { params }).then((r) => r.data),

  getById: (id: string) =>
    apiClient.get<ApiResponse<SupplierDto>>(`/api/suppliers/${id}`).then((r) => r.data),

  create: (data: CreateSupplierRequest) =>
    apiClient.post<ApiResponse<SupplierDto>>('/api/suppliers', data).then((r) => r.data),

  update: (id: string, data: UpdateSupplierRequest) =>
    apiClient.put<ApiResponse<SupplierDto>>(`/api/suppliers/${id}`, data).then((r) => r.data),

  delete: (id: string) =>
    apiClient.delete<ApiResponse<null>>(`/api/suppliers/${id}`).then((r) => r.data),
}
