import { useState } from 'react'
import { Button, Popconfirm, Space, Typography } from 'antd'
import { PlusOutlined, EditOutlined, DeleteOutlined } from '@ant-design/icons'
import type { ColumnsType } from 'antd/es/table'
import DataTable from '@/shared/components/DataTable/DataTable'
import { useDataTable } from '@/shared/hooks/useDataTable'
import { useAuthStore } from '@/features/auth/store/useAuthStore'
import { formatDate } from '@/shared/lib/format'
import { useSuppliers, useSupplierMutations } from '../api/useSuppliers'
import SupplierFormModal from '../components/SupplierFormModal'
import type { SupplierDto } from '../types'

const { Title } = Typography

export default function SupplierListPage() {
  const { params, setPage, setSort, setSearch } = useDataTable({ sortBy: 'name' })
  const [modalOpen, setModalOpen] = useState(false)
  const [editing, setEditing] = useState<SupplierDto | null>(null)
  const { hasRole } = useAuthStore()
  const canEdit = hasRole('Manager')

  const { data, isLoading } = useSuppliers(params)
  const { remove } = useSupplierMutations()
  const result = data?.data

  const columns: ColumnsType<SupplierDto> = [
    { title: 'Mã', dataIndex: 'code', key: 'code', sorter: true, width: 110 },
    { title: 'Tên nhà cung cấp', dataIndex: 'name', key: 'name', sorter: true },
    { title: 'SĐT', dataIndex: 'phone', key: 'phone', width: 130 },
    { title: 'Email', dataIndex: 'email', key: 'email', width: 200 },
    { title: 'Địa chỉ', dataIndex: 'address', key: 'address', ellipsis: true },
    { title: 'Ngày tạo', dataIndex: 'createdAt', key: 'createdAt', width: 120, render: formatDate },
    {
      title: 'Thao tác', key: 'actions', width: 100, fixed: 'right',
      render: (_: unknown, record: SupplierDto) => (
        <Space size={4}>
          {canEdit && (
            <>
              <Button size="small" icon={<EditOutlined />} onClick={() => { setEditing(record); setModalOpen(true) }} />
              <Popconfirm title="Xóa nhà cung cấp này?" onConfirm={() => remove.mutate(record.id)} okText="Xóa" cancelText="Hủy">
                <Button size="small" danger icon={<DeleteOutlined />} />
              </Popconfirm>
            </>
          )}
        </Space>
      ),
    },
  ]

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 16 }}>
        <Title level={4} style={{ margin: 0 }}>Nhà cung cấp</Title>
        {canEdit && (
          <Button type="primary" icon={<PlusOutlined />} onClick={() => { setEditing(null); setModalOpen(true) }}>
            Thêm NCC
          </Button>
        )}
      </div>
      <DataTable<SupplierDto>
        columns={columns} dataSource={result?.items ?? []} loading={isLoading}
        total={result?.totalCount ?? 0} pageNumber={params.pageNumber} pageSize={params.pageSize}
        search={params.search} searchPlaceholder="Tìm theo mã hoặc tên..."
        onParamsChange={({ pageNumber, pageSize, sortBy, sortDirection, search }) => {
          if (pageNumber || pageSize) setPage(pageNumber ?? params.pageNumber, pageSize ?? params.pageSize)
          if (sortBy !== undefined) setSort(sortBy, sortDirection ?? 'asc')
          if (search !== undefined) setSearch(search)
        }}
      />
      <SupplierFormModal open={modalOpen} supplier={editing} onClose={() => { setModalOpen(false); setEditing(null) }} />
    </div>
  )
}
