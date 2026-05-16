import { App } from 'antd'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { extractApiError } from '@/shared/lib/errors'
import { tikTokShopsApi } from './tikTokShopsApi'

const QUERY_KEY = 'tiktok-shops'

export function useTikTokShops() {
  return useQuery({
    queryKey: [QUERY_KEY],
    queryFn: () => tikTokShopsApi.list(),
  })
}

export function useTikTokShopMutations() {
  const qc = useQueryClient()
  const { message } = App.useApp()
  const invalidate = () => qc.invalidateQueries({ queryKey: [QUERY_KEY] })

  const remove = useMutation({
    mutationFn: (id: string) => tikTokShopsApi.delete(id),
    onSuccess: () => { invalidate(); message.success('Đã xóa kết nối') },
    onError: (e) => message.error(extractApiError(e)),
  })

  const refreshToken = useMutation({
    mutationFn: (id: string) => tikTokShopsApi.refreshToken(id),
    onSuccess: () => { invalidate(); message.success('Đã làm mới token') },
    onError: (e) => message.error(extractApiError(e)),
  })

  return { remove, refreshToken }
}
