import clsx from 'clsx'
import { forwardRef, type InputHTMLAttributes, type TextareaHTMLAttributes } from 'react'

const fieldClasses =
  'w-full rounded-md border border-surface-muted bg-surface px-3 py-2.5 text-sm text-text outline-none transition-colors duration-150 placeholder:text-text-muted focus:border-primary'

type TextInputProps = InputHTMLAttributes<HTMLInputElement> & { label?: string; error?: string }

export const TextInput = forwardRef<HTMLInputElement, TextInputProps>(function TextInput(
  { label, error, id, className, ...props },
  ref,
) {
  return (
    <div className="flex flex-col gap-1.5">
      {label && (
        <label htmlFor={id} className="text-sm font-medium text-text">
          {label}
        </label>
      )}
      <input ref={ref} id={id} className={clsx(fieldClasses, className)} {...props} />
      {error && <p className="text-xs text-danger">{error}</p>}
    </div>
  )
})

type TextAreaProps = TextareaHTMLAttributes<HTMLTextAreaElement> & { label?: string; error?: string }

export const TextArea = forwardRef<HTMLTextAreaElement, TextAreaProps>(function TextArea(
  { label, error, id, className, rows = 4, ...props },
  ref,
) {
  return (
    <div className="flex flex-col gap-1.5">
      {label && (
        <label htmlFor={id} className="text-sm font-medium text-text">
          {label}
        </label>
      )}
      <textarea ref={ref} id={id} rows={rows} className={clsx(fieldClasses, 'resize-y', className)} {...props} />
      {error && <p className="text-xs text-danger">{error}</p>}
    </div>
  )
})
