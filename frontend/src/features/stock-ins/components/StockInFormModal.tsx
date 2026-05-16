import { useEffect, useState } from 'react'
import { DatePicker, Form, Input, InputNumber, Modal } from 'antd'
import { useForm, Controller } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import dayjs from 'dayjs'
import ProductSelect from '@/shared/components/ProductSelect'
import SupplierSelect from '@/shared/components/SupplierSelect'
import { formatVND } from '@/shared/lib/format'
import { useStockInMutations } from '../api/useStockIns'
import type { StockInDto } from '../types'

const schema = z.object({
  productId: z.string().min(1, 'Chọn sản phẩm'),
  supplierId: z.string().min(1, 'Chọn nhà cung cấp'),
  quantity: z.number({ error: 'Nhập số' }).int().positive('Phải > 0'),
  unitPrice: z.number({ error: 'Nhập số' }).positive('Phải > 0'),
  transactionDate: z.string().min(1, 'Chọn ngày'),
  note: z.string().optional(),
})

type FormValues = z.infer<typeof schema>

interface Props {
  open: boolean
  editing?: StockInDto | null
  onClose: () => void
}

export default function StockInFormModal({ open, editing, onClose }: Props) {
  const isEdit = !!editing
  const { create, update } = useStockInMutations()
  const [preview, setPreview] = useState(0)

  const { control, handleSubmit, reset, watch, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { productId: '', supplierId: '', quantity: 1, unitPrice: 0, transactionDate: dayjs().toISOString(), note: '' },
  })

  const qty = watch('quantity')
  const price = watch('unitPrice')
  useEffect(() => { setPreview((qty || 0) * (price || 0)) }, [qty, price])

  useEffect(() => {
    if (editing) {
      reset({ productId: editing.productId, supplierId: editing.supplierId, quantity: editing.quantity, unitPrice: editing.unitPrice, transactionDate: editing.transactionDate, note: editing.note ?? '' })
    } else {
      reset({ productId: '', supplierId: '', quantity: 1, unitPrice: 0, transactionDate: dayjs().toISOString(), note: '' })
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
    <Modal open={open} title={isEdit ? 'Chỉnh sửa phiếu nhập' : 'Nhập hàng'} onCancel={onClose} onOk={handleSubmit(onSubmit)} okText={isEdit ? 'Lưu' : 'Xác nhận nhập'} confirmLoading={isSubmitting} destroyOnClose width={560}>
      <Form layout="vertical" style={{ marginTop: 16 }}>
        {fi('productId', 'Sản phẩm', <Controller name="productId" control={control} render={({ field: f }) => <ProductSelect value={f.value || undefined} onChange={(v) => f.onChange(v)} disabled={isEdit} />} />)}
        {fi('supplierId', 'Nhà cung cấp', <Controller name="supplierId" control={control} render={({ field: f }) => <SupplierSelect value={f.value || undefined} onChange={f.onChange} disabled={isEdit} />} />)}
        {fi('quantity', 'Số lượng', <Controller name="quantity" control={control} render={({ field: f }) => <InputNumber {...f} style={{ width: '100%' }} min={1} disabled={isEdit} />} />)}
        {fi('unitPrice', 'Đơn giá nhập (VND)', <Controller name="unitPrice" control={control} render={({ field: f }) => <InputNumber {...f} style={{ width: '100%' }} min={0} step={1000} formatter={(v) => `${v}`.replace(/\B(?=(\d{3})+(?!\d))/g, ',')} parser={(v) => Number(v?.replace(/,/g, '')) as never} disabled={isEdit} />} />)}
        {fi('transactionDate', 'Ngày nhập', <Controller name="transactionDate" control={control} render={({ field: f }) => <DatePicker value={f.value ? dayjs(f.value) : null} onChange={(d) => f.onChange(d?.toISOString() ?? '')} style={{ width: '100%' }} format="DD/MM/YYYY" />} />)}
        {fi('note', 'Ghi chú', <Controller name="note" control={control} render={({ field: f }) => <Input.TextArea {...f} rows={2} />} />)}
        {!isEdit && <Form.Item label="Tổng tiền (dự tính)"><strong style={{ color: '#1677ff' }}>{formatVND(preview)}</strong></Form.Item>}
      </Form>
    </Modal>
  )
}
