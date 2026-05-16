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
import TikTokShopsPage from '@/features/tiktok-shops/pages/TikTokShopsPage'
import ProductMappingsPage from '@/features/product-mappings/pages/ProductMappingsPage'
import TikTokOrdersPage from '@/features/tiktok-orders/pages/TikTokOrdersPage'
import TikTokReturnsPage from '@/features/tiktok-returns/pages/TikTokReturnsPage'
import TikTokFinancePage from '@/features/tiktok-finance/pages/TikTokFinancePage'
import { ROUTES } from '@/shared/constants/routes'

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
          { path: ROUTES.TIKTOK_ORDERS, element: <TikTokOrdersPage /> },
          { path: ROUTES.TIKTOK_RETURNS, element: <TikTokReturnsPage /> },
          { path: ROUTES.TIKTOK_FINANCE, element: <TikTokFinancePage /> },
          { path: ROUTES.PROFILE, element: <ProfilePage /> },
          {
            element: <ProtectedRoute requiredRole="Manager" />,
            children: [
              { path: ROUTES.SUPPLIERS, element: <SupplierListPage /> },
              { path: ROUTES.STOCK_INS, element: <StockInListPage /> },
              { path: ROUTES.PRODUCT_MAPPINGS, element: <ProductMappingsPage /> },
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
