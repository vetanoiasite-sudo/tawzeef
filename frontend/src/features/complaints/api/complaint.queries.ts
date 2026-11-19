import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { complaintApi } from './complaint.api'
import { complaintKeys } from './complaint.keys'

export function useMyComplaints(pageIndex: number, pageSize: number) {
  return useQuery({
    queryKey: complaintKeys.mine(pageIndex, pageSize),
    queryFn: () => complaintApi.listMine(pageIndex, pageSize),
  })
}

export function useCreateComplaint() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: complaintApi.create,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['complaints', 'mine'] }),
  })
}
