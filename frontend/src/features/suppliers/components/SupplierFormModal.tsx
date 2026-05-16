import { useEffect } from 'react'
import { Form, Input, Modal } from 'antd'
import { useForm, Controller } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { useSupplierMutations } from '../api/useSuppliers'
import type { SupplierDto } from '../types'

const schema = z.object({
  code: z.string().min(1, 'Bắt buộc').max(50),
  name: z.string().min(1, 'Bắt buộc').max(200),
  phone: z.string().optional(),
  email: z.string().email('Email không hợp lệ').optional().or(z.literal('')),
  address: z.string().optional(),
  note: z.string().optional(),
})

type FormValues = z.infer<typeof schema>

interface Props {
  open: boolean
  supplier?: SupplierDto | null
  onClose: () => void
}

export default function SupplierFormModal({ open, supplier, onClose }: Props) {
  const isEdit = !!supplier
  const { create, update } = useSupplierMutations()

  const { control, handleSubmit, reset, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { code: '', name: '', phone: '', email: '', address: '', note: '' },
  })

  useEffect(() => {
    if (supplier) {
      reset({ code: supplier.code, name: supplier.name, phone: supplier.phone ?? '', email: supplier.email ?? '', address: supplier.address ?? '', note: supplier.note ?? '' })
    } else {
      reset({ code: '', name: '', phone: '', email: '', address: '', note: '' })
    }
  }, [supplier, reset])

  const onSubmit = async (values: FormValues) => {
    if (isEdit && supplier) {
      await update.mutateAsync({ id: supplier.id, data: { name: values.name, phone: values.phone, email: values.email, address: values.address, note: values.note } })
    } else {
      await create.mutateAsync(values)
    }
    onClose()
  }

  const field = (name: keyof FormValues, label: string, node: React.ReactNode) => (
    <Form.Item label={label} validateStatus={errors[name] ? 'error' : ''} help={errors[name]?.message}>
      {node}
    </Form.Item>
  )

  return (
    <Modal open={open} title={isEdit ? 'Chỉnh sửa nhà cung cấp' : 'Thêm nhà cung cấp'} onCancel={onClose} onOk={handleSubmit(onSubmit)} okText={isEdit ? 'Lưu' : 'Tạo'} confirmLoading={isSubmitting} destroyOnClose width={520}>
      <Form layout="vertical" style={{ marginTop: 16 }}>
        {!isEdit && field('code', 'Mã NCC', <Controller name="code" control={control} render={({ field: f }) => <Input {...f} placeholder="NCC-001" />} />)}
        {field('name', 'Tên nhà cung cấp', <Controller name="name" control={control} render={({ field: f }) => <Input {...f} />} />)}
        {field('phone', 'Số điện thoại', <Controller name="phone" control={control} render={({ field: f }) => <Input {...f} />} />)}
        {field('email', 'Email', <Controller name="email" control={control} render={({ field: f }) => <Input {...f} />} />)}
        {field('address', 'Địa chỉ', <Controller name="address" control={control} render={({ field: f }) => <Input.TextArea {...f} rows={2} />} />)}
        {field('note', 'Ghi chú', <Controller name="note" control={control} render={({ field: f }) => <Input.TextArea {...f} rows={2} />} />)}
      </Form>
    </Modal>
  )
}
