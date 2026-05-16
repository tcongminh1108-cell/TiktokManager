import { App } from 'antd'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { extractApiError } from '@/shared/lib/errors'
import { tikTokOrdersApi } from './tikTokOrdersApi'
import type { TikTokOrderQueryParams } from '../types'

const QUERY_KEY = 'tiktok-orders'

export function useTikTokOrders(params: TikTokOrderQueryParams) {
  return useQuery({
    queryKey: [QUERY_KEY, params],
    queryFn: () => tikTokOrdersApi.list(params),
  })
}

export function useTikTokOrderMutations() {
  const qc = useQueryClient()
  const { message } = App.useApp()

  const syncNow = useMutation({
    mutationFn: (connectionId: string) => tikTokOrdersApi.syncNow(connectionId),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: [QUERY_KEY] })
      message.success('Đã kích hoạt đồng bộ đơn hàng')
    },
    onError: (e) => message.error(extractApiError(e)),
  })

  return { syncNow }
}
