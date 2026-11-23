import { httpClient } from '@/shared/api/httpClient'
import type { JobAdFormValues } from '../schemas/jobAd.schema'
import type {
  ApplicantListItem,
  ApplicantsFilter,
  JobAdDetail,
  JobAdListItem,
  JobAdQueryFilter,
} from '../types/employerJobAds.types'

function toRequestDto(values: JobAdFormValues) {
  return {
    title: values.title,
    description: values.description || null,
    experienceRequirement: values.experienceRequirement || null,
    workingHours: values.workingHours || null,
    address: values.address || null,
    category: values.category || null,
    city: values.city || null,
    employmentType: values.employmentType || null,
    salaryMin: values.salaryMin === '' ? null : values.salaryMin,
    salaryMax: values.salaryMax === '' ? null : values.salaryMax,
    vacancyCount: values.vacancyCount,
  }
}

export const employerJobAdsApi = {
  listMine: (filter: JobAdQueryFilter) =>
    httpClient.getPaged<JobAdListItem[]>('/api/employer/job-advertisements', {
      status: filter.status,
      pageIndex: filter.pageIndex,
      pageSize: filter.pageSize,
    }),
  getById: (id: string) => httpClient.get<JobAdDetail>(`/api/employer/job-advertisements/${id}`),
  create: (values: JobAdFormValues) =>
    httpClient.post<JobAdDetail>('/api/employer/job-advertisements', toRequestDto(values)),
  update: (id: string, values: JobAdFormValues) =>
    httpClient.put<JobAdDetail>(`/api/employer/job-advertisements/${id}`, toRequestDto(values)),
  submit: (id: string) =>
    httpClient.post<JobAdDetail>(`/api/employer/job-advertisements/${id}/submit`),
  close: (id: string) =>
    httpClient.post<JobAdDetail>(`/api/employer/job-advertisements/${id}/close`),
  renew: (id: string) =>
    httpClient.post<JobAdDetail>(`/api/employer/job-advertisements/${id}/renew`),
  listApplicants: (adId: string, filter: ApplicantsFilter) =>
    httpClient.getPaged<ApplicantListItem[]>(
      `/api/employer/job-advertisements/${adId}/applicants`,
      {
        pageIndex: filter.pageIndex,
        pageSize: filter.pageSize,
      },
    ),
  updateOutcome: ({
    applicationId,
    outcome,
    notes,
  }: {
    applicationId: string
    outcome: string
    notes?: string
  }) =>
    httpClient.post<void>(`/api/employer/applications/${applicationId}/outcome`, {
      outcome,
      notes,
    }),
  downloadCv: (applicationId: string) =>
    httpClient.getBlob(`/api/employer/applications/${applicationId}/cv`),
}
