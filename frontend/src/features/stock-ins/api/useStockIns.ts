import { App } from 'antd'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { extractApiError } from '@/shared/lib/errors'
import { stockInApi } from './stockInApi'
import type { CreateStockInRequest, StockInQueryParams, UpdateStockInRequest } from '../types'

const QUERY_KEY = 'stock-ins'

export function useStockIns(params: StockInQueryParams) {
  return useQuery({
    queryKey: [QUERY_KEY, params],
    queryFn: () => stockInApi.list(params),
  })
}

export function useStockInMutations() {
  const qc = useQueryClient()
  const { message } = App.useApp()
  const invalidate = () => qc.invalidateQueries({ queryKey: [QUERY_KEY] })

  const create = useMutation({
    mutationFn: (data: CreateStockInRequest) => stockInApi.create(data),
    onSuccess: () => {
      invalidate()
      qc.invalidateQueries({ queryKey: ['inventory'] })
      message.success('Nhập hàng thành công')
    },
    onError: (e) => message.error(extractApiError(e)),
  })

  const update = useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateStockInRequest }) => stockInApi.update(id, data),
    onSuccess: () => { invalidate(); message.success('Cập nhật thành công') },
    onError: (e) => message.error(extractApiError(e)),
  })

  const remove = useMutation({
    mutationFn: (id: string) => stockInApi.delete(id),
    onSuccess: () => {
      invalidate()
      qc.invalidateQueries({ queryKey: ['inventory'] })
      message.success('Đã xóa phiếu nhập')
    },
    onError: (e) => message.error(extractApiError(e)),
  })

  return { create, update, remove }
}
