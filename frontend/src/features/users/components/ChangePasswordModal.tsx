import { Form, Input, Modal } from 'antd'
import { useForm, Controller } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { useUserMutations } from '../api/useUsers'
import type { UserDto } from '../types'

const schema = z.object({
  newPassword: z.string().min(6, 'Ít nhất 6 ký tự'),
  confirmPassword: z.string().min(1, 'Nhập lại mật khẩu'),
}).refine((d) => d.newPassword === d.confirmPassword, {
  message: 'Mật khẩu không khớp',
  path: ['confirmPassword'],
})

type FormValues = z.infer<typeof schema>

interface Props {
  open: boolean
  user?: UserDto | null
  onClose: () => void
}

export default function ChangePasswordModal({ open, user, onClose }: Props) {
  const { changePassword } = useUserMutations()

  const { control, handleSubmit, reset, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { newPassword: '', confirmPassword: '' },
  })

  const onSubmit = async (values: FormValues) => {
    if (!user) return
    await changePassword.mutateAsync({ id: user.id, data: values })
    reset()
    onClose()
  }

  const fi = (name: keyof FormValues, label: string, node: React.ReactNode) => (
    <Form.Item label={label} validateStatus={errors[name] ? 'error' : ''} help={errors[name]?.message}>
      {node}
    </Form.Item>
  )

  return (
    <Modal open={open} title={`Đổi mật khẩu — ${user?.fullName}`} onCancel={() => { reset(); onClose() }} onOk={handleSubmit(onSubmit)} okText="Đổi mật khẩu" confirmLoading={isSubmitting} destroyOnClose width={420}>
      <Form layout="vertical" style={{ marginTop: 16 }}>
        {fi('newPassword', 'Mật khẩu mới', <Controller name="newPassword" control={control} render={({ field: f }) => <Input.Password {...f} />} />)}
        {fi('confirmPassword', 'Nhập lại', <Controller name="confirmPassword" control={control} render={({ field: f }) => <Input.Password {...f} />} />)}
      </Form>
    </Modal>
  )
}
