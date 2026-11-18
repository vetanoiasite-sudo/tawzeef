import { configureHttpClient } from '@/shared/api/httpClient'
import { useSessionStore } from './sessionStore'

// Wires the session store into the one network entry point (BACKEND_GUIDELINES.md §12.2) without
// httpClient importing shared/auth directly. Call once, at app startup.
export function bootstrapAuth() {
  configureHttpClient({
    getAuthToken: () => useSessionStore.getState().accessToken,
    onUnauthorized: () => useSessionStore.getState().clearSession(),
  })
}
