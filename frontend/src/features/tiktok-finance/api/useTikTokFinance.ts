import { useQuery } from '@tanstack/react-query'
import { tikTokFinanceApi } from './tikTokFinanceApi'
import type { TikTokStatementQueryParams } from '../types'

const QUERY_KEY = 'tiktok-finance'

export function useTikTokStatements(params: TikTokStatementQueryParams) {
  return useQuery({
    queryKey: [QUERY_KEY, 'statements', params],
    queryFn: () => tikTokFinanceApi.listStatements(params),
  })
}
