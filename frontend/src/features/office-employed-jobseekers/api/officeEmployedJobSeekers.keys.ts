import type { EmployedJobSeekersQuery } from '../types/officeEmployedJobSeekers.types'

export const officeEmployedJobSeekersKeys = {
  all: ['officeEmployedJobSeekers'] as const,
  list: (query: EmployedJobSeekersQuery) => [...officeEmployedJobSeekersKeys.all, 'list', query] as const,
}
