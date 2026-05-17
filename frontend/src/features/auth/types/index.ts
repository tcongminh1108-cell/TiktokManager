export type UserRole = 'Admin' | 'Manager' | 'Staff'

export interface AuthUser {
  userId: string
  tenantId: string
  email: string
  fullName: string
  role: UserRole
}

export interface LoginRequest {
  email: string
  password: string
}

export interface RegisterTenantRequest {
  tenantName: string
  contactEmail: string
  contactPhone?: string
  adminEmail: string
  adminPassword: string
  adminFullName: string
}

export interface AuthResponse {
  accessToken: string
  refreshToken: string
  accessTokenExpiresAt: string
  user: AuthUser
}

export interface RefreshRequest {
  refreshToken: string
}
