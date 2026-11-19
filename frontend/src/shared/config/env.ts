import { z } from 'zod'

const schema = z.object({
  apiBaseUrl: z.url(),
})

function readEnv() {
  // "same-origin" lets a production build (served from the .NET backend's wwwroot) call the API
  // on whatever host/port it was actually deployed to, instead of baking in a fixed URL at build time.
  const rawBaseUrl = import.meta.env.VITE_API_BASE_URL
  const apiBaseUrl = rawBaseUrl === 'same-origin' ? window.location.origin : rawBaseUrl

  const parsed = schema.safeParse({ apiBaseUrl })

  if (!parsed.success) {
    // Fail fast (PROJECT_ARCHITECTURE_FRONTEND.md §20) — a misconfigured app should refuse to
    // start, not fail confusingly on the first network call.
    throw new Error(`Invalid environment configuration: ${parsed.error.message}`)
  }

  return parsed.data
}

export const env = readEnv()
