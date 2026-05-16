export interface ProductMappingDto {
  id: string
  productId: string
  productName: string
  productCode: string
  connectionId: string
  shopName: string
  tikTokProductId: string
  tikTokSkuId: string
  tikTokSkuName: string
  warehouseId: string | null
  createdAt: string
}

export interface CreateProductMappingRequest {
  productId: string
  connectionId: string
  tikTokProductId: string
  tikTokSkuId: string
  tikTokSkuName: string
  warehouseId?: string
}

export interface ProductMappingQueryParams {
  pageNumber?: number
  pageSize?: number
  connectionId?: string
  productId?: string
  search?: string
}

export interface TikTokSkuInfo {
  productId: string
  productName: string
  skuId: string
  skuName: string
}

export interface TikTokProductListResponse {
  products: TikTokSkuInfo[]
  nextPageToken: string | null
}
