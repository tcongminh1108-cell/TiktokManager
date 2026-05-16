import { useQuery } from '@tanstack/react-query'
import { tikTokReturnsApi } from './tikTokReturnsApi'
import type { TikTokReturnQueryParams } from '../types'

const QUERY_KEY = 'tiktok-returns'

export function useTikTokReturns(params: TikTokReturnQueryParams) {
  return useQuery({
    queryKey: [QUERY_KEY, params],
    queryFn: () => tikTokReturnsApi.list(params),
  })
}
