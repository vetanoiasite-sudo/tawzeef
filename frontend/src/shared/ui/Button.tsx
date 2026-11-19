import clsx from 'clsx'
import { type ButtonHTMLAttributes, forwardRef } from 'react'

type ButtonVariant = 'primary' | 'secondary' | 'danger' | 'success' | 'ghost'

const VARIANT_CLASSES: Record<ButtonVariant, string> = {
  primary: 'bg-primary text-surface hover:bg-primary-dark',
  secondary: 'border border-surface-muted bg-surface text-text hover:border-primary/40 hover:text-primary',
  danger: 'border border-danger/40 text-danger hover:bg-danger/10',
  success: 'bg-success text-surface hover:bg-success/90',
  ghost: 'text-primary hover:bg-primary/10',
}

/** UI_UX_GUIDELINES.md §1.5 — the one button component, used everywhere instead of ad-hoc
 * Tailwind classes repeated per page. Min 44px touch target, visible focus ring, consistent
 * 150–200ms transition (the "Flat Design" style's own key-effects guidance). */
export const Button = forwardRef<HTMLButtonElement, ButtonHTMLAttributes<HTMLButtonElement> & { variant?: ButtonVariant }>(
  function Button({ variant = 'primary', className, disabled, ...props }, ref) {
    return (
      <button
        ref={ref}
        type={props.type ?? 'button'}
        disabled={disabled}
        className={clsx(
          'inline-flex min-h-11 items-center justify-center gap-1.5 rounded-md px-4 text-sm font-medium transition-colors duration-150',
          'focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-primary',
          'disabled:cursor-not-allowed disabled:opacity-60',
          VARIANT_CLASSES[variant],
          className,
        )}
        {...props}
      />
    )
  },
)
