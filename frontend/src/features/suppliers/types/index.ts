export interface SupplierDto {
  id: string
  code: string
  name: string
  phone?: string
  email?: string
  address?: string
  note?: string
  createdAt: string
  updatedAt?: string
}

export interface CreateSupplierRequest {
  code: string
  name: string
  phone?: string
  email?: string
  address?: string
  note?: string
}

export interface UpdateSupplierRequest {
  name: string
  phone?: string
  email?: string
  address?: string
  note?: string
}

export interface SupplierQueryParams {
  pageNumber?: number
  pageSize?: number
  sortBy?: string
  sortDirection?: string
  search?: string
}
