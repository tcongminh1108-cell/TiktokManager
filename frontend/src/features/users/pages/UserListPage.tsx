import { useState } from 'react'
import { Button, Popconfirm, Select, Space, Tag, Typography } from 'antd'
import { PlusOutlined, EditOutlined, DeleteOutlined, KeyOutlined, CheckCircleOutlined, StopOutlined } from '@ant-design/icons'
import type { ColumnsType } from 'antd/es/table'
import DataTable from '@/shared/components/DataTable/DataTable'
import { useDataTable } from '@/shared/hooks/useDataTable'
import { formatDate } from '@/shared/lib/format'
import { useUsers, useUserMutations } from '../api/useUsers'
import UserFormModal from '../components/UserFormModal'
import ChangePasswordModal from '../components/ChangePasswordModal'
import type { UserDto } from '../types'
import type { UserRole } from '@/features/auth/types'

const { Title } = Typography

const ROLE_TAG: Record<string, { color: string; label: string }> = {
  Admin: { color: 'red', label: 'Admin' },
  Manager: { color: 'blue', label: 'Manager' },
  Staff: { color: 'default', label: 'Staff' },
}

interface ExtraFilters { role?: UserRole; isActive?: boolean }

export default function UserListPage() {
  const { params, setPage, setSort, setSearch, setFilters } = useDataTable<ExtraFilters>({ sortBy: 'createdAt', sortDirection: 'desc' })
  const [modalOpen, setModalOpen] = useState(false)
  const [pwModalOpen, setPwModalOpen] = useState(false)
  const [editing, setEditing] = useState<UserDto | null>(null)
  const [pwTarget, setPwTarget] = useState<UserDto | null>(null)

  const { data, isLoading } = useUsers(params)
  const { activate, deactivate, remove } = useUserMutations()
  const result = data?.data

  const columns: ColumnsType<UserDto> = [
    { title: 'Email', dataIndex: 'email', key: 'email', sorter: true, ellipsis: true },
    { title: 'Họ tên', dataIndex: 'fullName', key: 'fullName', sorter: true },
    { title: 'Vai trò', dataIndex: 'role', key: 'role', width: 100,
      render: (r: string) => <Tag color={ROLE_TAG[r]?.color}>{ROLE_TAG[r]?.label ?? r}</Tag> },
    { title: 'Trạng thái', dataIndex: 'isActive', key: 'isActive', width: 110,
      render: (v: boolean) => <Tag color={v ? 'success' : 'error'}>{v ? 'Hoạt động' : 'Vô hiệu'}</Tag> },
    { title: 'Ngày tạo', dataIndex: 'createdAt', key: 'createdAt', width: 110, render: formatDate, sorter: true },
    {
      title: 'Thao tác', key: 'actions', width: 150, fixed: 'right',
      render: (_: unknown, record: UserDto) => (
        <Space size={4}>
          <Button size="small" icon={<EditOutlined />} onClick={() => { setEditing(record); setModalOpen(true) }} />
          <Button size="small" icon={<KeyOutlined />} onClick={() => { setPwTarget(record); setPwModalOpen(true) }} title="Đổi mật khẩu" />
          {record.isActive ? (
            <Popconfirm title="Vô hiệu hóa người dùng này?" onConfirm={() => deactivate.mutate(record.id)} okText="Vô hiệu" cancelText="Hủy">
              <Button size="small" icon={<StopOutlined />} />
            </Popconfirm>
          ) : (
            <Popconfirm title="Kích hoạt người dùng này?" onConfirm={() => activate.mutate(record.id)} okText="Kích hoạt" cancelText="Hủy">
              <Button size="small" icon={<CheckCircleOutlined />} type="primary" ghost />
            </Popconfirm>
          )}
          <Popconfirm title="Xóa người dùng này?" onConfirm={() => remove.mutate(record.id)} okText="Xóa" cancelText="Hủy">
            <Button size="small" danger icon={<DeleteOutlined />} />
          </Popconfirm>
        </Space>
      ),
    },
  ]

  const roleFilter = (
    <Select
      placeholder="Lọc vai trò"
      allowClear
      style={{ width: 140 }}
      options={[
        { label: 'Admin', value: 'Admin' },
        { label: 'Manager', value: 'Manager' },
        { label: 'Staff', value: 'Staff' },
      ]}
      onChange={(v: UserRole | undefined) => setFilters({ role: v })}
    />
  )

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 16 }}>
        <Title level={4} style={{ margin: 0 }}>Quản lý người dùng</Title>
        <Button type="primary" icon={<PlusOutlined />} onClick={() => { setEditing(null); setModalOpen(true) }}>
          Thêm người dùng
        </Button>
      </div>
      <DataTable<UserDto>
        columns={columns} dataSource={result?.items ?? []} loading={isLoading}
        total={result?.totalCount ?? 0} pageNumber={params.pageNumber} pageSize={params.pageSize}
        search={params.search} searchPlaceholder="Tìm theo email hoặc họ tên..."
        extraFilters={roleFilter}
        onParamsChange={({ pageNumber, pageSize, sortBy, sortDirection, search }) => {
          if (pageNumber || pageSize) setPage(pageNumber ?? params.pageNumber, pageSize ?? params.pageSize)
          if (sortBy !== undefined) setSort(sortBy, sortDirection ?? 'asc')
          if (search !== undefined) setSearch(search)
        }}
      />
      <UserFormModal open={modalOpen} editing={editing} onClose={() => { setModalOpen(false); setEditing(null) }} />
      <ChangePasswordModal open={pwModalOpen} user={pwTarget} onClose={() => { setPwModalOpen(false); setPwTarget(null) }} />
    </div>
  )
}
