import clsx from 'clsx'
import { forwardRef, type InputHTMLAttributes, type ReactNode, type TextareaHTMLAttributes } from 'react'

/** Figma Text input (component 150:3768 et al) — 48px tall, 8px radius, 1px
 * border, brand border on focus, negative token on error. */
const fieldClasses =
  'w-full rounded-200 border border-border-primary bg-surface px-4 text-[16px] text-content-primary outline-none transition-colors duration-150 placeholder:text-content-tertiary hover:border-border-inactive focus:border-border-active focus:ring-2 focus:ring-brand/20 disabled:cursor-not-allowed disabled:bg-surface-muted disabled:text-content-tertiary'

const inputFieldClasses = clsx(fieldClasses, 'h-12')

type TextInputProps = InputHTMLAttributes<HTMLInputElement> & {
  label?: string
  helperText?: string
  error?: string
  startIcon?: ReactNode
  endIcon?: ReactNode
}

export const TextInput = forwardRef<HTMLInputElement, TextInputProps>(function TextInput(
  { label, helperText, error, startIcon, endIcon, id, className, ...props },
  ref,
) {
  const hasIcon = Boolean(startIcon || endIcon)
  return (
    <div className="flex flex-col gap-1.5">
      {label && (
        <label htmlFor={id} className="text-[14px] font-bold text-content-primary">
          {label}
        </label>
      )}
      <div className={clsx('relative', hasIcon && 'flex items-center')}>
        {startIcon && (
          <span className="pointer-events-none absolute inset-y-0 start-3 flex items-center text-content-tertiary">
            {startIcon}
          </span>
        )}
        <input
          ref={ref}
          id={id}
          aria-invalid={error ? true : undefined}
          className={clsx(
            inputFieldClasses,
            startIcon && 'ps-10',
            endIcon && 'pe-10',
            error && 'border-danger focus:border-danger focus:ring-danger/20',
            className,
          )}
          {...props}
        />
        {endIcon && (
          <span className="absolute inset-y-0 end-3 flex items-center text-content-tertiary">{endIcon}</span>
        )}
      </div>
      {error ? (
        <p className="text-[12px] font-medium text-danger">{error}</p>
      ) : helperText ? (
        <p className="text-[12px] text-content-tertiary">{helperText}</p>
      ) : null}
    </div>
  )
})

type TextAreaProps = TextareaHTMLAttributes<HTMLTextAreaElement> & {
  label?: string
  helperText?: string
  error?: string
}

export const TextArea = forwardRef<HTMLTextAreaElement, TextAreaProps>(function TextArea(
  { label, helperText, error, id, className, rows = 4, ...props },
  ref,
) {
  return (
    <div className="flex flex-col gap-1.5">
      {label && (
        <label htmlFor={id} className="text-[14px] font-bold text-content-primary">
          {label}
        </label>
      )}
      <textarea
        ref={ref}
        id={id}
        rows={rows}
        aria-invalid={error ? true : undefined}
        className={clsx(
          fieldClasses,
          'resize-y py-3 leading-6',
          error && 'border-danger focus:border-danger focus:ring-danger/20',
          className,
        )}
        {...props}
      />
      {error ? (
        <p className="text-[12px] font-medium text-danger">{error}</p>
      ) : helperText ? (
        <p className="text-[12px] text-content-tertiary">{helperText}</p>
      ) : null}
    </div>
  )
})
