import axios, { type AxiosError } from 'axios'
import type { ApiResponse } from '@/shared/types/api'
import type { AuthResponse } from '@/features/auth/types'
import { useAuthStore } from '@/features/auth/store/useAuthStore'

export const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5000',
  headers: { 'Content-Type': 'application/json' },
})

apiClient.interceptors.request.use((config) => {
  const token = useAuthStore.getState().accessToken
  if (token) config.headers.Authorization = `Bearer ${token}`
  return config
})

let isRefreshing = false
let failedQueue: Array<{
  resolve: (value: unknown) => void
  reject: (reason?: unknown) => void
}> = []

const processQueue = (error: unknown, token: string | null = null) => {
  failedQueue.forEach(({ resolve, reject }) => {
    if (error) reject(error)
    else resolve(token)
  })
  failedQueue = []
}

apiClient.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const original = error.config as typeof error.config & { _retry?: boolean }

    if (error.response?.status !== 401 || original?._retry) {
      return Promise.reject(error)
    }

    if (isRefreshing) {
      return new Promise((resolve, reject) => {
        failedQueue.push({ resolve, reject })
      })
        .then((token) => {
          if (original) original.headers.Authorization = `Bearer ${token}`
          return apiClient(original!)
        })
        .catch((err) => Promise.reject(err))
    }

    original!._retry = true
    isRefreshing = true

    const refreshToken = useAuthStore.getState().refreshToken

    if (!refreshToken) {
      useAuthStore.getState().logout()
      window.location.href = '/login'
      return Promise.reject(error)
    }

    try {
      const { data } = await axios.post<ApiResponse<AuthResponse>>(
        `${import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5000'}/api/auth/refresh`,
        { refreshToken },
      )

      if (!data.success || !data.data) throw new Error('Refresh failed')

      const { accessToken, refreshToken: newRefreshToken, user } = data.data
      useAuthStore.getState().login(accessToken, newRefreshToken, user)

      processQueue(null, accessToken)
      if (original) original.headers.Authorization = `Bearer ${accessToken}`
      return apiClient(original!)
    } catch (refreshError) {
      processQueue(refreshError, null)
      useAuthStore.getState().logout()
      window.location.href = '/login'
      return Promise.reject(refreshError)
    } finally {
      isRefreshing = false
    }
  },
)

export default apiClient
