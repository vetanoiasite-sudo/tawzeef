import clsx from 'clsx'
import type { ReactNode } from 'react'
import { useReveal } from '@/shared/hooks/useReveal'

/** Fades + slides a section up into place the first time it scrolls into view. Pure decoration —
 * content is already in the DOM and readable before the transition runs. */
export function Reveal({
  children,
  delayMs = 0,
  className,
}: {
  children: ReactNode
  delayMs?: number
  className?: string
}) {
  const { ref, visible } = useReveal<HTMLDivElement>()

  return (
    <div
      ref={ref}
      style={{ transitionDelay: visible ? `${delayMs}ms` : '0ms' }}
      className={clsx(
        'transition-[opacity,transform] duration-700 ease-out',
        visible ? 'translate-y-0 opacity-100' : 'translate-y-6 opacity-0',
        className,
      )}
    >
      {children}
    </div>
  )
}
