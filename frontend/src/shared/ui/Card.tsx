import clsx from 'clsx'
import type { HTMLAttributes } from 'react'

/** The one "surface panel" container — flat style, no shadow (UI_UX_GUIDELINES.md's key-effects:
 * "no gradients/shadows"), a hairline border instead for separation from the muted page background. */
export function Card({ className, ...props }: HTMLAttributes<HTMLDivElement>) {
  return <div className={clsx('rounded-lg border border-surface-muted bg-surface p-5', className)} {...props} />
}
