import { useState } from 'react'

export interface DataTableParams {
  pageNumber: number
  pageSize: number
  sortBy?: string
  sortDirection: 'asc' | 'desc'
  search?: string
}

export function useDataTable<T extends object = Record<never, never>>(
  initial?: Partial<DataTableParams> & Partial<T>,
) {
  type Params = DataTableParams & T
  const [params, setParams] = useState<Params>({
    pageNumber: 1,
    pageSize: 20,
    sortDirection: 'asc',
    ...initial,
  } as Params)

  const setPage = (pageNumber: number, pageSize: number) =>
    setParams((prev) => ({ ...prev, pageNumber, pageSize }))

  const setSort = (sortBy: string | undefined, sortDirection: 'asc' | 'desc') =>
    setParams((prev) => ({ ...prev, sortBy, sortDirection, pageNumber: 1 }))

  const setSearch = (search: string | undefined) =>
    setParams((prev) => ({ ...prev, search: search || undefined, pageNumber: 1 }))

  const setFilters = (filters: Partial<T>) =>
    setParams((prev) => ({ ...prev, ...filters, pageNumber: 1 }))

  return { params, setPage, setSort, setSearch, setFilters }
}
