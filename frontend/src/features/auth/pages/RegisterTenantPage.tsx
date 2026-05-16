import { useNavigate, Link } from 'react-router-dom'
import { App, Button, Card, Col, Form, Input, Row, Typography } from 'antd'
import { useForm, Controller } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { authApi } from '../api/authApi'
import { useAuthStore } from '../store/useAuthStore'
import { ROUTES } from '@/shared/constants/routes'

const { Title, Text } = Typography

const schema = z.object({
  tenantName: z.string().min(2, 'Tên shop tối thiểu 2 ký tự'),
  tenantCode: z
    .string()
    .min(2, 'Code tối thiểu 2 ký tự')
    .max(20, 'Code tối đa 20 ký tự')
    .regex(/^[a-z0-9-]+$/, 'Chỉ dùng chữ thường, số và dấu gạch ngang'),
  contactEmail: z.string().email('Email không hợp lệ'),
  contactPhone: z.string().optional(),
  adminEmail: z.string().email('Email không hợp lệ'),
  adminPassword: z.string().min(8, 'Mật khẩu tối thiểu 8 ký tự'),
  adminFullName: z.string().min(2, 'Tên tối thiểu 2 ký tự'),
})

type FormValues = z.infer<typeof schema>

export default function RegisterTenantPage() {
  const navigate = useNavigate()
  const login = useAuthStore((s) => s.login)
  const { message } = App.useApp()

  const {
    control,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: {
      tenantName: '',
      tenantCode: '',
      contactEmail: '',
      contactPhone: '',
      adminEmail: '',
      adminPassword: '',
      adminFullName: '',
    },
  })

  const onSubmit = async (values: FormValues) => {
    try {
      const res = await authApi.register(values)
      if (!res.success || !res.data) throw new Error(res.message ?? 'Đăng ký thất bại')
      login(res.data.accessToken, res.data.refreshToken, res.data.user)
      message.success('Đăng ký thành công!')
      navigate(ROUTES.DASHBOARD, { replace: true })
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'Đăng ký thất bại'
      message.error(msg)
    }
  }

  return (
    <Card style={{ width: 560, boxShadow: '0 4px 24px rgba(0,0,0,.08)' }}>
      <Title level={3} style={{ textAlign: 'center', marginBottom: 24 }}>
        Đăng ký Tenant
      </Title>

      <Form layout="vertical" onFinish={handleSubmit(onSubmit)} autoComplete="off">
        <Title level={5} style={{ marginBottom: 12 }}>
          Thông tin Shop
        </Title>

        <Row gutter={16}>
          <Col span={14}>
            <Form.Item
              label="Tên shop"
              validateStatus={errors.tenantName ? 'error' : ''}
              help={errors.tenantName?.message}
            >
              <Controller
                name="tenantName"
                control={control}
                render={({ field }) => <Input {...field} placeholder="My TikTok Shop" />}
              />
            </Form.Item>
          </Col>
          <Col span={10}>
            <Form.Item
              label="Tenant code"
              validateStatus={errors.tenantCode ? 'error' : ''}
              help={errors.tenantCode?.message}
            >
              <Controller
                name="tenantCode"
                control={control}
                render={({ field }) => <Input {...field} placeholder="my-shop" />}
              />
            </Form.Item>
          </Col>
        </Row>

        <Row gutter={16}>
          <Col span={14}>
            <Form.Item
              label="Email liên hệ"
              validateStatus={errors.contactEmail ? 'error' : ''}
              help={errors.contactEmail?.message}
            >
              <Controller
                name="contactEmail"
                control={control}
                render={({ field }) => <Input {...field} placeholder="contact@shop.com" />}
              />
            </Form.Item>
          </Col>
          <Col span={10}>
            <Form.Item
              label="SĐT liên hệ"
              validateStatus={errors.contactPhone ? 'error' : ''}
              help={errors.contactPhone?.message}
            >
              <Controller
                name="contactPhone"
                control={control}
                render={({ field }) => <Input {...field} placeholder="0901234567" />}
              />
            </Form.Item>
          </Col>
        </Row>

        <Title level={5} style={{ marginBottom: 12 }}>
          Tài khoản Admin
        </Title>

        <Form.Item
          label="Họ tên"
          validateStatus={errors.adminFullName ? 'error' : ''}
          help={errors.adminFullName?.message}
        >
          <Controller
            name="adminFullName"
            control={control}
            render={({ field }) => <Input {...field} placeholder="Nguyễn Văn A" />}
          />
        </Form.Item>

        <Row gutter={16}>
          <Col span={14}>
            <Form.Item
              label="Email"
              validateStatus={errors.adminEmail ? 'error' : ''}
              help={errors.adminEmail?.message}
            >
              <Controller
                name="adminEmail"
                control={control}
                render={({ field }) => <Input {...field} placeholder="admin@shop.com" />}
              />
            </Form.Item>
          </Col>
          <Col span={10}>
            <Form.Item
              label="Mật khẩu"
              validateStatus={errors.adminPassword ? 'error' : ''}
              help={errors.adminPassword?.message}
            >
              <Controller
                name="adminPassword"
                control={control}
                render={({ field }) => <Input.Password {...field} placeholder="••••••••" />}
              />
            </Form.Item>
          </Col>
        </Row>

        <Form.Item style={{ marginBottom: 8 }}>
          <Button type="primary" htmlType="submit" block loading={isSubmitting}>
            Đăng ký
          </Button>
        </Form.Item>

        <div style={{ textAlign: 'center' }}>
          <Text type="secondary">Đã có tài khoản? </Text>
          <Link to={ROUTES.LOGIN}>Đăng nhập</Link>
        </div>
      </Form>
    </Card>
  )
}
