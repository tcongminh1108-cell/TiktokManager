import { useEffect, useState } from 'react'
import { Alert, DatePicker, Form, Input, InputNumber, Modal, Spin, Typography } from 'antd'
import { useForm, Controller } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import dayjs from 'dayjs'
import { useQuery } from '@tanstack/react-query'
import ProductSelect from '@/shared/components/ProductSelect'
import { formatVND } from '@/shared/lib/format'
import apiClient from '@/shared/lib/api-client'
import type { ApiResponse } from '@/shared/types/api'
import { useStockOutMutations } from '../api/useStockOuts'
import type { StockOutDto } from '../types'

interface InventoryItem {
  productId: string
  physicalStock: number
  reservedQuantity: number
  availableStock: number
}

const schema = z.object({
  productId: z.string().min(1, 'Chọn sản phẩm'),
  quantity: z.number({ error: 'Nhập số' }).int().positive('Phải > 0'),
  unitPrice: z.number({ error: 'Nhập số' }).min(0, 'Phải ≥ 0'),
  transactionDate: z.string().min(1, 'Chọn ngày'),
  note: z.string().optional(),
})

type FormValues = z.infer<typeof schema>

interface Props {
  open: boolean
  editing?: StockOutDto | null
  onClose: () => void
}

export default function StockOutFormModal({ open, editing, onClose }: Props) {
  const isEdit = !!editing
  const { create, update } = useStockOutMutations()
  const [preview, setPreview] = useState(0)

  const { control, handleSubmit, reset, watch, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { productId: '', quantity: 1, unitPrice: 0, transactionDate: dayjs().toISOString(), note: '' },
  })

  const productId = watch('productId')
  const qty = watch('quantity')
  const price = watch('unitPrice')

  useEffect(() => { setPreview((qty || 0) * (price || 0)) }, [qty, price])

  const { data: inventoryData, isLoading: inventoryLoading } = useQuery({
    queryKey: ['inventory', productId],
    queryFn: () => apiClient.get<ApiResponse<InventoryItem>>(`/api/inventory/${productId}`).then((r) => r.data),
    enabled: !!productId && !isEdit,
  })

  const inventory = inventoryData?.data
  const availableStock = inventory?.availableStock ?? 0
  const isOverStock = !isEdit && !!productId && qty > availableStock

  useEffect(() => {
    if (editing) {
      reset({ productId: editing.productId, quantity: editing.quantity, unitPrice: editing.unitPrice, transactionDate: editing.transactionDate, note: editing.note ?? '' })
    } else {
      reset({ productId: '', quantity: 1, unitPrice: 0, transactionDate: dayjs().toISOString(), note: '' })
    }
  }, [editing, reset])

  const onSubmit = async (values: FormValues) => {
    if (isEdit && editing) {
      await update.mutateAsync({ id: editing.id, data: { transactionDate: values.transactionDate, note: values.note } })
    } else {
      await create.mutateAsync(values)
    }
    onClose()
  }

  const fi = (name: keyof FormValues, label: string, node: React.ReactNode) => (
    <Form.Item label={label} validateStatus={errors[name] ? 'error' : ''} help={errors[name]?.message}>
      {node}
    </Form.Item>
  )

  return (
    <Modal open={open} title={isEdit ? 'Chỉnh sửa phiếu xuất' : 'Xuất hàng'} onCancel={onClose} onOk={handleSubmit(onSubmit)} okText={isEdit ? 'Lưu' : 'Xác nhận xuất'} confirmLoading={isSubmitting} destroyOnClose width={560}>
      <Form layout="vertical" style={{ marginTop: 16 }}>
        {fi('productId', 'Sản phẩm', <Controller name="productId" control={control} render={({ field: f }) => <ProductSelect value={f.value || undefined} onChange={(v) => f.onChange(v)} disabled={isEdit} />} />)}

        {!isEdit && productId && (
          <Form.Item>
            {inventoryLoading ? <Spin size="small" /> : (
              <Typography.Text type={availableStock <= 0 ? 'danger' : availableStock < 10 ? 'warning' : 'success'}>
                Tồn có thể xuất: <strong>{availableStock}</strong>
                {inventory && ` (vật lý: ${inventory.physicalStock}, đang giữ: ${inventory.reservedQuantity})`}
              </Typography.Text>
            )}
          </Form.Item>
        )}

        {isOverStock && (
          <Alert type="warning" showIcon message={`Số lượng xuất (${qty}) vượt quá tồn kho khả dụng (${availableStock}). Giao dịch có thể bị từ chối.`} style={{ marginBottom: 16 }} />
        )}

        {fi('quantity', 'Số lượng', <Controller name="quantity" control={control} render={({ field: f }) => <InputNumber {...f} style={{ width: '100%' }} min={1} disabled={isEdit} status={isOverStock ? 'warning' : ''} />} />)}
        {fi('unitPrice', 'Đơn giá bán (VND)', <Controller name="unitPrice" control={control} render={({ field: f }) => <InputNumber {...f} style={{ width: '100%' }} min={0} step={1000} formatter={(v) => `${v}`.replace(/\B(?=(\d{3})+(?!\d))/g, ',')} parser={(v) => Number(v?.replace(/,/g, '')) as never} disabled={isEdit} />} />)}
        {fi('transactionDate', 'Ngày xuất', <Controller name="transactionDate" control={control} render={({ field: f }) => <DatePicker value={f.value ? dayjs(f.value) : null} onChange={(d) => f.onChange(d?.toISOString() ?? '')} style={{ width: '100%' }} format="DD/MM/YYYY" />} />)}
        {fi('note', 'Ghi chú', <Controller name="note" control={control} render={({ field: f }) => <Input.TextArea {...f} rows={2} />} />)}
        {!isEdit && <Form.Item label="Tổng tiền (dự tính)"><strong style={{ color: '#1677ff' }}>{formatVND(preview)}</strong></Form.Item>}
      </Form>
    </Modal>
  )
}
