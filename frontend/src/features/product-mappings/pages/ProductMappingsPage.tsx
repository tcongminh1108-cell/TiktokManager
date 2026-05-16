import { useState } from 'react'
import {
  Button,
  Form,
  Input,
  Modal,
  Popconfirm,
  Select,
  Space,
  Table,
  Tag,
  Typography,
} from 'antd'
import { DeleteOutlined, PlusOutlined } from '@ant-design/icons'
import type { ColumnsType } from 'antd/es/table'
import { useTikTokShops } from '@/features/tiktok-shops/api/useTikTokShops'
import { useProducts } from '@/features/products/api/useProducts'
import { useProductMappingMutations, useProductMappings, useTikTokSkus } from '../api/useProductMappings'
import type { CreateProductMappingRequest, ProductMappingDto } from '../types'

const { Title } = Typography

export default function ProductMappingsPage() {
  const [page, setPage] = useState(1)
  const [modalOpen, setModalOpen] = useState(false)
  const [selectedConnection, setSelectedConnection] = useState<string>('')
  const [skuSearch, setSkuSearch] = useState('')
  const [form] = Form.useForm<CreateProductMappingRequest>()

  const { data: mappingData, isLoading } = useProductMappings({ pageNumber: page, pageSize: 20 })
  const { data: shopsData } = useTikTokShops()
  const { data: productsData } = useProducts({ pageNumber: 1, pageSize: 100 })
  const { data: skusData, isLoading: skusLoading } = useTikTokSkus(selectedConnection, skuSearch)
  const { create, remove } = useProductMappingMutations()

  const shops = shopsData?.data ?? []
  const products = productsData?.data?.items ?? []
  const skus = skusData?.data?.products ?? []
  const mappings = mappingData?.data?.items ?? []
  const total = mappingData?.data?.totalCount ?? 0

  const handleConnectionChange = (val: string) => {
    setSelectedConnection(val)
    form.setFieldValue('tikTokSkuId', undefined)
    form.setFieldValue('tikTokProductId', undefined)
    form.setFieldValue('tikTokSkuName', undefined)
  }

  const handleSkuChange = (skuId: string) => {
    const sku = skus.find((s) => s.skuId === skuId)
    if (sku) {
      form.setFieldsValue({
        tikTokProductId: sku.productId,
        tikTokSkuName: sku.skuName,
      })
    }
  }

  const handleSubmit = async (values: CreateProductMappingRequest) => {
    await create.mutateAsync(values)
    setModalOpen(false)
    form.resetFields()
  }

  const columns: ColumnsType<ProductMappingDto> = [
    {
      title: 'Sản phẩm nội bộ',
      render: (_, r) => (
        <Space direction="vertical" size={0}>
          <span style={{ fontWeight: 500 }}>{r.productName}</span>
          <span style={{ fontSize: 12, color: '#888' }}>{r.productCode}</span>
        </Space>
      ),
    },
    {
      title: 'Shop',
      dataIndex: 'shopName',
      render: (s) => <Tag color="blue">{s}</Tag>,
    },
    {
      title: 'TikTok SKU',
      render: (_, r) => (
        <Space direction="vertical" size={0}>
          <span style={{ fontWeight: 500 }}>{r.tikTokSkuName}</span>
          <span style={{ fontSize: 12, color: '#888' }}>ID: {r.tikTokSkuId}</span>
        </Space>
      ),
    },
    {
      title: 'Warehouse',
      dataIndex: 'warehouseId',
      render: (v) => v ?? '—',
    },
    {
      title: 'Thao tác',
      key: 'action',
      width: 80,
      render: (_, r) => (
        <Popconfirm title="Xóa mapping này?" onConfirm={() => remove.mutate(r.id)}>
          <Button size="small" danger icon={<DeleteOutlined />} />
        </Popconfirm>
      ),
    },
  ]

  return (
    <Space direction="vertical" style={{ width: '100%' }} size="large">
      <Space style={{ justifyContent: 'space-between', width: '100%' }}>
        <Title level={4} style={{ margin: 0 }}>Product Mappings</Title>
        <Button
          type="primary"
          icon={<PlusOutlined />}
          onClick={() => setModalOpen(true)}
        >
          Thêm Mapping
        </Button>
      </Space>

      <Table
        rowKey="id"
        loading={isLoading}
        dataSource={mappings}
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

      <Modal
        title="Thêm Product Mapping"
        open={modalOpen}
        onOk={() => form.submit()}
        onCancel={() => { setModalOpen(false); form.resetFields() }}
        confirmLoading={create.isPending}
        width={560}
      >
        <Form form={form} layout="vertical" onFinish={handleSubmit}>
          <Form.Item name="productId" label="Sản phẩm nội bộ" rules={[{ required: true }]}>
            <Select
              showSearch
              optionFilterProp="label"
              options={products.map((p) => ({ value: p.id, label: `${p.name} (${p.code})` }))}
            />
          </Form.Item>
          <Form.Item name="connectionId" label="TikTok Shop" rules={[{ required: true }]}>
            <Select
              options={shops.map((s) => ({ value: s.id, label: s.shopName }))}
              onChange={handleConnectionChange}
            />
          </Form.Item>
          <Form.Item name="tikTokSkuId" label="TikTok SKU" rules={[{ required: true }]}>
            <Select
              showSearch
              loading={skusLoading}
              onSearch={setSkuSearch}
              filterOption={false}
              onChange={handleSkuChange}
              options={skus.map((s) => ({
                value: s.skuId,
                label: `${s.productName} — ${s.skuName}`,
              }))}
              disabled={!selectedConnection}
            />
          </Form.Item>
          <Form.Item name="tikTokProductId" hidden><Input /></Form.Item>
          <Form.Item name="tikTokSkuName" hidden><Input /></Form.Item>
          <Form.Item name="warehouseId" label="Warehouse ID (tuỳ chọn)">
            <Input placeholder="Để trống nếu không dùng warehouse cụ thể" />
          </Form.Item>
        </Form>
      </Modal>
    </Space>
  )
}
