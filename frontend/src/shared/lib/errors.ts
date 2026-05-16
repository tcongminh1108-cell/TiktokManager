import axios from 'axios'
import type { ApiResponse } from '@/shared/types/api'

export function extractApiError(error: unknown): string {
  if (axios.isAxiosError(error)) {
    const data = error.response?.data as ApiResponse<unknown> | undefined
    if (data?.message) return data.message
    if (data?.errors) {
      const first = Object.values(data.errors)[0]?.[0]
      if (first) return first
    }
  }
  if (error instanceof Error) return error.message
  return 'Có lỗi xảy ra, vui lòng thử lại'
}
