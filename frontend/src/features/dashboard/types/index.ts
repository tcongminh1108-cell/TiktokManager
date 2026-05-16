export interface OverviewDto {
  grossRevenue: number
  totalPurchaseCost: number
  grossProfit: number
  totalProducts: number
  totalSuppliers: number
  stockInTransactions: number
  stockOutTransactions: number
  totalPhysicalStock: number
  totalReservedStock: number
}

export interface RevenuByDayDto {
  date: string
  revenue: number
}

export interface TopProductDto {
  productId: string
  productCode: string
  productName: string
  totalQuantity: number
  totalRevenue: number
}

export interface RevenueBySourceDto {
  source: string
  revenue: number
  percentage: number
}

export interface ProductProfitDto {
  productId: string
  productCode: string
  productName: string
  totalSoldQty: number
  grossRevenue: number
  avgCostPrice?: number
  grossProfit: number
  marginPercent: number
  manualSoldQty: number
  tikTokSoldQty: number
}

export interface DashboardParams {
  from?: string
  to?: string
  source?: 'All' | 'Manual' | 'TikTokOrder'
}
