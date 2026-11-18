import { create } from 'zustand'

export type SessionUser = {
  id: string
  fullName: string
  officeId: string | null
  isSuperAdmin: boolean
  /** Permission codes the UI gates on — "module.action", RBAC_PERMISSIONS.md §2. */
  permissions: string[]
}

type SessionState = {
  accessToken: string | null
  user: SessionUser | null
  setSession: (accessToken: string, user: SessionUser) => void
  clearSession: () => void
}

// In-memory only for now, deliberately — no persistence middleware. Once the login flow exists,
// decide then whether the backend issues an HttpOnly cookie (preferred,
// PROJECT_ARCHITECTURE_FRONTEND.md §18.2) or a client-stored token; don't default to localStorage
// just because it's the easy option.
export const useSessionStore = create<SessionState>((set) => ({
  accessToken: null,
  user: null,
  setSession: (accessToken, user) => set({ accessToken, user }),
  clearSession: () => set({ accessToken: null, user: null }),
}))
