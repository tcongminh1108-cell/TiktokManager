import { useState } from 'react'
import { Select } from 'antd'
import { useQuery } from '@tanstack/react-query'
import { useDebounce } from '@/shared/hooks/useDebounce'
import apiClient from '@/shared/lib/api-client'
import type { ApiResponse, PaginatedResult } from '@/shared/types/api'

interface SupplierOption {
  id: string
  code: string
  name: string
}

interface SupplierSelectProps {
  value?: string
  onChange?: (value: string) => void
  placeholder?: string
  disabled?: boolean
}

export default function SupplierSelect({ value, onChange, placeholder = 'Chọn nhà cung cấp', disabled }: SupplierSelectProps) {
  const [search, setSearch] = useState('')
  const debouncedSearch = useDebounce(search, 300)

  const { data, isLoading } = useQuery({
    queryKey: ['suppliers', 'select', debouncedSearch],
    queryFn: () =>
      apiClient
        .get<ApiResponse<PaginatedResult<SupplierOption>>>('/api/suppliers', {
          params: { search: debouncedSearch, pageSize: 30 },
        })
        .then((r) => r.data.data?.items ?? []),
  })

  const options = (data ?? []).map((s) => ({
    value: s.id,
    label: `[${s.code}] ${s.name}`,
  }))

  return (
    <Select
      showSearch
      filterOption={false}
      onSearch={setSearch}
      loading={isLoading}
      options={options}
      value={value}
      onChange={onChange}
      placeholder={placeholder}
      disabled={disabled}
      style={{ width: '100%' }}
    />
  )
}
