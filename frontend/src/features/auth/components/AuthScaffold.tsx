import { type ReactNode } from 'react'
import { TawzeefLogo } from '@/shared/ui/TawzeefLogo'

/**
 * Shared full-canvas chrome for every auth screen (login + registration flows).
 * Minimal, borrow-nothing-you-don't-need: a single hiring photo, one thin dark
 * overlay for text contrast, a white card held by a hairline border — no
 * gradients, no heavy shadows, no decorative glows. Keeps every auth screen
 * identical.
 */
export function AuthScaffold({
  children,
  maxWidth = 460,
}: {
  children: ReactNode
  maxWidth?: number
}) {
  return (
    <div className="relative min-h-screen w-full overflow-hidden bg-content-primary">
      <div
        aria-hidden
        className="absolute inset-0 bg-cover bg-center"
        style={{ backgroundImage: 'url(/images/auth-bg.jpg)' }}
      />
      <div
        aria-hidden
        className="absolute inset-0"
        style={{ backgroundColor: 'rgba(0, 16, 24, 0.55)' }}
      />

      <div className="relative z-10 flex min-h-screen flex-col items-center justify-center px-4 py-10 sm:py-14">
        <div className="mb-8 flex justify-center">
          <TawzeefLogo height={42} primary="#FFFFFF" cross="#FFFFFF" accent="#FFEA28" />
        </div>

        <div className="w-full" style={{ maxWidth }}>
          <div className="rounded-300 border border-border-primary bg-surface p-7 sm:p-8">
            {children}
          </div>
        </div>
      </div>
    </div>
  )
}
