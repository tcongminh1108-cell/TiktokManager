import { useState } from 'react'
import { Card, Col, DatePicker, Row, Select, Spin, Statistic, Table, Tag, Typography, Button, Space } from 'antd'
import {
  ArrowUpOutlined, ArrowDownOutlined, ShoppingOutlined, TeamOutlined,
  InboxOutlined, SwapOutlined, DownloadOutlined,
} from '@ant-design/icons'
import type { ColumnsType } from 'antd/es/table'
import dayjs from 'dayjs'
import {
  LineChart, Line, BarChart, Bar, PieChart, Pie, Cell, Legend,
  XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer,
} from 'recharts'
import { formatVND } from '@/shared/lib/format'
import {
  useOverview, useRevenueByDay, useTopProducts,
  useRevenueBySource, useProductProfit,
} from '../api/useDashboard'
import type { DashboardParams, ProductProfitDto } from '../types'

const { Title, Text } = Typography
const { RangePicker } = DatePicker

const PIE_COLORS = ['#1677ff', '#52c41a', '#faad14', '#ff4d4f', '#722ed1']
const SOURCE_LABELS: Record<string, string> = {
  Manual: 'Thủ công', TikTokOrder: 'TikTok', Adjustment: 'Điều chỉnh', Import: 'Import', TikTokReturn: 'TikTok Return',
}

function KpiCard({ title, value, prefix, suffix, color, icon }: { title: string; value: number; prefix?: string; suffix?: string; color?: string; icon?: React.ReactNode }) {
  return (
    <Card size="small" style={{ height: '100%' }}>
      <Statistic
        title={<Space>{icon}<span>{title}</span></Space>}
        value={value}
        prefix={prefix}
        suffix={suffix}
        valueStyle={color ? { color } : undefined}
        formatter={prefix === '₫' ? ((v) => formatVND(Number(v))) : undefined}
      />
    </Card>
  )
}

function exportProfitCsv(items: ProductProfitDto[]) {
  const header = ['Mã SP', 'Tên SP', 'SL bán', 'Doanh thu', 'Giá vốn TB', 'Lợi nhuận', 'Biên %', 'SL Manual', 'SL TikTok']
  const rows = items.map((i) => [
    i.productCode, i.productName, i.totalSoldQty, i.grossRevenue,
    i.avgCostPrice ?? '', i.grossProfit, i.marginPercent, i.manualSoldQty, i.tikTokSoldQty,
  ])
  const csv = [header, ...rows].map((r) => r.join(',')).join('\n')
  const blob = new Blob(['﻿' + csv], { type: 'text/csv;charset=utf-8;' })
  const a = Object.assign(document.createElement('a'), {
    href: URL.createObjectURL(blob),
    download: `profit_${new Date().toISOString().slice(0, 10)}.csv`,
  })
  a.click()
  URL.revokeObjectURL(a.href)
}

