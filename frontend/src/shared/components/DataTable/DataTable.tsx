import { Input, Table } from 'antd'
import { SearchOutlined } from '@ant-design/icons'
import type { TableProps, TablePaginationConfig } from 'antd'
import type { SorterResult, FilterValue } from 'antd/es/table/interface'
import type { ReactNode } from 'react'

interface DataTableProps<T extends object> {
  columns: TableProps<T>['columns']
  dataSource: T[]
  loading?: boolean
  total: number
  pageNumber: number
  pageSize: number
  search?: string
  searchPlaceholder?: string
  extraFilters?: ReactNode
  rowKey?: string | ((record: T) => string)
  onParamsChange: (params: {
    pageNumber?: number
    pageSize?: number
    sortBy?: string
    sortDirection?: 'asc' | 'desc'
    search?: string
  }) => void
  onRowClick?: (record: T) => void
}

export default function DataTable<T extends object>({
  columns,
  dataSource,
  loading,
  total,
  pageNumber,
  pageSize,
  search,
  searchPlaceholder = 'Tìm kiếm...',
  extraFilters,
  rowKey = 'id',
  onParamsChange,
  onRowClick,
}: DataTableProps<T>) {
  const handleTableChange = (
    pagination: TablePaginationConfig,
    _filters: Record<string, FilterValue | null>,
    sorter: SorterResult<T> | SorterResult<T>[],
  ) => {
    const s = Array.isArray(sorter) ? sorter[0] : sorter
    onParamsChange({
      pageNumber: pagination.current,
      pageSize: pagination.pageSize,
      sortBy: s.field as string | undefined,
      sortDirection: s.order === 'descend' ? 'desc' : 'asc',
    })
  }

  return (
    <div>
      <div style={{ display: 'flex', gap: 8, marginBottom: 16, flexWrap: 'wrap' }}>
        <Input
          prefix={<SearchOutlined />}
          placeholder={searchPlaceholder}
          value={search}
          onChange={(e) => onParamsChange({ search: e.target.value, pageNumber: 1 })}
          allowClear
          style={{ width: 280 }}
        />
        {extraFilters}
      </div>

      <Table<T>
        columns={columns}
        dataSource={dataSource}
        loading={loading}
        rowKey={rowKey}
        onChange={handleTableChange}
        onRow={onRowClick ? (record) => ({ onClick: () => onRowClick(record), style: { cursor: 'pointer' } }) : undefined}
        pagination={{
          current: pageNumber,
          pageSize,
          total,
          showSizeChanger: true,
          showTotal: (t) => `Tổng ${t} bản ghi`,
          pageSizeOptions: ['10', '20', '50', '100'],
        }}
        scroll={{ x: 'max-content' }}
      />
    </div>
  )
}
