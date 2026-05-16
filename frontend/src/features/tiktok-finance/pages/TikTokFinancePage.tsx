import { useState } from 'react'
import { DatePicker, Select, Space, Table, Tag, Typography } from 'antd'
import type { ColumnsType } from 'antd/es/table'
import dayjs from 'dayjs'
import { useTikTokShops } from '@/features/tiktok-shops/api/useTikTokShops'
import { useTikTokStatements } from '../api/useTikTokFinance'
import type { TikTokStatementDto } from '../types'

const { Title } = Typography
const { RangePicker } = DatePicker

export default function TikTokFinancePage() {
  const [page, setPage] = useState(1)
  const [connectionId, setConnectionId] = useState<string | undefined>()
  const [dateRange, setDateRange] = useState<[string, string] | undefined>()

  const { data: shopsData } = useTikTokShops()
  const { data, isLoading } = useTikTokStatements({
    pageNumber: page,
    pageSize: 20,
    connectionId,
    fromDate: dateRange?.[0],
    toDate: dateRange?.[1],
  })

  const shops = shopsData?.data ?? []
  const statements = data?.data?.items ?? []
  const total = data?.data?.totalCount ?? 0

  const columns: ColumnsType<TikTokStatementDto> = [
    {
      title: 'Statement ID',
      dataIndex: 'statementId',
      render: (v) => <code style={{ fontSize: 12 }}>{v}</code>,
    },
    {
      title: 'Shop',
      dataIndex: 'shopName',
      render: (s) => <Tag color="blue">{s}</Tag>,
    },
    {
      title: 'Loại',
      dataIndex: 'statementType',
    },
    {
      title: 'Trạng thái',
      dataIndex: 'status',
      render: (s) => <Tag>{s}</Tag>,
    },
    {
      title: 'Tổng tiền',
      dataIndex: 'totalAmount',
      align: 'right',
      render: (v, r) => `${Number(v).toLocaleString('vi-VN')} ${r.currency}`,
    },
    {
      title: 'Ngày sao kê',
      dataIndex: 'statementDate',
      width: 140,
      render: (v) => dayjs(v).format('DD/MM/YYYY'),
    },
  ]

  return (
    <Space direction="vertical" style={{ width: '100%' }} size="large">
      <Space style={{ justifyContent: 'space-between', width: '100%', flexWrap: 'wrap' }}>
        <Title level={4} style={{ margin: 0 }}>Tài chính TikTok</Title>
        <Space wrap>
          <Select
            allowClear
            placeholder="Lọc theo Shop"
            style={{ width: 200 }}
            options={shops.map((s) => ({ value: s.id, label: s.shopName }))}
            onChange={setConnectionId}
          />
          <RangePicker
            onChange={(_, strs) =>
              setDateRange(strs[0] && strs[1] ? [strs[0], strs[1]] : undefined)
            }
          />
        </Space>
      </Space>

      <Table
        rowKey="id"
        loading={isLoading}
        dataSource={statements}
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
