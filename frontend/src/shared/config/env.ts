import { z } from 'zod'

const schema = z.object({
  apiBaseUrl: z.url(),
})

function readEnv() {
  const parsed = schema.safeParse({
    apiBaseUrl: import.meta.env.VITE_API_BASE_URL,
  })

  if (!parsed.success) {
    // Fail fast (PROJECT_ARCHITECTURE_FRONTEND.md §20) — a misconfigured app should refuse to
    // start, not fail confusingly on the first network call.
    throw new Error(`Invalid environment configuration: ${parsed.error.message}`)
  }

  return parsed.data
}

export const env = readEnv()
