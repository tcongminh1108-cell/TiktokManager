import { App } from 'antd'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { extractApiError } from '@/shared/lib/errors'
import { stockOutApi } from './stockOutApi'
import type { CreateStockOutRequest, StockOutQueryParams, UpdateStockOutRequest } from '../types'

const QUERY_KEY = 'stock-outs'

export function useStockOuts(params: StockOutQueryParams) {
  return useQuery({
    queryKey: [QUERY_KEY, params],
    queryFn: () => stockOutApi.list(params),
  })
}

export function useStockOutMutations() {
  const qc = useQueryClient()
  const { message } = App.useApp()
  const invalidate = () => qc.invalidateQueries({ queryKey: [QUERY_KEY] })

  const create = useMutation({
    mutationFn: (data: CreateStockOutRequest) => stockOutApi.create(data),
    onSuccess: () => {
      invalidate()
      qc.invalidateQueries({ queryKey: ['inventory'] })
      message.success('Xuất hàng thành công')
    },
    onError: (e) => message.error(extractApiError(e)),
  })

  const update = useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateStockOutRequest }) => stockOutApi.update(id, data),
    onSuccess: () => { invalidate(); message.success('Cập nhật thành công') },
    onError: (e) => message.error(extractApiError(e)),
  })

  const remove = useMutation({
    mutationFn: (id: string) => stockOutApi.delete(id),
    onSuccess: () => {
      invalidate()
      qc.invalidateQueries({ queryKey: ['inventory'] })
      message.success('Đã xóa phiếu xuất')
    },
    onError: (e) => message.error(extractApiError(e)),
  })

  return { create, update, remove }
}
