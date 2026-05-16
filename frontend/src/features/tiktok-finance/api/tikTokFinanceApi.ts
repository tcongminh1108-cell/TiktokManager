import apiClient from '@/shared/lib/api-client'
import type { ApiResponse, PaginatedResult } from '@/shared/types/api'
import type { TikTokStatementDto, TikTokStatementQueryParams } from '../types'

export const tikTokFinanceApi = {
  listStatements: (params: TikTokStatementQueryParams) =>
    apiClient
      .get<ApiResponse<PaginatedResult<TikTokStatementDto>>>('/api/tiktok-finance/statements', {
        params,
      })
      .then((r) => r.data),

  getStatementTransactions: (statementId: string) =>
    apiClient
      .get<ApiResponse<string>>(`/api/tiktok-finance/statements/${statementId}/transactions/raw`)
      .then((r) => r.data),
}
