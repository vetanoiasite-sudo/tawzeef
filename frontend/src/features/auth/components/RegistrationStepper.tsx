import { useTranslation } from 'react-i18next'
import clsx from 'clsx'

const STEPS = ['phone', 'otp', 'details'] as const
type Step = (typeof STEPS)[number]

const LABEL_KEY: Record<Step, string> = {
  phone: 'auth:stepPhone',
  otp: 'auth:stepOtp',
  details: 'auth:stepDetails',
}

// UI_UX_GUIDELINES.md §13: the multi-step registration form gets a stepper. Document upload and
// review steps are deferred (not yet built on the backend), so this only ever shows these three.
export function RegistrationStepper({ current }: { current: Step }) {
  const { t } = useTranslation('auth')
  const currentIndex = STEPS.indexOf(current)

  return (
    <ol className="flex items-center justify-center gap-2 text-xs">
      {STEPS.map((step, index) => (
        <li key={step} className="flex items-center gap-2">
          <span
            className={clsx(
              'flex h-6 w-6 items-center justify-center rounded-full font-medium',
              index <= currentIndex ? 'bg-primary text-surface' : 'bg-surface-muted text-text-muted',
            )}
          >
            {index + 1}
          </span>
          <span className={index === currentIndex ? 'text-text' : 'text-text-muted'}>
            {t(LABEL_KEY[step])}
          </span>
          {index < STEPS.length - 1 && <span className="mx-1 text-text-muted">—</span>}
        </li>
      ))}
    </ol>
  )
}
