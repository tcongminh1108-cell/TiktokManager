import { useEffect } from 'react'
import { Form, Input, InputNumber, Modal } from 'antd'
import { useForm, Controller } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { useProductMutations } from '../api/useProducts'
import type { ProductDto } from '../types'

const schema = z.object({
  code: z.string().min(1, 'Bắt buộc').max(50),
  name: z.string().min(1, 'Bắt buộc').max(200),
  description: z.string().optional(),
  sellingPrice: z.number({ error: 'Nhập số' }).positive('Phải > 0'),
  unit: z.string().min(1, 'Bắt buộc').max(50),
  imageUrl: z.string().optional(),
})

type FormValues = z.infer<typeof schema>

interface Props {
  open: boolean
  product?: ProductDto | null
  onClose: () => void
}

export default function ProductFormModal({ open, product, onClose }: Props) {
  const isEdit = !!product
  const { create, update } = useProductMutations()

  const { control, handleSubmit, reset, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { code: '', name: '', description: '', sellingPrice: 0, unit: '', imageUrl: '' },
  })

  useEffect(() => {
    if (product) {
      reset({ code: product.code, name: product.name, description: product.description ?? '', sellingPrice: product.sellingPrice, unit: product.unit, imageUrl: product.imageUrl ?? '' })
    } else {
      reset({ code: '', name: '', description: '', sellingPrice: 0, unit: '', imageUrl: '' })
    }
  }, [product, reset])

  const onSubmit = async (values: FormValues) => {
    if (isEdit && product) {
      await update.mutateAsync({ id: product.id, data: { name: values.name, description: values.description, sellingPrice: values.sellingPrice, unit: values.unit, imageUrl: values.imageUrl } })
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
    <Modal
      open={open}
      title={isEdit ? 'Chỉnh sửa sản phẩm' : 'Thêm sản phẩm'}
      onCancel={onClose}
      onOk={handleSubmit(onSubmit)}
      okText={isEdit ? 'Lưu' : 'Tạo'}
      confirmLoading={isSubmitting}
      destroyOnClose
      width={560}
    >
      <Form layout="vertical" style={{ marginTop: 16 }}>
        {!isEdit && field('code', 'Mã sản phẩm', (
          <Controller name="code" control={control} render={({ field: f }) => <Input {...f} placeholder="SP-001" />} />
        ))}
        {field('name', 'Tên sản phẩm', (
          <Controller name="name" control={control} render={({ field: f }) => <Input {...f} placeholder="Tên sản phẩm" />} />
        ))}
        {field('description', 'Mô tả', (
          <Controller name="description" control={control} render={({ field: f }) => <Input.TextArea {...f} rows={2} />} />
        ))}
        {field('sellingPrice', 'Giá bán (VND)', (
          <Controller name="sellingPrice" control={control} render={({ field: f }) => (
            <InputNumber {...f} style={{ width: '100%' }} min={0} step={1000} formatter={(v) => `${v}`.replace(/\B(?=(\d{3})+(?!\d))/g, ',')} parser={(v) => Number(v?.replace(/,/g, '')) as never} />
          )} />
        ))}
        {field('unit', 'Đơn vị', (
          <Controller name="unit" control={control} render={({ field: f }) => <Input {...f} placeholder="pcs, kg, box..." /> } />
        ))}
        {field('imageUrl', 'URL hình ảnh', (
          <Controller name="imageUrl" control={control} render={({ field: f }) => <Input {...f} placeholder="https://..." />} />
        ))}
      </Form>
    </Modal>
  )
}
