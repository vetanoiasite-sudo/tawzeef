import clsx from 'clsx'

/** The repeated "status filter pills" row (all queue pages). Flex-wrap so chips never get
 * clipped into one fixed-height row (a confirmed a11y/layout anti-pattern). */
export function FilterChips<T extends string>({
  options,
  value,
  onChange,
}: {
  options: { value: T; label: string }[]
  value: T
  onChange: (value: T) => void
}) {
  return (
    <div className="flex flex-wrap gap-2">
      {options.map((option) => (
        <button
          key={option.value}
          type="button"
          onClick={() => onChange(option.value)}
          className={clsx(
            'min-h-9 rounded-full px-3.5 text-sm font-medium transition-colors duration-150',
            value === option.value
              ? 'bg-primary text-surface'
              : 'border border-surface-muted bg-surface text-text hover:border-primary/40 hover:text-primary',
          )}
        >
          {option.label}
        </button>
      ))}
    </div>
  )
}
