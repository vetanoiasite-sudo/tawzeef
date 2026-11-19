import { httpClient } from '@/shared/api/httpClient'
import type { InviteOfficeUserPayload, OfficeUser } from '../types/officeUser.types'

export const officeUserApi = {
  invite: (payload: InviteOfficeUserPayload) => httpClient.post<null>('/api/office-users/invite', payload),
  list: () => httpClient.get<OfficeUser[]>('/api/office-users'),
  updatePermissions: ({ membershipId, permissionCodes }: { membershipId: string; permissionCodes: string[] }) =>
    httpClient.put<OfficeUser>(`/api/office-users/${membershipId}/permissions`, { permissionCodes }),
  deactivate: (membershipId: string) => httpClient.delete<null>(`/api/office-users/${membershipId}`),
}
