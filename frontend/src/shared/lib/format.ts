import dayjs from 'dayjs'

export const formatVND = (amount: number): string =>
  new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(amount)

export const formatNumber = (n: number): string =>
  new Intl.NumberFormat('vi-VN').format(n)

export const formatDate = (date: string | null | undefined): string =>
  date ? dayjs(date).format('DD/MM/YYYY') : '—'

export const formatDateTime = (date: string | null | undefined): string =>
  date ? dayjs(date).format('DD/MM/YYYY HH:mm') : '—'
