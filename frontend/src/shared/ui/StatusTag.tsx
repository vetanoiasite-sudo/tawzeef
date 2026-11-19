import clsx from 'clsx'
import type { StatusTone } from '@/app/styles/theme'
import { resolveStatusTone, type StatusDomain } from './statusTags'

const TONE_CLASSES: Record<StatusTone, string> = {
  success: 'bg-success/10 text-success',
  warning: 'bg-warning/10 text-warning',
  danger: 'bg-danger/10 text-danger',
  neutral: 'bg-neutral/10 text-neutral',
  info: 'bg-info/10 text-info',
}

const DOT_CLASSES: Record<StatusTone, string> = {
  success: 'bg-success',
  warning: 'bg-warning',
  danger: 'bg-danger',
  neutral: 'bg-neutral',
  info: 'bg-info',
}

/**
 * UI_UX_GUIDELINES.md §3/§5 — the one place a status value becomes a color. Never color alone:
 * the translated label is always present too (an a11y requirement, not just a nicety).
 */
export function StatusTag({ domain, value, label }: { domain: StatusDomain; value: string; label: string }) {
  const tone = resolveStatusTone(domain, value)
  return (
    <span
      className={clsx(
        'inline-flex w-fit items-center gap-1.5 rounded-full px-2.5 py-1 text-xs font-medium whitespace-nowrap',
        TONE_CLASSES[tone],
      )}
    >
      <span className={clsx('size-1.5 shrink-0 rounded-full', DOT_CLASSES[tone])} aria-hidden="true" />
      {label}
    </span>
  )
}
