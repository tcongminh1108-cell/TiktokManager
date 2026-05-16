import { createBrowserRouter } from 'react-router-dom'
import AuthLayout from '@/layouts/AuthLayout'
import MainLayout from '@/layouts/MainLayout'
import ProtectedRoute from './ProtectedRoute'
import LoginPage from '@/features/auth/pages/LoginPage'
import RegisterTenantPage from '@/features/auth/pages/RegisterTenantPage'
import DashboardPage from '@/features/dashboard/pages/DashboardPage'
import ProductListPage from '@/features/products/pages/ProductListPage'
import SupplierListPage from '@/features/suppliers/pages/SupplierListPage'
import StockInListPage from '@/features/stock-ins/pages/StockInListPage'
import StockOutListPage from '@/features/stock-outs/pages/StockOutListPage'
import InventoryPage from '@/features/inventory/pages/InventoryPage'
import UserListPage from '@/features/users/pages/UserListPage'
import { ROUTES } from '@/shared/constants/routes'
const TikTokShopsPage = () => <div style={{ padding: 24 }}>TikTok Shops (coming soon)</div>
const ProfilePage = () => <div style={{ padding: 24 }}>Profile (coming soon)</div>

export const router = createBrowserRouter([
  {
    element: <AuthLayout />,
    children: [
      { path: ROUTES.LOGIN, element: <LoginPage /> },
      { path: ROUTES.REGISTER_TENANT, element: <RegisterTenantPage /> },
    ],
  },
  {
    element: <ProtectedRoute />,
    children: [
      {
        element: <MainLayout />,
        children: [
          { path: ROUTES.DASHBOARD, element: <DashboardPage /> },
          { path: ROUTES.PRODUCTS, element: <ProductListPage /> },
          { path: ROUTES.STOCK_OUTS, element: <StockOutListPage /> },
          { path: ROUTES.INVENTORY, element: <InventoryPage /> },
          { path: ROUTES.PROFILE, element: <ProfilePage /> },
          {
            element: <ProtectedRoute requiredRole="Manager" />,
            children: [
              { path: ROUTES.SUPPLIERS, element: <SupplierListPage /> },
              { path: ROUTES.STOCK_INS, element: <StockInListPage /> },
            ],
          },
          {
            element: <ProtectedRoute requiredRole="Admin" />,
            children: [
              { path: ROUTES.USERS, element: <UserListPage /> },
              { path: ROUTES.TIKTOK_SHOPS, element: <TikTokShopsPage /> },
            ],
          },
        ],
      },
    ],
  },
  { path: '/', element: <LoginPage /> },
])
