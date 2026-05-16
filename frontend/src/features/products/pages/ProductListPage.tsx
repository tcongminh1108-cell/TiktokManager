import { useState } from 'react'
import { Button, Popconfirm, Select, Space, Tag, Typography } from 'antd'
import { PlusOutlined, EditOutlined, DeleteOutlined, PoweroffOutlined } from '@ant-design/icons'
import type { ColumnsType } from 'antd/es/table'
import DataTable from '@/shared/components/DataTable/DataTable'
import { useDataTable } from '@/shared/hooks/useDataTable'
import { useAuthStore } from '@/features/auth/store/useAuthStore'
import { formatVND, formatDate } from '@/shared/lib/format'
import { useProducts, useProductMutations } from '../api/useProducts'
import ProductFormModal from '../components/ProductFormModal'
import type { ProductDto } from '../types'

const { Title } = Typography

export default function ProductListPage() {
  const { params, setPage, setSort, setSearch, setFilters } = useDataTable({ sortBy: 'name' })
  const [modalOpen, setModalOpen] = useState(false)
  const [editing, setEditing] = useState<ProductDto | null>(null)
  const { hasRole } = useAuthStore()
  const canEdit = hasRole('Manager')

  const { data, isLoading } = useProducts(params)
  const { remove, activate, deactivate } = useProductMutations()

  const result = data?.data

  const columns: ColumnsType<ProductDto> = [
    { title: 'Mã', dataIndex: 'code', key: 'code', sorter: true, width: 110 },
    { title: 'Tên sản phẩm', dataIndex: 'name', key: 'name', sorter: true },
    { title: 'Giá bán', dataIndex: 'sellingPrice', key: 'sellingPrice', sorter: true, width: 140, render: (v: number) => formatVND(v) },
    { title: 'Đơn vị', dataIndex: 'unit', key: 'unit', width: 80 },
    {
      title: 'Trạng thái', dataIndex: 'isActive', key: 'isActive', width: 110,
      render: (v: boolean) => <Tag color={v ? 'green' : 'red'}>{v ? 'Hoạt động' : 'Tạm dừng'}</Tag>,
    },
    { title: 'Ngày tạo', dataIndex: 'createdAt', key: 'createdAt', width: 120, render: formatDate },
    {
      title: 'Thao tác', key: 'actions', width: 140, fixed: 'right',
      render: (_: unknown, record: ProductDto) => (
        <Space size={4}>
          {canEdit && (
            <>
              <Button size="small" icon={<EditOutlined />} onClick={() => { setEditing(record); setModalOpen(true) }} />
              <Button
                size="small"
                icon={<PoweroffOutlined />}
                title={record.isActive ? 'Tạm dừng' : 'Kích hoạt'}
                onClick={() => record.isActive ? deactivate.mutate(record.id) : activate.mutate(record.id)}
              />
              <Popconfirm title="Xóa sản phẩm này?" onConfirm={() => remove.mutate(record.id)} okText="Xóa" cancelText="Hủy">
                <Button size="small" danger icon={<DeleteOutlined />} />
              </Popconfirm>
            </>
          )}
        </Space>
      ),
    },
  ]

  const extraFilters = (
    <Select
      placeholder="Trạng thái"
      allowClear
      style={{ width: 140 }}
      onChange={(v) => setFilters({ ...params, isActive: v } as never)}
      options={[{ value: true, label: 'Hoạt động' }, { value: false, label: 'Tạm dừng' }]}
    />
  )

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 16 }}>
        <Title level={4} style={{ margin: 0 }}>Sản phẩm</Title>
        {canEdit && (
          <Button type="primary" icon={<PlusOutlined />} onClick={() => { setEditing(null); setModalOpen(true) }}>
            Thêm sản phẩm
          </Button>
        )}
      </div>

      <DataTable<ProductDto>
        columns={columns}
        dataSource={result?.items ?? []}
        loading={isLoading}
        total={result?.totalCount ?? 0}
        pageNumber={params.pageNumber}
        pageSize={params.pageSize}
        search={params.search}
        searchPlaceholder="Tìm theo mã hoặc tên..."
        extraFilters={extraFilters}
        onParamsChange={({ pageNumber, pageSize, sortBy, sortDirection, search }) => {
          if (pageNumber !== undefined || pageSize !== undefined)
            setPage(pageNumber ?? params.pageNumber, pageSize ?? params.pageSize)
          if (sortBy !== undefined) setSort(sortBy, sortDirection ?? 'asc')
          if (search !== undefined) setSearch(search)
        }}
      />

      <ProductFormModal
        open={modalOpen}
        product={editing}
        onClose={() => { setModalOpen(false); setEditing(null) }}
      />
    </div>
  )
}
