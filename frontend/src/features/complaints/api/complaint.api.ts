import { httpClient } from '@/shared/api/httpClient'
import type { Complaint, CreateComplaintPayload } from '../types/complaint.types'

export const complaintApi = {
  create: (payload: CreateComplaintPayload) => httpClient.post<Complaint>('/api/complaints', payload),
  listMine: (pageIndex: number, pageSize: number) =>
    httpClient.getPaged<Complaint[]>('/api/complaints/mine', { pageIndex, pageSize }),
}
