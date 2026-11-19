import type { ReactNode } from 'react'

/** UI_UX_GUIDELINES.md §3 — "guide users when no content exists," never a bare empty div. */
export function EmptyState({ message, action }: { message: string; action?: ReactNode }) {
  return (
    <div className="flex flex-col items-center gap-3 rounded-lg border border-dashed border-surface-muted bg-surface px-6 py-10 text-center">
      <p className="text-sm text-text-muted">{message}</p>
      {action}
    </div>
  )
}
