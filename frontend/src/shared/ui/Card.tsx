import clsx from 'clsx'
import type { HTMLAttributes } from 'react'

/** Surface panel — Figma: #FFFFFF bg, 12px radius (radius-300), 1px border, and
 * a soft brand-tinted shadow (shadow100). */
export function Card({ className, ...props }: HTMLAttributes<HTMLDivElement>) {
  return (
    <div
      className={clsx('rounded-300 border border-border-primary bg-surface p-6', className)}
      {...props}
    />
  )
}
