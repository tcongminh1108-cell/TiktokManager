import apiClient from '@/shared/lib/api-client'
import type { ApiResponse, PaginatedResult } from '@/shared/types/api'
import type { DashboardParams, OverviewDto, ProductProfitDto, RevenueBySourceDto, RevenuByDayDto, TopProductDto } from '../types'

export const dashboardApi = {
  getOverview: (params: DashboardParams) =>
    apiClient.get<ApiResponse<OverviewDto>>('/api/dashboard/overview', { params }).then((r) => r.data),

  getRevenueByDay: (params: DashboardParams) =>
    apiClient.get<ApiResponse<RevenuByDayDto[]>>('/api/dashboard/revenue-by-day', { params }).then((r) => r.data),

  getTopProducts: (params: DashboardParams & { limit?: number }) =>
    apiClient.get<ApiResponse<TopProductDto[]>>('/api/dashboard/top-products', { params }).then((r) => r.data),

  getRevenueBySource: (params: DashboardParams) =>
    apiClient.get<ApiResponse<RevenueBySourceDto[]>>('/api/dashboard/revenue-by-source', { params }).then((r) => r.data),

  getProductProfit: (params: DashboardParams & { pageNumber?: number; pageSize?: number }) =>
    apiClient.get<ApiResponse<PaginatedResult<ProductProfitDto>>>('/api/dashboard/product-profit', { params }).then((r) => r.data),
}
