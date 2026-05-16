import type { UserRole } from '@/features/auth/types'

export interface UserDto {
  id: string
  email: string
  fullName: string
  role: UserRole
  isActive: boolean
  createdAt: string
  lastLoginAt?: string
}

export interface CreateUserRequest {
  email: string
  fullName: string
  password: string
  role: UserRole
}

export interface UpdateUserRequest {
  fullName: string
  role: UserRole
}

export interface ChangePasswordRequest {
  newPassword: string
  confirmPassword: string
}

export interface UserQueryParams {
  pageNumber?: number
  pageSize?: number
  sortBy?: string
  sortDirection?: string
  search?: string
  role?: UserRole
  isActive?: boolean
}
