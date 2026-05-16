import { useState } from 'react'
import { Select, Space, Table, Tag, Typography } from 'antd'
import type { ColumnsType } from 'antd/es/table'
import dayjs from 'dayjs'
import { useTikTokShops } from '@/features/tiktok-shops/api/useTikTokShops'
import { useTikTokReturns } from '../api/useTikTokReturns'
import type { TikTokReturnDto } from '../types'

const { Title } = Typography

const STATUS_COLOR: Record<string, string> = {
  Requested: 'blue',
  Approved: 'cyan',
  Rejected: 'error',
  ReturnReceived: 'orange',
  Refunded: 'success',
  Closed: 'default',
}

export default function TikTokReturnsPage() {
  const [page, setPage] = useState(1)
  const [connectionId, setConnectionId] = useState<string | undefined>()

  const { data: shopsData } = useTikTokShops()
  const { data, isLoading } = useTikTokReturns({ pageNumber: page, pageSize: 20, connectionId })

  const shops = shopsData?.data ?? []
  const returns = data?.data?.items ?? []
  const total = data?.data?.totalCount ?? 0

  const columns: ColumnsType<TikTokReturnDto> = [
    {
      title: 'Return ID',
      dataIndex: 'returnId',
      render: (v) => <code style={{ fontSize: 12 }}>{v}</code>,
    },
    {
      title: 'Đơn gốc',
      dataIndex: 'orderId',
      render: (v) => <code style={{ fontSize: 12 }}>{v}</code>,
    },
    {
      title: 'Shop',
      dataIndex: 'shopName',
      render: (s) => <Tag color="blue">{s}</Tag>,
    },
    {
      title: 'Trạng thái',
      dataIndex: 'status',
      render: (s) => <Tag color={STATUS_COLOR[s] ?? 'default'}>{s}</Tag>,
    },
    {
      title: 'Lý do',
      dataIndex: 'returnReason',
      render: (v) => v ?? '—',
    },
    {
      title: 'Hoàn tiền',
      dataIndex: 'refundAmount',
      align: 'right',
      render: (v, r) => `${Number(v).toLocaleString('vi-VN')} ${r.currency}`,
    },
    {
      title: 'Cập nhật',
      dataIndex: 'updatedAt',
      width: 150,
      render: (v) => dayjs(v).format('DD/MM/YY HH:mm'),
    },
  ]

  return (
    <Space direction="vertical" style={{ width: '100%' }} size="large">
      <Space style={{ justifyContent: 'space-between', width: '100%' }}>
        <Title level={4} style={{ margin: 0 }}>Hoàn trả TikTok</Title>
        <Select
          allowClear
          placeholder="Lọc theo Shop"
          style={{ width: 200 }}
          options={shops.map((s) => ({ value: s.id, label: s.shopName }))}
          onChange={setConnectionId}
        />
      </Space>

      <Table
        rowKey="id"
        loading={isLoading}
        dataSource={returns}
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
