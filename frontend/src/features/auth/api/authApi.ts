import apiClient from '@/shared/lib/api-client'
import type { ApiResponse } from '@/shared/types/api'
import type { AuthResponse, LoginRequest, RefreshRequest, RegisterTenantRequest } from '../types'

export const authApi = {
  login: (data: LoginRequest) =>
    apiClient.post<ApiResponse<AuthResponse>>('/api/auth/login', data).then((r) => r.data),

  register: (data: RegisterTenantRequest) =>
    apiClient
      .post<ApiResponse<AuthResponse>>('/api/auth/register-tenant', data)
      .then((r) => r.data),

  refresh: (data: RefreshRequest) =>
    apiClient.post<ApiResponse<AuthResponse>>('/api/auth/refresh', data).then((r) => r.data),

  logout: (refreshToken: string) =>
    apiClient
      .post<ApiResponse<null>>('/api/auth/logout', { refreshToken })
      .then((r) => r.data),
}
