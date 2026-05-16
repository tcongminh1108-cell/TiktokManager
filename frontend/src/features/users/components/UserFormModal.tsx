import { useEffect } from 'react'
import { Form, Input, Modal, Select } from 'antd'
import { useForm, Controller } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { useUserMutations } from '../api/useUsers'
import type { UserDto } from '../types'

const createSchema = z.object({
  email: z.string().email('Email không hợp lệ'),
  fullName: z.string().min(1, 'Nhập họ tên'),
  password: z.string().min(6, 'Ít nhất 6 ký tự'),
  role: z.enum(['Admin', 'Manager', 'Staff']),
})

const editSchema = z.object({
  fullName: z.string().min(1, 'Nhập họ tên'),
  role: z.enum(['Admin', 'Manager', 'Staff']),
})

type CreateValues = z.infer<typeof createSchema>
type EditValues = z.infer<typeof editSchema>
type FormValues = CreateValues

interface Props {
  open: boolean
  editing?: UserDto | null
  onClose: () => void
}

const ROLE_OPTIONS = [
  { label: 'Admin', value: 'Admin' },
  { label: 'Manager', value: 'Manager' },
  { label: 'Staff', value: 'Staff' },
]

export default function UserFormModal({ open, editing, onClose }: Props) {
  const isEdit = !!editing
  const { create, update } = useUserMutations()

  const { control, handleSubmit, reset, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(isEdit ? (editSchema as unknown as typeof createSchema) : createSchema),
    defaultValues: { email: '', fullName: '', password: '', role: 'Staff' },
  })

  useEffect(() => {
    if (editing) {
      reset({ email: editing.email, fullName: editing.fullName, password: '', role: editing.role })
    } else {
      reset({ email: '', fullName: '', password: '', role: 'Staff' })
    }
  }, [editing, reset])

  const onSubmit = async (values: FormValues) => {
    if (isEdit && editing) {
      const payload: EditValues = { fullName: values.fullName, role: values.role }
      await update.mutateAsync({ id: editing.id, data: payload })
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
    <Modal open={open} title={isEdit ? 'Chỉnh sửa người dùng' : 'Thêm người dùng'} onCancel={onClose} onOk={handleSubmit(onSubmit)} okText={isEdit ? 'Lưu' : 'Tạo'} confirmLoading={isSubmitting} destroyOnClose width={480}>
      <Form layout="vertical" style={{ marginTop: 16 }}>
        {fi('email', 'Email', <Controller name="email" control={control} render={({ field: f }) => <Input {...f} disabled={isEdit} placeholder="email@example.com" />} />)}
        {fi('fullName', 'Họ và tên', <Controller name="fullName" control={control} render={({ field: f }) => <Input {...f} />} />)}
        {!isEdit && fi('password', 'Mật khẩu', <Controller name="password" control={control} render={({ field: f }) => <Input.Password {...f} />} />)}
        {fi('role', 'Vai trò', <Controller name="role" control={control} render={({ field: f }) => <Select {...f} options={ROLE_OPTIONS} style={{ width: '100%' }} />} />)}
      </Form>
    </Modal>
  )
}
