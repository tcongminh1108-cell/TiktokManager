import { App } from 'antd'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { extractApiError } from '@/shared/lib/errors'
import { productApi } from './productApi'
import type { CreateProductRequest, ProductQueryParams, UpdateProductRequest } from '../types'

const QUERY_KEY = 'products'

export function useProducts(params: ProductQueryParams) {
  return useQuery({
    queryKey: [QUERY_KEY, params],
    queryFn: () => productApi.list(params),
  })
}

export function useProductMutations() {
  const qc = useQueryClient()
  const { message } = App.useApp()
  const invalidate = () => qc.invalidateQueries({ queryKey: [QUERY_KEY] })

  const create = useMutation({
    mutationFn: (data: CreateProductRequest) => productApi.create(data),
    onSuccess: () => { invalidate(); message.success('Tạo sản phẩm thành công') },
    onError: (e) => message.error(extractApiError(e)),
  })

  const update = useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateProductRequest }) =>
      productApi.update(id, data),
    onSuccess: () => { invalidate(); message.success('Cập nhật thành công') },
    onError: (e) => message.error(extractApiError(e)),
  })

  const remove = useMutation({
    mutationFn: (id: string) => productApi.delete(id),
    onSuccess: () => { invalidate(); message.success('Đã xóa sản phẩm') },
    onError: (e) => message.error(extractApiError(e)),
  })

  const activate = useMutation({
    mutationFn: (id: string) => productApi.activate(id),
    onSuccess: () => { invalidate(); message.success('Đã kích hoạt') },
    onError: (e) => message.error(extractApiError(e)),
  })

  const deactivate = useMutation({
    mutationFn: (id: string) => productApi.deactivate(id),
    onSuccess: () => { invalidate(); message.success('Đã tạm dừng') },
    onError: (e) => message.error(extractApiError(e)),
  })

  return { create, update, remove, activate, deactivate }
}
