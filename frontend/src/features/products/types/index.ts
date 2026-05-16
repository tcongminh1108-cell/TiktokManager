export interface ProductDto {
  id: string
  code: string
  name: string
  description?: string
  sellingPrice: number
  unit: string
  imageUrl?: string
  isActive: boolean
  createdAt: string
  updatedAt?: string
}

export interface CreateProductRequest {
  code: string
  name: string
  description?: string
  sellingPrice: number
  unit: string
  imageUrl?: string
}

export interface UpdateProductRequest {
  name: string
  description?: string
  sellingPrice: number
  unit: string
  imageUrl?: string
}

export interface ProductQueryParams {
  pageNumber?: number
  pageSize?: number
  sortBy?: string
  sortDirection?: string
  search?: string
  isActive?: boolean
  minPrice?: number
  maxPrice?: number
}
