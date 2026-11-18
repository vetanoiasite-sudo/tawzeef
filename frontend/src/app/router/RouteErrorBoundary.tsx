import { isRouteErrorResponse, useRouteError } from 'react-router-dom'

/** errorElement for route segments (PROJECT_ARCHITECTURE_FRONTEND.md §13.2). */
export function RouteErrorBoundary() {
  const error = useRouteError()
  const message = isRouteErrorResponse(error) ? error.statusText : 'Something went wrong.'

  return (
    <div className="flex min-h-[40vh] flex-col items-center justify-center gap-2 p-6 text-center">
      <p className="text-lg font-semibold text-text">حدث خطأ غير متوقع</p>
      <p className="text-sm text-text-muted">{message}</p>
    </div>
  )
}
