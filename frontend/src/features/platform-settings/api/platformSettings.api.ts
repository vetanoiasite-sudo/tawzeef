import { httpClient } from '@/shared/api/httpClient'
import type { PlatformSetting } from '../types/platformSettings.types'

export const platformSettingsApi = {
  list: () => httpClient.get<PlatformSetting[]>('/api/platformsettings'),
  update: (key: string, value: string) =>
    httpClient.put<PlatformSetting>(`/api/platformsettings/${encodeURIComponent(key)}`, { value }),
  // Anonymous (PlatformSettingsController.GetSupportedCurrencies) — replaces the old hardcoded
  // SUPPORTED_CURRENCIES = ['EGP'] arrays in the two components that need a currency dropdown.
  getCurrencies: () => httpClient.get<string[]>('/api/platformsettings/currencies'),
}
