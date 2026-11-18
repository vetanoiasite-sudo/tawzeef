/** Mirrors the backend's ServiceResult envelope (BACKEND_GUIDELINES.md §10). */
export type ApiEnvelope<T> = {
  isSuccess: boolean
  error: 'None' | 'Validation' | 'NotFound' | 'Conflict' | 'Forbidden'
  errorMessages: string[]
  data: T | null
}

export type ApiErrorKind =
  'validation' | 'notFound' | 'conflict' | 'forbidden' | 'unauthorized' | 'network' | 'unknown'

/** One normalized error type for every failed request (BACKEND_GUIDELINES.md §9.4). */
export class ApiError extends Error {
  readonly kind: ApiErrorKind
  readonly messages: string[]

  constructor(kind: ApiErrorKind, messages: string[]) {
    super(messages[0] ?? kind)
    this.name = 'ApiError'
    this.kind = kind
    this.messages = messages
  }
}
