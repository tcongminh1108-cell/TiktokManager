import { useNavigate, Link } from 'react-router-dom'
import { App, Button, Card, Form, Input, Typography } from 'antd'
import { LockOutlined, MailOutlined } from '@ant-design/icons'
import { useForm, Controller } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { authApi } from '../api/authApi'
import { useAuthStore } from '../store/useAuthStore'
import { ROUTES } from '@/shared/constants/routes'

const { Title, Text } = Typography

const schema = z.object({
  email: z.string().email('Email không hợp lệ'),
  password: z.string().min(1, 'Mật khẩu không được để trống'),
})

type FormValues = z.infer<typeof schema>

export default function LoginPage() {
  const navigate = useNavigate()
  const login = useAuthStore((s) => s.login)
  const { message } = App.useApp()

  const {
    control,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { email: '', password: '' },
  })

  const onSubmit = async (values: FormValues) => {
    try {
      const res = await authApi.login(values)
      if (!res.success || !res.data) throw new Error(res.message ?? 'Đăng nhập thất bại')
      login(res.data.accessToken, res.data.refreshToken, res.data.user)
      navigate(ROUTES.DASHBOARD, { replace: true })
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'Đăng nhập thất bại'
      message.error(msg)
    }
  }

  return (
    <Card style={{ width: 400, boxShadow: '0 4px 24px rgba(0,0,0,.08)' }}>
      <Title level={3} style={{ textAlign: 'center', marginBottom: 24 }}>
        Đăng nhập
      </Title>

      <Form layout="vertical" onFinish={handleSubmit(onSubmit)} autoComplete="off">
        <Form.Item
          label="Email"
          validateStatus={errors.email ? 'error' : ''}
          help={errors.email?.message}
        >
          <Controller
            name="email"
            control={control}
            render={({ field }) => (
              <Input {...field} prefix={<MailOutlined />} placeholder="admin@example.com" />
            )}
          />
        </Form.Item>

        <Form.Item
          label="Mật khẩu"
          validateStatus={errors.password ? 'error' : ''}
          help={errors.password?.message}
        >
          <Controller
            name="password"
            control={control}
            render={({ field }) => (
              <Input.Password {...field} prefix={<LockOutlined />} placeholder="••••••••" />
            )}
          />
        </Form.Item>

        <Form.Item style={{ marginBottom: 8 }}>
          <Button type="primary" htmlType="submit" block loading={isSubmitting}>
            Đăng nhập
          </Button>
        </Form.Item>

        <div style={{ textAlign: 'center' }}>
          <Text type="secondary">Chưa có tài khoản? </Text>
          <Link to={ROUTES.REGISTER_TENANT}>Đăng ký tenant</Link>
        </div>
      </Form>
    </Card>
  )
}
