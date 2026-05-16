import { useState } from 'react'
import { Outlet, useNavigate, useLocation } from 'react-router-dom'
import { Layout, Menu, Avatar, Dropdown, Typography, theme } from 'antd'
import {
  DashboardOutlined,
  ShoppingOutlined,
  TeamOutlined,
  ImportOutlined,
  ExportOutlined,
  InboxOutlined,
  UserOutlined,
  SettingOutlined,
  LogoutOutlined,
  MenuFoldOutlined,
  MenuUnfoldOutlined,
  LinkOutlined,
  SwapOutlined,
  FileTextOutlined,
  RollbackOutlined,
  DollarOutlined,
} from '@ant-design/icons'
import { useAuthStore } from '@/features/auth/store/useAuthStore'
import { authApi } from '@/features/auth/api/authApi'
import { ROUTES } from '@/shared/constants/routes'
import type { MenuProps } from 'antd'

const { Header, Sider, Content } = Layout
const { Text } = Typography

export default function MainLayout() {
  const [collapsed, setCollapsed] = useState(false)
  const { token } = theme.useToken()
  const navigate = useNavigate()
  const location = useLocation()
  const { user, hasRole, logout, refreshToken } = useAuthStore()

  const handleLogout = async () => {
    if (refreshToken) {
      try {
        await authApi.logout(refreshToken)
      } catch {
        // ignore — we still clear local state
      }
    }
    logout()
    navigate(ROUTES.LOGIN)
  }

  const menuItems: MenuProps['items'] = [
    {
      key: ROUTES.DASHBOARD,
      icon: <DashboardOutlined />,
      label: 'Dashboard',
    },
    {
      key: ROUTES.PRODUCTS,
      icon: <ShoppingOutlined />,
      label: 'Sản phẩm',
    },
    ...(hasRole('Manager')
      ? [
          {
            key: ROUTES.SUPPLIERS,
            icon: <TeamOutlined />,
            label: 'Nhà cung cấp',
          },
          {
            key: ROUTES.STOCK_INS,
            icon: <ImportOutlined />,
            label: 'Nhập hàng',
          },
        ]
      : []),
    {
      key: ROUTES.STOCK_OUTS,
      icon: <ExportOutlined />,
      label: 'Bán hàng',
    },
    {
      key: ROUTES.INVENTORY,
      icon: <InboxOutlined />,
      label: 'Tồn kho',
    },
    {
      key: 'tiktok-group',
      icon: <LinkOutlined />,
      label: 'TikTok',
      children: [
        {
          key: ROUTES.TIKTOK_ORDERS,
          icon: <FileTextOutlined />,
          label: 'Đơn hàng',
        },
        {
          key: ROUTES.TIKTOK_RETURNS,
          icon: <RollbackOutlined />,
          label: 'Hoàn trả',
        },
        {
          key: ROUTES.TIKTOK_FINANCE,
          icon: <DollarOutlined />,
          label: 'Tài chính',
        },
        ...(hasRole('Manager')
          ? [
              {
                key: ROUTES.PRODUCT_MAPPINGS,
                icon: <SwapOutlined />,
                label: 'Product Mappings',
              },
            ]
          : []),
      ],
    },
    ...(hasRole('Admin')
      ? [
          {
            key: ROUTES.USERS,
            icon: <UserOutlined />,
            label: 'Người dùng',
          },
          {
            key: ROUTES.TIKTOK_SHOPS,
            icon: <SettingOutlined />,
            label: 'TikTok Shops',
          },
        ]
      : []),
  ]

  const userMenuItems: MenuProps['items'] = [
    {
      key: 'profile',
      icon: <UserOutlined />,
      label: 'Hồ sơ',
      onClick: () => navigate(ROUTES.PROFILE),
    },
    { type: 'divider' },
    {
      key: 'logout',
      icon: <LogoutOutlined />,
      label: 'Đăng xuất',
      danger: true,
      onClick: handleLogout,
    },
  ]

  return (
    <Layout style={{ minHeight: '100vh' }}>
      <Sider
        trigger={null}
        collapsible
        collapsed={collapsed}
        style={{ background: token.colorBgContainer }}
      >
        <div
          style={{
            height: 64,
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            borderBottom: `1px solid ${token.colorBorderSecondary}`,
            padding: '0 16px',
          }}
        >
          <Text strong style={{ fontSize: collapsed ? 14 : 16, whiteSpace: 'nowrap' }}>
            {collapsed ? 'TTS' : 'TikTok Shop'}
          </Text>
        </div>

        <Menu
          mode="inline"
          selectedKeys={[location.pathname]}
          items={menuItems}
          style={{ borderRight: 0, flex: 1 }}
          onClick={({ key }) => navigate(key)}
        />
      </Sider>

      <Layout>
        <Header
          style={{
            background: token.colorBgContainer,
            padding: '0 24px',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'space-between',
            borderBottom: `1px solid ${token.colorBorderSecondary}`,
          }}
        >
          <div
            style={{ cursor: 'pointer', fontSize: 18 }}
            onClick={() => setCollapsed(!collapsed)}
          >
            {collapsed ? <MenuUnfoldOutlined /> : <MenuFoldOutlined />}
          </div>

          <Dropdown menu={{ items: userMenuItems }} placement="bottomRight" trigger={['click']}>
            <div style={{ cursor: 'pointer', display: 'flex', alignItems: 'center', gap: 8 }}>
              <Avatar icon={<UserOutlined />} style={{ background: token.colorPrimary }} />
              {!collapsed && (
                <Text style={{ maxWidth: 160, overflow: 'hidden', textOverflow: 'ellipsis' }}>
                  {user?.fullName}
                </Text>
              )}
            </div>
          </Dropdown>
        </Header>

        <Content style={{ margin: 24, overflow: 'auto' }}>
          <Outlet />
        </Content>
      </Layout>
    </Layout>
  )
}
