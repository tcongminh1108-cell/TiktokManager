export type TikTokShopStatus = 'Active' | 'Expired' | 'Revoked' | 'Error'

export interface TikTokConnectionDto {
  id: string
  shopId: string
  shopName: string
  region: string
  status: TikTokShopStatus
  tokenExpiresAt: string
  lastSyncedAt: string | null
  lastWebhookAt: string | null
  createdAt: string
}

export interface TikTokAuthUrlResponse {
  authUrl: string
  state: string
}
