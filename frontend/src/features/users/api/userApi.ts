import apiClient from '@/shared/lib/api-client'
import type { ApiResponse, PaginatedResult } from '@/shared/types/api'
import type { ChangePasswordRequest, CreateUserRequest, UpdateUserRequest, UserDto, UserQueryParams } from '../types'

export const userApi = {
  list: (params: UserQueryParams) =>
    apiClient.get<ApiResponse<PaginatedResult<UserDto>>>('/api/users', { params }).then((r) => r.data),

  create: (data: CreateUserRequest) =>
    apiClient.post<ApiResponse<UserDto>>('/api/users', data).then((r) => r.data),

  update: (id: string, data: UpdateUserRequest) =>
    apiClient.put<ApiResponse<UserDto>>(`/api/users/${id}`, data).then((r) => r.data),

  changePassword: (id: string, data: ChangePasswordRequest) =>
    apiClient.post<ApiResponse<null>>(`/api/users/${id}/change-password`, data).then((r) => r.data),

  activate: (id: string) =>
    apiClient.post<ApiResponse<null>>(`/api/users/${id}/activate`).then((r) => r.data),

  deactivate: (id: string) =>
    apiClient.post<ApiResponse<null>>(`/api/users/${id}/deactivate`).then((r) => r.data),

  delete: (id: string) =>
    apiClient.delete<ApiResponse<null>>(`/api/users/${id}`).then((r) => r.data),
}
