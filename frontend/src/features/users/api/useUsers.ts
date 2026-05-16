import { App } from 'antd'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { extractApiError } from '@/shared/lib/errors'
import { userApi } from './userApi'
import type { ChangePasswordRequest, CreateUserRequest, UpdateUserRequest, UserQueryParams } from '../types'

const QUERY_KEY = 'users'

export function useUsers(params: UserQueryParams) {
  return useQuery({
    queryKey: [QUERY_KEY, params],
    queryFn: () => userApi.list(params),
  })
}

export function useUserMutations() {
  const qc = useQueryClient()
  const { message } = App.useApp()
  const invalidate = () => qc.invalidateQueries({ queryKey: [QUERY_KEY] })

  const create = useMutation({
    mutationFn: (data: CreateUserRequest) => userApi.create(data),
    onSuccess: () => { invalidate(); message.success('Tạo người dùng thành công') },
    onError: (e) => message.error(extractApiError(e)),
  })

  const update = useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateUserRequest }) => userApi.update(id, data),
    onSuccess: () => { invalidate(); message.success('Cập nhật thành công') },
    onError: (e) => message.error(extractApiError(e)),
  })

  const changePassword = useMutation({
    mutationFn: ({ id, data }: { id: string; data: ChangePasswordRequest }) => userApi.changePassword(id, data),
    onSuccess: () => message.success('Đổi mật khẩu thành công'),
    onError: (e) => message.error(extractApiError(e)),
  })

  const activate = useMutation({
    mutationFn: (id: string) => userApi.activate(id),
    onSuccess: () => { invalidate(); message.success('Đã kích hoạt người dùng') },
    onError: (e) => message.error(extractApiError(e)),
  })

  const deactivate = useMutation({
    mutationFn: (id: string) => userApi.deactivate(id),
    onSuccess: () => { invalidate(); message.success('Đã vô hiệu hóa người dùng') },
    onError: (e) => message.error(extractApiError(e)),
  })

  const remove = useMutation({
    mutationFn: (id: string) => userApi.delete(id),
    onSuccess: () => { invalidate(); message.success('Đã xóa người dùng') },
    onError: (e) => message.error(extractApiError(e)),
  })

  return { create, update, changePassword, activate, deactivate, remove }
}
