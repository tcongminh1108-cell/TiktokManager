import { useState } from 'react'
import { Drawer, Descriptions, Table, Tag, Tabs, Empty, Spin, Badge } from 'antd'
import type { ColumnsType } from 'antd/es/table'
import { formatVND, formatDate, formatDateTime } from '@/shared/lib/format'
import { useInventoryDetail } from '../api/useInventory'
import type { ActiveReservationDto, InventoryItemDto, MovementHistoryDto } from '../types'

const TYPE_COLOR: Record<string, string> = {
  In: 'green', Out: 'red', ReturnIn: 'cyan', ReturnOut: 'orange', Adjustment: 'blue',
}
const SOURCE_LABEL: Record<string, string> = {
  Manual: 'Thủ công', TikTokOrder: 'TikTok', Adjustment: 'Điều chỉnh', Import: 'Import', TikTokReturn: 'TikTok Return',
}

interface Props {
  product: InventoryItemDto | null
  onClose: () => void
}

export default function InventoryDetailDrawer({ product, onClose }: Props) {
  const [page, setPage] = useState(1)
  const { data, isLoading } = useInventoryDetail(product?.productId ?? null, { pageNumber: page, pageSize: 10 })
  const detail = data?.data

  const movementColumns: ColumnsType<MovementHistoryDto> = [
    { title: 'Ngày', dataIndex: 'occurredAt', key: 'occurredAt', width: 130, render: formatDateTime },
    { title: 'Loại', dataIndex: 'type', key: 'type', width: 90, render: (t: string) => <Tag color={TYPE_COLOR[t] ?? 'default'}>{t}</Tag> },
    { title: 'Nguồn', dataIndex: 'source', key: 'source', width: 100, render: (s: string) => SOURCE_LABEL[s] ?? s },
    { title: 'SL', dataIndex: 'quantity', key: 'quantity', width: 70, align: 'right' },
    { title: 'Đơn giá', dataIndex: 'unitCost', key: 'unitCost', width: 120, align: 'right', render: formatVND },
    { title: 'Ghi chú', dataIndex: 'note', key: 'note', ellipsis: true },
  ]

  const reservationColumns: ColumnsType<ActiveReservationDto> = [
    { title: 'Đặt lúc', dataIndex: 'reservedAt', key: 'reservedAt', render: formatDateTime },
    { title: 'Hết hạn', dataIndex: 'expiresAt', key: 'expiresAt', render: formatDate },
    { title: 'SL', dataIndex: 'quantity', key: 'quantity', width: 70, align: 'right' },
    { title: 'Idempotency', dataIndex: 'idempotencyKey', key: 'idempotencyKey', ellipsis: true },
  ]

  const summary = detail?.summary ?? product

  return (
    <Drawer title={product ? `${product.productCode} — ${product.productName}` : ''} open={!!product} onClose={onClose} width={720} destroyOnClose>
      {isLoading && !detail ? (
        <div style={{ textAlign: 'center', paddingTop: 60 }}><Spin /></div>
      ) : (
        <>
          {summary && (
            <Descriptions bordered size="small" column={2} style={{ marginBottom: 24 }}>
              <Descriptions.Item label="Tồn vật lý"><strong>{summary.physicalStock}</strong></Descriptions.Item>
              <Descriptions.Item label="Đang giữ"><Badge count={summary.reservedQuantity} showZero color="orange" /></Descriptions.Item>
              <Descriptions.Item label="Khả dụng"><strong style={{ color: summary.availableStock > 0 ? '#52c41a' : '#ff4d4f' }}>{summary.availableStock}</strong></Descriptions.Item>
              <Descriptions.Item label="Giá bán">{formatVND(summary.sellingPrice)}</Descriptions.Item>
              {summary.avgCostPrice != null && <Descriptions.Item label="Giá vốn TB">{formatVND(summary.avgCostPrice)}</Descriptions.Item>}
              {summary.estimatedValue != null && <Descriptions.Item label="Giá trị tồn kho">{formatVND(summary.estimatedValue)}</Descriptions.Item>}
            </Descriptions>
          )}

          <Tabs items={[
            {
              key: 'movements',
              label: `Lịch sử biến động`,
              children: detail?.movementHistory.items.length ? (
                <Table<MovementHistoryDto>
                  columns={movementColumns}
                  dataSource={detail.movementHistory.items}
                  rowKey="id"
                  size="small"
                  loading={isLoading}
                  pagination={{
                    current: page,
                    pageSize: 10,
                    total: detail.movementHistory.totalCount,
                    onChange: setPage,
                    showTotal: (t) => `${t} bản ghi`,
                  }}
                />
              ) : <Empty description="Chưa có biến động" />,
            },
            {
              key: 'reservations',
              label: `Đặt chỗ (${detail?.activeReservations.length ?? 0})`,
              children: detail?.activeReservations.length ? (
                <Table<ActiveReservationDto>
                  columns={reservationColumns}
                  dataSource={detail.activeReservations}
                  rowKey="id"
                  size="small"
                  pagination={false}
                />
              ) : <Empty description="Không có reservation đang active" />,
            },
          ]} />
        </>
      )}
    </Drawer>
  )
}
