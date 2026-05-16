import { useQuery } from '@tanstack/react-query'
import { inventoryApi } from './inventoryApi'
import type { InventoryDetailQueryParams, InventoryQueryParams } from '../types'

const QUERY_KEY = 'inventory'

export function useInventory(params: InventoryQueryParams) {
  return useQuery({
    queryKey: [QUERY_KEY, params],
    queryFn: () => inventoryApi.list(params),
  })
}

export function useInventoryDetail(productId: string | null, params: InventoryDetailQueryParams) {
  return useQuery({
    queryKey: [QUERY_KEY, productId, params],
    queryFn: () => inventoryApi.getDetail(productId!, params),
    enabled: !!productId,
  })
}
