import { create } from 'zustand'
import { persist } from 'zustand/middleware'
import type { AuthUser, UserRole } from '../types'

interface AuthState {
  accessToken: string | null
  refreshToken: string | null
  user: AuthUser | null
  isAuthenticated: boolean
  setTokens: (accessToken: string, refreshToken: string) => void
  setUser: (user: AuthUser) => void
  login: (accessToken: string, refreshToken: string, user: AuthUser) => void
  logout: () => void
  hasRole: (role: UserRole) => boolean
}

const ROLE_HIERARCHY: Record<UserRole, number> = {
  Admin: 3,
  Manager: 2,
  Staff: 1,
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set, get) => ({
      accessToken: null,
      refreshToken: null,
      user: null,
      isAuthenticated: false,

      setTokens: (accessToken, refreshToken) =>
        set({ accessToken, refreshToken, isAuthenticated: true }),

      setUser: (user) => set({ user }),

      login: (accessToken, refreshToken, user) =>
        set({ accessToken, refreshToken, user, isAuthenticated: true }),

      logout: () =>
        set({ accessToken: null, refreshToken: null, user: null, isAuthenticated: false }),

      // Returns true if current user's role is at or above the required role level.
      hasRole: (requiredRole) => {
        const user = get().user
        if (!user) return false
        return ROLE_HIERARCHY[user.role] >= ROLE_HIERARCHY[requiredRole]
      },
    }),
    {
      name: 'auth-storage',
      partialize: (state) => ({
        accessToken: state.accessToken,
        refreshToken: state.refreshToken,
        user: state.user,
        isAuthenticated: state.isAuthenticated,
      }),
    },
  ),
)
