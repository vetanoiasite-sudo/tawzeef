import { QueryClient } from '@tanstack/react-query'
import { ApiError } from './types'

function shouldRetry(failureCount: number, error: unknown): boolean {
  if (failureCount >= 2) return false
  // Retry only network/5xx-shaped failures (BACKEND_GUIDELINES.md §12.4) — never retry a
  // validation/not-found/conflict/forbidden response, since retrying won't change the answer.
  if (error instanceof ApiError) return error.kind === 'network' || error.kind === 'unknown'
  return false
}

export const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 30_000,
      retry: shouldRetry,
    },
    mutations: {
      retry: false,
    },
  },
})
