import { Outlet } from 'react-router-dom'
import { Layout, theme } from 'antd'

const { Content } = Layout
const { useToken } = theme

export default function AuthLayout() {
  const { token } = useToken()

  return (
    <Layout style={{ minHeight: '100vh', background: token.colorBgLayout }}>
      <Content
        style={{
          display: 'flex',
          justifyContent: 'center',
          alignItems: 'center',
          padding: '24px',
        }}
      >
        <Outlet />
      </Content>
    </Layout>
  )
}
