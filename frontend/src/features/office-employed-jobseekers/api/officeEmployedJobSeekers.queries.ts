import { useQuery } from '@tanstack/react-query'
import { officeEmployedJobSeekersApi } from './officeEmployedJobSeekers.api'
import { officeEmployedJobSeekersKeys } from './officeEmployedJobSeekers.keys'
import type { EmployedJobSeekersQuery } from '../types/officeEmployedJobSeekers.types'

export function useOfficeEmployedJobSeekers(query: EmployedJobSeekersQuery) {
  return useQuery({
    queryKey: officeEmployedJobSeekersKeys.list(query),
    queryFn: () => officeEmployedJobSeekersApi.list(query),
  })
}