export default function DashboardPage() {
  const [dateRange, setDateRange] = useState<[dayjs.Dayjs, dayjs.Dayjs]>([dayjs().subtract(30, 'day'), dayjs()])
  const [source, setSource] = useState<DashboardParams['source']>('All')
  const [profitPage, setProfitPage] = useState(1)

  const params: DashboardParams = {
    from: dateRange[0].startOf('day').toISOString(),
    to: dateRange[1].endOf('day').toISOString(),
    source,
  }

  const { data: overviewData, isLoading: overviewLoading } = useOverview(params)
  const { data: revenueByDayData } = useRevenueByDay(params)
  const { data: topProductsData } = useTopProducts(params, 10)
  const { data: revenueBySourceData } = useRevenueBySource(params)
  const { data: profitData } = useProductProfit(params, profitPage, 10)

  const overview = overviewData?.data
  const revenueByDay = revenueByDayData?.data ?? []
  const topProducts = topProductsData?.data ?? []
  const revenueBySource = revenueBySourceData?.data ?? []
  const profit = profitData?.data

  const profitColumns: ColumnsType<ProductProfitDto> = [
    { title: 'Sản phẩm', key: 'product', render: (_, r) => <span>{r.productCode} — {r.productName}</span>, ellipsis: true },
    { title: 'SL bán', dataIndex: 'totalSoldQty', key: 'totalSoldQty', width: 80, align: 'right' },
    { title: 'Doanh thu', dataIndex: 'grossRevenue', key: 'grossRevenue', width: 130, align: 'right', render: formatVND },
    { title: 'Giá vốn TB', dataIndex: 'avgCostPrice', key: 'avgCostPrice', width: 120, align: 'right',
      render: (v?: number) => v != null ? formatVND(v) : '—' },
    { title: 'Lợi nhuận', dataIndex: 'grossProfit', key: 'grossProfit', width: 130, align: 'right',
      render: (v: number) => <span style={{ color: v >= 0 ? '#52c41a' : '#ff4d4f' }}>{formatVND(v)}</span> },
    { title: 'Biên %', dataIndex: 'marginPercent', key: 'marginPercent', width: 80, align: 'right',
      render: (v: number) => <Tag color={v >= 20 ? 'success' : v >= 0 ? 'warning' : 'error'}>{v}%</Tag> },
    { title: 'Manual', dataIndex: 'manualSoldQty', key: 'manualSoldQty', width: 75, align: 'right' },
    { title: 'TikTok', dataIndex: 'tikTokSoldQty', key: 'tikTokSoldQty', width: 70, align: 'right' },
  ]

  return (
    <div>
      {/* Header + filters */}
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 20, flexWrap: 'wrap', gap: 12 }}>
        <Title level={4} style={{ margin: 0 }}>Dashboard</Title>
        <Space wrap>
          <RangePicker
            value={dateRange}
            onChange={(v) => v && setDateRange(v as [dayjs.Dayjs, dayjs.Dayjs])}
            format="DD/MM/YYYY"
            presets={[
              { label: '7 ngày', value: [dayjs().subtract(6, 'day'), dayjs()] },
              { label: '30 ngày', value: [dayjs().subtract(29, 'day'), dayjs()] },
              { label: 'Tháng này', value: [dayjs().startOf('month'), dayjs()] },
              { label: 'Tháng trước', value: [dayjs().subtract(1, 'month').startOf('month'), dayjs().subtract(1, 'month').endOf('month')] },
            ]}
          />
          <Select
            value={source}
            onChange={setSource}
            style={{ width: 150 }}
            options={[
              { label: 'Tất cả nguồn', value: 'All' },
              { label: 'Thủ công', value: 'Manual' },
              { label: 'TikTok', value: 'TikTokOrder' },
            ]}
          />
        </Space>
      </div>

      {/* KPI cards */}
      <Spin spinning={overviewLoading}>
        <Row gutter={[12, 12]} style={{ marginBottom: 20 }}>
          <Col xs={24} sm={12} lg={8}>
            <KpiCard title="Doanh thu gộp" value={overview?.grossRevenue ?? 0} prefix="₫" color="#1677ff" icon={<ArrowUpOutlined />} />
          </Col>
          <Col xs={24} sm={12} lg={8}>
            <KpiCard title="Chi phí nhập hàng" value={overview?.totalPurchaseCost ?? 0} prefix="₫" color="#faad14" icon={<ArrowDownOutlined />} />
          </Col>
          <Col xs={24} sm={12} lg={8}>
            <KpiCard title="Lợi nhuận gộp" value={overview?.grossProfit ?? 0} prefix="₫" color={(overview?.grossProfit ?? 0) >= 0 ? '#52c41a' : '#ff4d4f'} />
          </Col>
          <Col xs={24} sm={12} lg={6}>
            <KpiCard title="Sản phẩm" value={overview?.totalProducts ?? 0} icon={<ShoppingOutlined />} />
          </Col>
          <Col xs={24} sm={12} lg={6}>
            <KpiCard title="Nhà cung cấp" value={overview?.totalSuppliers ?? 0} icon={<TeamOutlined />} />
          </Col>
          <Col xs={24} sm={12} lg={6}>
            <KpiCard title="Tồn kho (units)" value={overview?.totalPhysicalStock ?? 0} icon={<InboxOutlined />} />
          </Col>
          <Col xs={24} sm={12} lg={6}>
            <KpiCard title="Giao dịch xuất" value={overview?.stockOutTransactions ?? 0} icon={<SwapOutlined />} />
          </Col>
        </Row>
      </Spin>

      {/* Charts row */}
      <Row gutter={[12, 12]} style={{ marginBottom: 20 }}>
        <Col xs={24} lg={16}>
          <Card title="Doanh thu theo ngày" size="small">
            <ResponsiveContainer width="100%" height={260}>
              <LineChart data={revenueByDay} margin={{ top: 5, right: 20, left: 0, bottom: 5 }}>
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis dataKey="date" tick={{ fontSize: 11 }} tickFormatter={(v: string) => dayjs(v).format('DD/MM')} />
                <YAxis tick={{ fontSize: 11 }} tickFormatter={(v: number) => `${(v / 1e6).toFixed(0)}M`} />
                <Tooltip formatter={(v) => [formatVND(Number(v) || 0), 'Doanh thu']} labelFormatter={(l) => dayjs(l as string).format('DD/MM/YYYY')} />
                <Line type="monotone" dataKey="revenue" stroke="#1677ff" dot={false} strokeWidth={2} />
              </LineChart>
            </ResponsiveContainer>
          </Card>
        </Col>
        <Col xs={24} lg={8}>
          <Card title="Phân bổ theo nguồn" size="small">
            <ResponsiveContainer width="100%" height={260}>
              {revenueBySource.length > 0 ? (
                <PieChart>
                  <Pie data={revenueBySource} dataKey="revenue" nameKey="source" cx="50%" cy="50%" outerRadius={80}>
                    {revenueBySource.map((_, i) => <Cell key={i} fill={PIE_COLORS[i % PIE_COLORS.length]} />)}
                  </Pie>
                  <Legend formatter={(v: string) => SOURCE_LABELS[v] ?? v} />
                  <Tooltip formatter={(v, name) => [formatVND(Number(v) || 0), SOURCE_LABELS[String(name)] ?? String(name)]} />
                </PieChart>
              ) : (
                <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'center', height: '100%' }}>
                  <Text type="secondary">Không có dữ liệu</Text>
                </div>
              )}
            </ResponsiveContainer>
          </Card>
        </Col>
      </Row>

      {/* Top products chart */}
      <Card title="Top 10 sản phẩm bán chạy" size="small" style={{ marginBottom: 20 }}>
        <ResponsiveContainer width="100%" height={240}>
          <BarChart data={topProducts} layout="vertical" margin={{ top: 5, right: 30, left: 80, bottom: 5 }}>
            <CartesianGrid strokeDasharray="3 3" />
            <XAxis type="number" tick={{ fontSize: 11 }} tickFormatter={(v: number) => `${(v / 1e6).toFixed(0)}M`} />
            <YAxis type="category" dataKey="productCode" tick={{ fontSize: 11 }} width={75} />
            <Tooltip formatter={(v) => [formatVND(Number(v) || 0), 'Doanh thu']} />
            <Bar dataKey="totalRevenue" fill="#1677ff" radius={[0, 4, 4, 0]} />
          </BarChart>
        </ResponsiveContainer>
      </Card>

      {/* Product profit table */}
      <Card
        title="Bảng lợi nhuận theo sản phẩm"
        size="small"
        extra={<Button size="small" icon={<DownloadOutlined />} onClick={() => exportProfitCsv(profit?.items ?? [])}>Xuất CSV</Button>}
      >
        <Table<ProductProfitDto>
          columns={profitColumns}
          dataSource={profit?.items ?? []}
          rowKey="productId"
          size="small"
          loading={!profit}
          pagination={{
            current: profitPage,
            pageSize: 10,
            total: profit?.totalCount ?? 0,
            onChange: setProfitPage,
            showTotal: (t) => `${t} sản phẩm`,
          }}
          scroll={{ x: 'max-content' }}
        />
      </Card>
    </div>
  )
}
