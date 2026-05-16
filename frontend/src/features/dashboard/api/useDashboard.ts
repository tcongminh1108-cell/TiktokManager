import { useQuery } from '@tanstack/react-query'
import { dashboardApi } from './dashboardApi'
import type { DashboardParams } from '../types'

const KEY = 'dashboard'

export function useOverview(params: DashboardParams) {
  return useQuery({ queryKey: [KEY, 'overview', params], queryFn: () => dashboardApi.getOverview(params) })
}

export function useRevenueByDay(params: DashboardParams) {
  return useQuery({ queryKey: [KEY, 'revenue-by-day', params], queryFn: () => dashboardApi.getRevenueByDay(params) })
}

export function useTopProducts(params: DashboardParams, limit = 10) {
  return useQuery({ queryKey: [KEY, 'top-products', params, limit], queryFn: () => dashboardApi.getTopProducts({ ...params, limit }) })
}

export function useRevenueBySource(params: DashboardParams) {
  return useQuery({ queryKey: [KEY, 'revenue-by-source', params], queryFn: () => dashboardApi.getRevenueBySource(params) })
}

export function useProductProfit(params: DashboardParams, pageNumber: number, pageSize: number) {
  return useQuery({
    queryKey: [KEY, 'product-profit', params, pageNumber, pageSize],
    queryFn: () => dashboardApi.getProductProfit({ ...params, pageNumber, pageSize }),
  })
}
