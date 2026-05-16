import { useState } from 'react'
import { Select } from 'antd'
import { useQuery } from '@tanstack/react-query'
import { useDebounce } from '@/shared/hooks/useDebounce'
import apiClient from '@/shared/lib/api-client'
import type { ApiResponse, PaginatedResult } from '@/shared/types/api'

interface ProductOption {
  id: string
  code: string
  name: string
  sellingPrice: number
}

interface ProductSelectProps {
  value?: string
  onChange?: (value: string, option: ProductOption) => void
  placeholder?: string
  disabled?: boolean
}

export default function ProductSelect({ value, onChange, placeholder = 'Chọn sản phẩm', disabled }: ProductSelectProps) {
  const [search, setSearch] = useState('')
  const debouncedSearch = useDebounce(search, 300)

  const { data, isLoading } = useQuery({
    queryKey: ['products', 'select', debouncedSearch],
    queryFn: () =>
      apiClient
        .get<ApiResponse<PaginatedResult<ProductOption>>>('/api/products', {
          params: { search: debouncedSearch, pageSize: 30, isActive: true },
        })
        .then((r) => r.data.data?.items ?? []),
  })

  const options = (data ?? []).map((p) => ({
    value: p.id,
    label: `[${p.code}] ${p.name}`,
    ...p,
  }))

  return (
    <Select
      showSearch
      filterOption={false}
      onSearch={setSearch}
      loading={isLoading}
      options={options}
      value={value}
      onChange={(val, opt) => onChange?.(val, opt as ProductOption)}
      placeholder={placeholder}
      disabled={disabled}
      style={{ width: '100%' }}
    />
  )
}
