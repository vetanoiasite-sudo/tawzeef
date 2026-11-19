import clsx from 'clsx'
import { forwardRef, type SelectHTMLAttributes } from 'react'

type SelectProps = SelectHTMLAttributes<HTMLSelectElement> & { label?: string }

export const Select = forwardRef<HTMLSelectElement, SelectProps>(function Select(
  { label, id, className, children, ...props },
  ref,
) {
  return (
    <div className="flex flex-col gap-1.5">
      {label && (
        <label htmlFor={id} className="text-sm font-medium text-text">
          {label}
        </label>
      )}
      <select
        ref={ref}
        id={id}
        className={clsx(
          'w-full rounded-md border border-surface-muted bg-surface px-3 py-2.5 text-sm text-text outline-none transition-colors duration-150 focus:border-primary',
          className,
        )}
        {...props}
      >
        {children}
      </select>
    </div>
  )
})
