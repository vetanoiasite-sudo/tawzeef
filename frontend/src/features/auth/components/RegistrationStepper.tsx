import clsx from 'clsx'
import { useTranslation } from 'react-i18next'

const STEPS = ['phone', 'otp', 'details'] as const
type Step = (typeof STEPS)[number]

const LABEL_KEY: Record<Step, string> = {
  phone: 'auth:stepPhone',
  otp: 'auth:stepOtp',
  details: 'auth:stepDetails',
}

/**
 * Horizontal progress stepper placed at the top of registration cards. Each
 * step is a numbered circle connected by a track; completed steps use the
 * brand color, the current step is highlighted, and upcoming steps are muted.
 * Shared across job-seeker registration, employer registration, and the
 * office-staff accept-invite flow.
 */
export function RegistrationStepper({ current }: { current: Step }) {
  const { t } = useTranslation('auth')
  const currentIndex = STEPS.indexOf(current)

  return (
    <div className="mb-6">
      <p className="mb-3 text-center text-[12px] font-bold uppercase tracking-[0.14em] text-brand">
        {t('stepProgress', {
          defaultValue: 'الخطوة {{current}} من {{total}}',
          current: currentIndex + 1,
          total: STEPS.length,
        })}
      </p>
      <ol className="flex items-start justify-between" aria-label="progress">
        {STEPS.map((step, index) => {
          const isDone = index < currentIndex
          const isCurrent = index === currentIndex
          return (
            <li
              key={step}
              className="relative flex flex-1 flex-col items-center"
              aria-current={isCurrent ? 'step' : undefined}
            >
              {index > 0 && (
                <span
                  aria-hidden
                  className={clsx(
                    'absolute top-[14px] h-[2px] w-1/2 -translate-y-1/2',
                    'end-1/2',
                    isDone || isCurrent ? 'bg-brand' : 'bg-border-primary',
                  )}
                />
              )}
              {index < STEPS.length - 1 && (
                <span
                  aria-hidden
                  className={clsx(
                    'absolute top-[14px] h-[2px] w-1/2 -translate-y-1/2',
                    'start-1/2',
                    isDone ? 'bg-brand' : 'bg-border-primary',
                  )}
                />
              )}
              <span
                className={clsx(
                  'relative z-10 flex h-7 w-7 items-center justify-center rounded-full border text-[12px] font-bold transition-colors',
                  isDone && 'border-brand bg-brand text-white',
                  isCurrent && 'border-brand bg-brand text-white',
                  !isDone && !isCurrent && 'border-border-primary bg-surface text-content-tertiary',
                )}
              >
                {isDone ? <CheckIcon /> : index + 1}
              </span>
              <span
                className={clsx(
                  'mt-2 text-center text-[12px] leading-[16px]',
                  isCurrent ? 'font-bold text-content-primary' : 'text-content-tertiary',
                )}
              >
                {t(LABEL_KEY[step])}
              </span>
            </li>
          )
        })}
      </ol>
    </div>
  )
}

function CheckIcon() {
  return (
    <svg width="14" height="14" viewBox="0 0 24 24" fill="none" aria-hidden>
      <path
        d="M5 12l5 5L20 7"
        stroke="currentColor"
        strokeWidth="3"
        strokeLinecap="round"
        strokeLinejoin="round"
      />
    </svg>
  )
}
