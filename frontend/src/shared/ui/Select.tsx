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
        <label htmlFor={id} className="text-[14px] font-bold text-content-primary">
          {label}
        </label>
      )}
      <select
        ref={ref}
        id={id}
        className={clsx(
          'h-12 w-full rounded-200 border border-border-primary bg-surface px-3 text-[16px] text-content-primary outline-none transition-colors duration-150 hover:border-border-inactive focus:border-border-active focus:ring-2 focus:ring-brand/20',
          className,
        )}
        {...props}
      >
        {children}
      </select>
    </div>
  )
})
