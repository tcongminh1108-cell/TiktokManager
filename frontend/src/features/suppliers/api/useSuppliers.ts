import { App } from 'antd'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { extractApiError } from '@/shared/lib/errors'
import { supplierApi } from './supplierApi'
import type { CreateSupplierRequest, SupplierQueryParams, UpdateSupplierRequest } from '../types'

const QUERY_KEY = 'suppliers'

export function useSuppliers(params: SupplierQueryParams) {
  return useQuery({
    queryKey: [QUERY_KEY, params],
    queryFn: () => supplierApi.list(params),
  })
}

export function useSupplierMutations() {
  const qc = useQueryClient()
  const { message } = App.useApp()
  const invalidate = () => qc.invalidateQueries({ queryKey: [QUERY_KEY] })

  const create = useMutation({
    mutationFn: (data: CreateSupplierRequest) => supplierApi.create(data),
    onSuccess: () => { invalidate(); message.success('Tạo nhà cung cấp thành công') },
    onError: (e) => message.error(extractApiError(e)),
  })

  const update = useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateSupplierRequest }) => supplierApi.update(id, data),
    onSuccess: () => { invalidate(); message.success('Cập nhật thành công') },
    onError: (e) => message.error(extractApiError(e)),
  })

  const remove = useMutation({
    mutationFn: (id: string) => supplierApi.delete(id),
    onSuccess: () => { invalidate(); message.success('Đã xóa nhà cung cấp') },
    onError: (e) => message.error(extractApiError(e)),
  })

  return { create, update, remove }
}
