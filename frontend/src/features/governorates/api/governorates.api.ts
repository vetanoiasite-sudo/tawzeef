import { httpClient } from '@/shared/api/httpClient'
import type { Governorate } from '../types/governorates.types'

export const governoratesApi = {
  // Public, anonymous endpoint (api/governorates) — every "pick a city/governorate" field in the
  // app reads from here instead of a frontend-hardcoded list.
  list: () => httpClient.get<Governorate[]>('/api/governorates'),
}
