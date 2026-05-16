import { App } from 'antd'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { extractApiError } from '@/shared/lib/errors'
import { productMappingsApi } from './productMappingsApi'
import type { CreateProductMappingRequest, ProductMappingQueryParams } from '../types'

const QUERY_KEY = 'product-mappings'

export function useProductMappings(params: ProductMappingQueryParams) {
  return useQuery({
    queryKey: [QUERY_KEY, params],
    queryFn: () => productMappingsApi.list(params),
  })
}

export function useTikTokSkus(connectionId: string, search?: string) {
  return useQuery({
    queryKey: ['tiktok-skus', connectionId, search],
    queryFn: () => productMappingsApi.getTikTokSkus(connectionId, search),
    enabled: !!connectionId,
  })
}

export function useProductMappingMutations() {
  const qc = useQueryClient()
  const { message } = App.useApp()
  const invalidate = () => qc.invalidateQueries({ queryKey: [QUERY_KEY] })

  const create = useMutation({
    mutationFn: (data: CreateProductMappingRequest) => productMappingsApi.create(data),
    onSuccess: () => { invalidate(); message.success('Đã tạo mapping') },
    onError: (e) => message.error(extractApiError(e)),
  })

  const remove = useMutation({
    mutationFn: (id: string) => productMappingsApi.delete(id),
    onSuccess: () => { invalidate(); message.success('Đã xóa mapping') },
    onError: (e) => message.error(extractApiError(e)),
  })

  return { create, remove }
}
