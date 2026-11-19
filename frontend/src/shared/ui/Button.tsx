import clsx from 'clsx'
import { type ButtonHTMLAttributes, forwardRef } from 'react'

type ButtonVariant = 'primary' | 'secondary' | 'danger' | 'success' | 'ghost'
type ButtonSize = 'sm' | 'md' | 'lg'

// Minimal, flat Figma-aligned button styling — brand fill on primary, hairline
// borders elsewhere, no drop shadows, no glows. Label stays on a single line
// via whitespace-nowrap so long translations never wrap.
const VARIANT_CLASSES: Record<ButtonVariant, string> = {
  primary: 'bg-brand text-white hover:bg-brand-hover active:bg-brand-active',
  secondary:
    'border border-border-primary bg-surface text-content-primary hover:border-brand hover:text-brand',
  danger: 'border border-danger/40 bg-surface text-danger hover:bg-danger hover:text-white',
  success: 'bg-success text-white hover:bg-success/90',
  ghost: 'text-brand hover:bg-brand-soft',
}

const SIZE_CLASSES: Record<ButtonSize, string> = {
  sm: 'h-9 min-h-9 px-4 text-[14px]',
  md: 'h-12 min-h-12 px-6 text-[16px]',
  lg: 'h-12 min-h-12 px-6 text-[16px]',
}

type ButtonProps = ButtonHTMLAttributes<HTMLButtonElement> & {
  variant?: ButtonVariant
  size?: ButtonSize
  fullWidth?: boolean
}

export const Button = forwardRef<HTMLButtonElement, ButtonProps>(function Button(
  { variant = 'primary', size = 'md', fullWidth, className, disabled, ...props },
  ref,
) {
  return (
    <button
      ref={ref}
      type={props.type ?? 'button'}
      disabled={disabled}
      className={clsx(
        'inline-flex items-center justify-center gap-2 whitespace-nowrap rounded-200 font-bold tracking-[0.01em] transition-colors duration-150',
        'focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-brand',
        'disabled:cursor-not-allowed disabled:opacity-60',
        SIZE_CLASSES[size],
        VARIANT_CLASSES[variant],
        fullWidth && 'w-full',
        className,
      )}
      {...props}
    />
  )
})
