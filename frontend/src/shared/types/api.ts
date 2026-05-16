export interface ApiResponse<T> {
  success: boolean
  data: T | null
  message: string | null
  errors: Record<string, string[]> | null
}

export interface PaginatedResult<T> {
  items: T[]
  pageNumber: number
  pageSize: number
  totalCount: number
  totalPages: number
  hasPrevious: boolean
  hasNext: boolean
}

export interface PageRequest {
  pageNumber?: number
  pageSize?: number
  sortBy?: string
  sortDirection?: 'asc' | 'desc'
  search?: string
}
