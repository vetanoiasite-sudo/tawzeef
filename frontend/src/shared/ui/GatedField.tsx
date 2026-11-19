/** UI_UX_GUIDELINES.md §7 — when the API has omitted a field entirely (not nulled, not present),
 * this is what renders in its place: a deliberate "locked" affordance explaining why, never a
 * silently missing value. */
export function GatedField({ reason }: { reason: string }) {
  return (
    <span className="inline-flex items-center gap-1.5 text-xs text-text-muted">
      <svg viewBox="0 0 16 16" fill="none" className="size-3.5 shrink-0" aria-hidden="true">
        <rect x="3" y="7" width="10" height="7" rx="1.5" stroke="currentColor" strokeWidth="1.3" />
        <path d="M5.5 7V4.8a2.5 2.5 0 0 1 5 0V7" stroke="currentColor" strokeWidth="1.3" strokeLinecap="round" />
      </svg>
      {reason}
    </span>
  )
}
