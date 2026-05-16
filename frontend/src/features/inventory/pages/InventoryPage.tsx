import { useState } from 'react'
import { Button, Tag, Typography } from 'antd'
import { DownloadOutlined } from '@ant-design/icons'
import type { ColumnsType } from 'antd/es/table'
import DataTable from '@/shared/components/DataTable/DataTable'
import { useDataTable } from '@/shared/hooks/useDataTable'
import { formatVND } from '@/shared/lib/format'
import { useInventory } from '../api/useInventory'
import InventoryDetailDrawer from '../components/InventoryDetailDrawer'
import type { InventoryItemDto, InventoryQueryParams } from '../types'

const { Title } = Typography

function exportCsv(items: InventoryItemDto[]) {
  const header = ['Mã SP', 'Tên SP', 'Tồn vật lý', 'Đang giữ', 'Khả dụng', 'Giá bán', 'Giá vốn TB', 'Giá trị tồn']
  const rows = items.map((i) => [
    i.productCode, i.productName, i.physicalStock, i.reservedQuantity, i.availableStock,
    i.sellingPrice, i.avgCostPrice ?? '', i.estimatedValue ?? '',
  ])
  const csv = [header, ...rows].map((r) => r.join(',')).join('\n')
  const blob = new Blob(['﻿' + csv], { type: 'text/csv;charset=utf-8;' })
  const url = URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = url
  a.download = `inventory_${new Date().toISOString().slice(0, 10)}.csv`
  a.click()
  URL.revokeObjectURL(url)
}

export default function InventoryPage() {
  const { params, setPage, setSort, setSearch } = useDataTable<InventoryQueryParams>({ sortBy: 'availableStock', sortDirection: 'asc' })
  const [selectedProduct, setSelectedProduct] = useState<InventoryItemDto | null>(null)

  const { data, isLoading } = useInventory(params)
  const result = data?.data

  const columns: ColumnsType<InventoryItemDto> = [
    { title: 'Mã SP', dataIndex: 'productCode', key: 'productCode', width: 110, sorter: true },
    { title: 'Tên sản phẩm', dataIndex: 'productName', key: 'productName', sorter: true, ellipsis: true },
    { title: 'Tồn vật lý', dataIndex: 'physicalStock', key: 'physicalStock', width: 110, align: 'right', sorter: true },
    { title: 'Đang giữ', dataIndex: 'reservedQuantity', key: 'reservedQuantity', width: 100, align: 'right',
      render: (v: number) => v > 0 ? <Tag color="orange">{v}</Tag> : <span style={{ color: '#999' }}>0</span> },
    { title: 'Khả dụng', dataIndex: 'availableStock', key: 'availableStock', width: 100, align: 'right', sorter: true,
      render: (v: number) => <strong style={{ color: v > 0 ? '#52c41a' : '#ff4d4f' }}>{v}</strong> },
    { title: 'Giá bán', dataIndex: 'sellingPrice', key: 'sellingPrice', width: 130, align: 'right', render: formatVND, sorter: true },
    { title: 'Giá vốn TB', dataIndex: 'avgCostPrice', key: 'avgCostPrice', width: 130, align: 'right',
      render: (v?: number) => v != null ? formatVND(v) : '—' },
    { title: 'Giá trị tồn', dataIndex: 'estimatedValue', key: 'estimatedValue', width: 140, align: 'right',
      render: (v?: number) => v != null ? formatVND(v) : '—', sorter: true },
  ]

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 16 }}>
        <Title level={4} style={{ margin: 0 }}>Tồn kho</Title>
        <Button icon={<DownloadOutlined />} onClick={() => exportCsv(result?.items ?? [])}>
          Xuất CSV
        </Button>
      </div>
      <DataTable<InventoryItemDto>
        columns={columns} dataSource={result?.items ?? []} loading={isLoading}
        total={result?.totalCount ?? 0} pageNumber={params.pageNumber} pageSize={params.pageSize}
        search={params.search} searchPlaceholder="Tìm theo mã hoặc tên sản phẩm..."
        rowKey="productId"
        onRowClick={(record) => setSelectedProduct(record)}
        onParamsChange={({ pageNumber, pageSize, sortBy, sortDirection, search }) => {
          if (pageNumber || pageSize) setPage(pageNumber ?? params.pageNumber, pageSize ?? params.pageSize)
          if (sortBy !== undefined) setSort(sortBy, sortDirection ?? 'asc')
          if (search !== undefined) setSearch(search)
        }}
      />
      <InventoryDetailDrawer product={selectedProduct} onClose={() => setSelectedProduct(null)} />
    </div>
  )
}
