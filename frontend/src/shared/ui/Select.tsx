import clsx from 'clsx'
import { forwardRef, type SelectHTMLAttributes } from 'react'

type SelectProps = SelectHTMLAttributes<HTMLSelectElement> & {
  label?: string
  helperText?: string
  error?: string
}

export const Select = forwardRef<HTMLSelectElement, SelectProps>(function Select(
  { label, helperText, error, id, className, children, ...props },
  ref,
) {
  return (
    <div className="flex flex-col gap-1.5">
      {label && (
        <label htmlFor={id} className="text-[14px] font-bold text-content-primary">
          {label}
        </label>
      )}
      <select
        ref={ref}
        id={id}
        dir="rtl"
        aria-invalid={error ? true : undefined}
        className={clsx(
          'h-12 w-full rounded-200 border bg-surface px-3 text-right text-[16px] text-content-primary outline-none transition-colors duration-150 focus:ring-2',
          error
            ? 'border-danger focus:border-danger focus:ring-danger/20'
            : 'border-border-primary hover:border-border-inactive focus:border-border-active focus:ring-brand/20',
          className,
        )}
        {...props}
      >
        {children}
      </select>
      {error ? (
        <p className="text-[12px] font-medium text-danger">{error}</p>
      ) : helperText ? (
        <p className="text-[12px] text-content-tertiary">{helperText}</p>
      ) : null}
    </div>
  )
})
