import { Navigate, Outlet } from 'react-router-dom'
import { useAuthStore } from '@/features/auth/store/useAuthStore'
import { ROUTES } from '@/shared/constants/routes'
import type { UserRole } from '@/features/auth/types'

interface Props {
  requiredRole?: UserRole
}

export default function ProtectedRoute({ requiredRole }: Props) {
  const { isAuthenticated, hasRole } = useAuthStore()

  if (!isAuthenticated) {
    return <Navigate to={ROUTES.LOGIN} replace />
  }

  if (requiredRole && !hasRole(requiredRole)) {
    return <Navigate to={ROUTES.DASHBOARD} replace />
  }

  return <Outlet />
}
