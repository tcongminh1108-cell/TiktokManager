import { useState } from 'react'
import { Button, Select, Space, Table, Tag, Typography } from 'antd'
import { SyncOutlined } from '@ant-design/icons'
import type { ColumnsType } from 'antd/es/table'
import dayjs from 'dayjs'
import { useTikTokShops } from '@/features/tiktok-shops/api/useTikTokShops'
import { useTikTokOrderMutations, useTikTokOrders } from '../api/useTikTokOrders'
import type { TikTokOrderDto } from '../types'

const { Title } = Typography

const STATUS_LABELS: Record<number, { label: string; color: string }> = {
  100: { label: 'Chưa thanh toán', color: 'default' },
  111: { label: 'Chờ giao', color: 'blue' },
  112: { label: 'Đã giao shipper', color: 'cyan' },
  121: { label: 'Đang vận chuyển', color: 'processing' },
  122: { label: 'Đã giao', color: 'green' },
  130: { label: 'Hoàn tất', color: 'success' },
  140: { label: 'Đã hủy', color: 'error' },
}

const SYNC_STATUS_COLOR: Record<string, string> = {
  Synced: 'success',
  MappingPending: 'warning',
  Reserved: 'blue',
  StockApplied: 'green',
  StockReversed: 'orange',
  Failed: 'error',
}

export default function TikTokOrdersPage() {
  const [page, setPage] = useState(1)
  const [connectionId, setConnectionId] = useState<string | undefined>()

  const { data: shopsData } = useTikTokShops()
  const { data, isLoading } = useTikTokOrders({ pageNumber: page, pageSize: 20, connectionId })
  const { syncNow } = useTikTokOrderMutations()

  const shops = shopsData?.data ?? []
  const orders = data?.data?.items ?? []
  const total = data?.data?.totalCount ?? 0

  const columns: ColumnsType<TikTokOrderDto> = [
    {
      title: 'Mã đơn',
      dataIndex: 'orderId',
      render: (v) => <code style={{ fontSize: 12 }}>{v}</code>,
    },
    {
      title: 'Shop',
      dataIndex: 'shopName',
      render: (s) => <Tag color="blue">{s}</Tag>,
    },
    {
      title: 'Trạng thái TikTok',
      dataIndex: 'statusCode',
      render: (code: number) => {
        const info = STATUS_LABELS[code] ?? { label: `${code}`, color: 'default' }
        return <Tag color={info.color}>{info.label}</Tag>
      },
    },
    {
      title: 'Sync',
      dataIndex: 'syncStatus',
      render: (s) => <Tag color={SYNC_STATUS_COLOR[s] ?? 'default'}>{s}</Tag>,
    },
    {
      title: 'Giá trị',
      dataIndex: 'totalAmount',
      align: 'right',
      render: (v, r) =>
        `${Number(v).toLocaleString('vi-VN')} ${r.currency}`,
    },
    {
      title: 'Cập nhật',
      dataIndex: 'updatedAt',
      width: 160,
      render: (v) => dayjs(v).format('DD/MM/YY HH:mm'),
    },
  ]

  return (
    <Space direction="vertical" style={{ width: '100%' }} size="large">
      <Space style={{ justifyContent: 'space-between', width: '100%' }}>
        <Title level={4} style={{ margin: 0 }}>Đơn hàng TikTok</Title>
        <Space>
          <Select
            allowClear
            placeholder="Lọc theo Shop"
            style={{ width: 200 }}
            options={shops.map((s) => ({ value: s.id, label: s.shopName }))}
            onChange={setConnectionId}
          />
          <Button
            icon={<SyncOutlined />}
            loading={syncNow.isPending}
            disabled={!connectionId}
            onClick={() => connectionId && syncNow.mutate(connectionId)}
          >
            Sync ngay
          </Button>
        </Space>
      </Space>

      <Table
        rowKey="id"
        loading={isLoading}
        dataSource={orders}
        columns={columns}
        size="small"
        pagination={{
          current: page,
          pageSize: 20,
          total,
          onChange: setPage,
          showSizeChanger: false,
        }}
      />
    </Space>
  )
}
