import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { interviewsAndReportsApi } from './interviewsAndReports.api'
import { interviewsAndReportsKeys } from './interviewsAndReports.keys'
import type {
  CompleteHiringFollowUpValues,
  CreateEvaluationValues,
  CreateInterviewReportValues,
  CreateInterviewValues,
  HiringFollowUpQuery,
  UpdateInterviewValues,
} from '../types/interviewsAndReports.types'

// retry: false everywhere here — a 404 commonly means "not created yet", which the pages render as
// a create form rather than an error, so there is no point retrying it.

export function useInterview(applicationId: string) {
  return useQuery({
    queryKey: interviewsAndReportsKeys.interview(applicationId),
    queryFn: () => interviewsAndReportsApi.getInterview(applicationId),
    retry: false,
  })
}

export function useScheduleInterview(applicationId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (values: CreateInterviewValues) => interviewsAndReportsApi.scheduleInterview(applicationId, values),
    onSuccess: (data) => queryClient.setQueryData(interviewsAndReportsKeys.interview(applicationId), data),
  })
}

export function useUpdateInterview(applicationId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (values: UpdateInterviewValues) => interviewsAndReportsApi.updateInterview(applicationId, values),
    onSuccess: (data) => queryClient.setQueryData(interviewsAndReportsKeys.interview(applicationId), data),
  })
}

export function useInterviewReport(applicationId: string) {
  return useQuery({
    queryKey: interviewsAndReportsKeys.interviewReport(applicationId),
    queryFn: () => interviewsAndReportsApi.getInterviewReport(applicationId),
    retry: false,
  })
}

export function useCreateInterviewReport(applicationId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (values: CreateInterviewReportValues) => interviewsAndReportsApi.createInterviewReport(applicationId, values),
    onSuccess: (data) => queryClient.setQueryData(interviewsAndReportsKeys.interviewReport(applicationId), data),
  })
}

export function useEvaluation(applicationId: string) {
  return useQuery({
    queryKey: interviewsAndReportsKeys.evaluation(applicationId),
    queryFn: () => interviewsAndReportsApi.getEvaluation(applicationId),
    retry: false,
  })
}

export function useCreateEvaluation(applicationId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (values: CreateEvaluationValues) => interviewsAndReportsApi.createEvaluation(applicationId, values),
    onSuccess: (data) => queryClient.setQueryData(interviewsAndReportsKeys.evaluation(applicationId), data),
  })
}

export function useHiringFollowUp(applicationId: string) {
  return useQuery({
    queryKey: interviewsAndReportsKeys.hiringFollowUp(applicationId),
    queryFn: () => interviewsAndReportsApi.getHiringFollowUp(applicationId),
    retry: false,
  })
}

export function useCreateHiringFollowUp(applicationId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (hiredAt?: string) => interviewsAndReportsApi.createHiringFollowUp(applicationId, hiredAt),
    onSuccess: (data) => queryClient.setQueryData(interviewsAndReportsKeys.hiringFollowUp(applicationId), data),
  })
}

export function useCompleteHiringFollowUp(applicationId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (values: CompleteHiringFollowUpValues) => interviewsAndReportsApi.completeHiringFollowUp(applicationId, values),
    onSuccess: (data) => {
      queryClient.setQueryData(interviewsAndReportsKeys.hiringFollowUp(applicationId), data)
      void queryClient.invalidateQueries({ queryKey: interviewsAndReportsKeys.all })
    },
  })
}

export function useDueFollowUps(query: HiringFollowUpQuery) {
  return useQuery({
    queryKey: interviewsAndReportsKeys.dueFollowUps(query),
    queryFn: () => interviewsAndReportsApi.listDueFollowUps(query),
  })
}
