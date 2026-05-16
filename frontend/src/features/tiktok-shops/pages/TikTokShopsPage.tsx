import { useState } from 'react'
import { App, Badge, Button, Popconfirm, Space, Table, Tag, Tooltip, Typography } from 'antd'
import {
  DeleteOutlined,
  LinkOutlined,
  SyncOutlined,
} from '@ant-design/icons'
import type { ColumnsType } from 'antd/es/table'
import dayjs from 'dayjs'
import { tikTokShopsApi } from '../api/tikTokShopsApi'
import { useTikTokShopMutations, useTikTokShops } from '../api/useTikTokShops'
import type { TikTokConnectionDto } from '../types'

const { Title } = Typography

const STATUS_COLOR: Record<string, string> = {
  Active: 'success',
  Expired: 'warning',
  Revoked: 'error',
  Error: 'error',
}

export default function TikTokShopsPage() {
  const { message } = App.useApp()
  const { data, isLoading } = useTikTokShops()
  const { remove, refreshToken } = useTikTokShopMutations()
  const [connecting, setConnecting] = useState(false)

  const shops = data?.data ?? []

  const handleConnect = async () => {
    setConnecting(true)
    try {
      const res = await tikTokShopsApi.getAuthUrl(window.location.href)
      if (res.data?.authUrl) {
        window.location.href = res.data.authUrl
      }
    } catch {
      message.error('Không thể lấy URL kết nối TikTok')
    } finally {
      setConnecting(false)
    }
  }

  const columns: ColumnsType<TikTokConnectionDto> = [
    {
      title: 'Shop',
      dataIndex: 'shopName',
      render: (name, record) => (
        <Space direction="vertical" size={0}>
          <span style={{ fontWeight: 500 }}>{name}</span>
          <span style={{ fontSize: 12, color: '#888' }}>ID: {record.shopId}</span>
        </Space>
      ),
    },
    {
      title: 'Vùng',
      dataIndex: 'region',
      width: 90,
      render: (r) => <Tag>{r}</Tag>,
    },
    {
      title: 'Trạng thái',
      dataIndex: 'status',
      width: 120,
      render: (s) => <Badge status={STATUS_COLOR[s] as any} text={s} />,
    },
    {
      title: 'Token hết hạn',
      dataIndex: 'tokenExpiresAt',
      width: 170,
      render: (v) => dayjs(v).format('DD/MM/YYYY HH:mm'),
    },
    {
      title: 'Đồng bộ cuối',
      dataIndex: 'lastSyncedAt',
      width: 160,
      render: (v) => (v ? dayjs(v).format('DD/MM/YYYY HH:mm') : '—'),
    },
    {
      title: 'Thao tác',
      key: 'action',
      width: 130,
      render: (_, record) => (
        <Space>
          <Tooltip title="Làm mới token">
            <Button
              size="small"
              icon={<SyncOutlined />}
              loading={refreshToken.isPending}
              onClick={() => refreshToken.mutate(record.id)}
            />
          </Tooltip>
          <Popconfirm
            title="Xóa kết nối này?"
            onConfirm={() => remove.mutate(record.id)}
          >
            <Button size="small" danger icon={<DeleteOutlined />} />
          </Popconfirm>
        </Space>
      ),
    },
  ]

  return (
    <Space direction="vertical" style={{ width: '100%' }} size="large">
      <Space style={{ justifyContent: 'space-between', width: '100%' }}>
        <Title level={4} style={{ margin: 0 }}>TikTok Shops</Title>
        <Button
          type="primary"
          icon={<LinkOutlined />}
          loading={connecting}
          onClick={handleConnect}
        >
          Kết nối Shop mới
        </Button>
      </Space>

      <Table
        rowKey="id"
        loading={isLoading}
        dataSource={shops}
        columns={columns}
        pagination={false}
        size="small"
      />
    </Space>
  )
}
