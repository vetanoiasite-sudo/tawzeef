import { env } from '@/shared/config/env'
import { ApiError, type ApiEnvelope, type ApiErrorKind } from './types'

type AuthTokenProvider = () => string | null
type UnauthorizedHandler = () => void

// Indirection instead of importing shared/auth directly, so this module (the one network entry
// point — BACKEND_GUIDELINES.md §12.2) never has to know the session store's shape.
let getAuthToken: AuthTokenProvider = () => null
let onUnauthorized: UnauthorizedHandler = () => {}

export function configureHttpClient(options: {
  getAuthToken?: AuthTokenProvider
  onUnauthorized?: UnauthorizedHandler
}) {
  if (options.getAuthToken) getAuthToken = options.getAuthToken
  if (options.onUnauthorized) onUnauthorized = options.onUnauthorized
}

type RequestOptions = {
  query?: Record<string, string | number | boolean | undefined>
  signal?: AbortSignal
}

function buildUrl(path: string, query?: RequestOptions['query']): string {
  const url = new URL(
    path.replace(/^\//, ''),
    env.apiBaseUrl.endsWith('/') ? env.apiBaseUrl : `${env.apiBaseUrl}/`,
  )
  if (query) {
    for (const [key, value] of Object.entries(query)) {
      if (value !== undefined) url.searchParams.set(key, String(value))
    }
  }
  return url.toString()
}

function errorKindFor(status: number): ApiErrorKind {
  switch (status) {
    case 400:
      return 'validation'
    case 401:
      return 'unauthorized'
    case 403:
      return 'forbidden'
    case 404:
      return 'notFound'
    case 409:
      return 'conflict'
    default:
      return 'unknown'
  }
}

async function request<T>(
  method: string,
  path: string,
  body?: unknown,
  options?: RequestOptions,
): Promise<T> {
  const headers: HeadersInit = { Accept: 'application/json' }
  const token = getAuthToken()
  if (token) headers['Authorization'] = `Bearer ${token}`

  const isFormData = body instanceof FormData
  if (body !== undefined && !isFormData) headers['Content-Type'] = 'application/json'

  let response: Response
  try {
    response = await fetch(buildUrl(path, options?.query), {
      method,
      headers,
      body: body === undefined ? undefined : isFormData ? body : JSON.stringify(body),
      signal: options?.signal,
    })
  } catch {
    throw new ApiError('network', ['Could not reach the server.'])
  }

  if (response.status === 401) {
    onUnauthorized()
    throw new ApiError('unauthorized', ['Your session has expired.'])
  }

  const envelope = (await response.json().catch(() => null)) as ApiEnvelope<T> | null

  if (!response.ok || !envelope?.isSuccess) {
    throw new ApiError(
      errorKindFor(response.status),
      envelope?.errorMessages ?? ['Something went wrong.'],
    )
  }

  return envelope.data as T
}

export const httpClient = {
  get: <T>(path: string, options?: RequestOptions) => request<T>('GET', path, undefined, options),
  post: <T>(path: string, body?: unknown, options?: RequestOptions) =>
    request<T>('POST', path, body, options),
  put: <T>(path: string, body?: unknown, options?: RequestOptions) =>
    request<T>('PUT', path, body, options),
  delete: <T>(path: string, options?: RequestOptions) =>
    request<T>('DELETE', path, undefined, options),
}
