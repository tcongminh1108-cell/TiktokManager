import { useState } from 'react'
import { Button, Popconfirm, Space, Typography, DatePicker } from 'antd'
import { PlusOutlined, DeleteOutlined, EditOutlined } from '@ant-design/icons'
import type { ColumnsType } from 'antd/es/table'
import DataTable from '@/shared/components/DataTable/DataTable'
import { useDataTable } from '@/shared/hooks/useDataTable'
import { useAuthStore } from '@/features/auth/store/useAuthStore'
import { formatVND, formatDate } from '@/shared/lib/format'
import { useStockOuts, useStockOutMutations } from '../api/useStockOuts'
import StockOutFormModal from '../components/StockOutFormModal'
import type { StockOutDto } from '../types'

const { Title } = Typography
const { RangePicker } = DatePicker

interface ExtraFilters { dateFrom?: string; dateTo?: string }

export default function StockOutListPage() {
  const { params, setPage, setSort, setSearch, setFilters } = useDataTable<ExtraFilters>({ sortBy: 'transactionDate', sortDirection: 'desc' })
  const [modalOpen, setModalOpen] = useState(false)
  const [editing, setEditing] = useState<StockOutDto | null>(null)
  const { hasRole } = useAuthStore()
  const canEdit = hasRole('Manager')

  const { data, isLoading } = useStockOuts(params)
  const { remove } = useStockOutMutations()
  const result = data?.data

  const columns: ColumnsType<StockOutDto> = [
    { title: 'Sản phẩm', dataIndex: 'productName', key: 'productName', ellipsis: true,
      render: (name: string, r: StockOutDto) => <span>{r.productCode} — {name}</span> },
    { title: 'Số lượng', dataIndex: 'quantity', key: 'quantity', width: 100, align: 'right', sorter: true },
    { title: 'Đơn giá', dataIndex: 'unitPrice', key: 'unitPrice', width: 130, align: 'right', render: formatVND, sorter: true },
    { title: 'Tổng tiền', dataIndex: 'totalAmount', key: 'totalAmount', width: 140, align: 'right', render: formatVND, sorter: true },
    { title: 'Ngày xuất', dataIndex: 'transactionDate', key: 'transactionDate', width: 110, render: formatDate, sorter: true },
    { title: 'Ghi chú', dataIndex: 'note', key: 'note', ellipsis: true },
    {
      title: 'Thao tác', key: 'actions', width: 90, fixed: 'right',
      render: (_: unknown, record: StockOutDto) => (
        <Space size={4}>
          {canEdit && (
            <>
              <Button size="small" icon={<EditOutlined />} onClick={() => { setEditing(record); setModalOpen(true) }} />
              <Popconfirm title="Xóa phiếu xuất này?" onConfirm={() => remove.mutate(record.id)} okText="Xóa" cancelText="Hủy">
                <Button size="small" danger icon={<DeleteOutlined />} />
              </Popconfirm>
            </>
          )}
        </Space>
      ),
    },
  ]

  const dateFilter = (
    <RangePicker
      format="DD/MM/YYYY"
      placeholder={['Từ ngày', 'Đến ngày']}
      onChange={(dates) => setFilters({
        dateFrom: dates?.[0]?.startOf('day').toISOString(),
        dateTo: dates?.[1]?.endOf('day').toISOString(),
      })}
      style={{ width: 240 }}
    />
  )

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 16 }}>
        <Title level={4} style={{ margin: 0 }}>Phiếu xuất hàng</Title>
        {canEdit && (
          <Button type="primary" icon={<PlusOutlined />} onClick={() => { setEditing(null); setModalOpen(true) }}>
            Xuất hàng
          </Button>
        )}
      </div>
      <DataTable<StockOutDto>
        columns={columns} dataSource={result?.items ?? []} loading={isLoading}
        total={result?.totalCount ?? 0} pageNumber={params.pageNumber} pageSize={params.pageSize}
        search={params.search} searchPlaceholder="Tìm theo sản phẩm..."
        extraFilters={dateFilter}
        onParamsChange={({ pageNumber, pageSize, sortBy, sortDirection, search }) => {
          if (pageNumber || pageSize) setPage(pageNumber ?? params.pageNumber, pageSize ?? params.pageSize)
          if (sortBy !== undefined) setSort(sortBy, sortDirection ?? 'asc')
          if (search !== undefined) setSearch(search)
        }}
      />
      <StockOutFormModal open={modalOpen} editing={editing} onClose={() => { setModalOpen(false); setEditing(null) }} />
    </div>
  )
}
