/** UI_UX_GUIDELINES.md §3 — replaces a bare "..." everywhere a query is in flight. */
export function LoadingState({ label }: { label?: string }) {
  return (
    <div className="flex items-center gap-2 py-6 text-sm text-text-muted" role="status">
      <span className="size-3.5 animate-spin rounded-full border-2 border-surface-muted border-t-primary" aria-hidden="true" />
      {label ?? '...'}
    </div>
  )
}
