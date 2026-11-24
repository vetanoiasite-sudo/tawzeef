import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Link, useNavigate } from 'react-router-dom'
import { ApiError } from '@/shared/api/types'
import { useSessionStore } from '@/shared/auth/sessionStore'
import {
  EmployerDocumentUploadCard,
  REQUIRED_DOCUMENT_TYPES,
  useSubmitEmployerProfileForReview,
  useUploadEmployerDocument,
  type EmployerDocument,
  type EmployerDocumentTypeCode,
} from '@/features/employer-profile'
import { toSessionUser } from '../api/auth.mapper'
import {
  useRegisterEmployer,
  useRequestEmailOtp,
  useVerifyEmailOtp,
} from '../api/auth.queries'
import { AuthHeader } from '../components/AuthHeader'
import { AuthScaffold } from '../components/AuthScaffold'
import { EmailStep } from '../components/EmailStep'
import { EmployerDetailsStep } from '../components/EmployerDetailsStep'
import { OtpStep } from '../components/OtpStep'
import { REGISTRATION_STEPS_WITH_DOCUMENTS, RegistrationStepper } from '../components/RegistrationStepper'
import type { EmployerDetailsFormValues } from '../schemas/auth.schemas'
import { Button } from '@/shared/ui/Button'

function errorMessage(error: unknown): string | undefined {
  return error instanceof ApiError ? error.messages[0] : undefined
}

// Step 4 of registration, confirmed 2026-11-24: mirrors RegisterJobSeekerPage exactly — office
// choice is part of step 3 (details) and document upload is its own step 4 inside the wizard
// (the account already exists by then, so the authenticated upload endpoint works), not a
// redirect to the company-profile page after the wizard ends.
export default function RegisterEmployerPage() {
  const { t } = useTranslation('auth')
  const navigate = useNavigate()
  const setSession = useSessionStore((s) => s.setSession)

  const [step, setStep] = useState<'email' | 'otp' | 'details' | 'documents'>('email')
  const [email, setEmail] = useState('')
  const [otpToken, setOtpToken] = useState('')
  const [documents, setDocuments] = useState<EmployerDocument[]>([])

  const requestEmailOtp = useRequestEmailOtp()
  const verifyEmailOtp = useVerifyEmailOtp()
  const registerEmployer = useRegisterEmployer()
  const uploadDocument = useUploadEmployerDocument()
  const submitForReview = useSubmitEmployerProfileForReview()

  function handleRequestOtp(values: { email: string }) {
    requestEmailOtp.mutate(
      { email: values.email },
      {
        onSuccess: () => {
          setEmail(values.email)
          setStep('otp')
        },
      },
    )
  }

  function handleVerifyOtp(values: { code: string }) {
    verifyEmailOtp.mutate(
      { email, code: values.code },
      {
        onSuccess: (result) => {
          setOtpToken(result.otpToken)
          setStep('details')
        },
      },
    )
  }

  function handleRegister(values: EmployerDetailsFormValues) {
    registerEmployer.mutate(
      {
        representativeName: values.representativeName,
        mobileNumber: values.mobileNumber,
        email,
        otpToken,
        password: values.password,
        legalName: values.legalName,
        displayName: values.displayName,
        officeIds: values.officeIds,
        allOffices: values.allOffices,
      },
      {
        onSuccess: (result) => {
          setSession(result.accessToken, toSessionUser(result))
          // The account now exists, so the authenticated upload endpoint works — move to step 4
          // instead of redirecting away (confirmed 2026-11-24: document upload is part of the
          // wizard, not a separate page reached afterward).
          setStep('documents')
        },
      },
    )
  }

  function handleUploadDocument(documentType: EmployerDocumentTypeCode, file: File) {
    uploadDocument.mutate(
      { documentType, file },
      {
        onSuccess: (document) => {
          setDocuments((prev) => [...prev.filter((d) => d.documentType !== documentType), document])
        },
      },
    )
  }

  function handleFinish() {
    const allPresent = REQUIRED_DOCUMENT_TYPES.every((type) =>
      documents.some((d) => d.documentType === type),
    )
    if (allPresent) {
      submitForReview.mutate(undefined, {
        onSuccess: () => void navigate('/employer/dashboard', { replace: true }),
        onError: () => void navigate('/employer/dashboard', { replace: true }),
      })
    } else {
      void navigate('/employer/dashboard', { replace: true })
    }
  }

  return (
    <AuthScaffold maxWidth={520}>
      <AuthHeader title={t('registerEmployerTitle')} />

      <RegistrationStepper current={step} steps={REGISTRATION_STEPS_WITH_DOCUMENTS} />

      {step === 'email' && (
        <>
          <p className="mb-4 text-sm text-content-secondary">{t('emailStepHint')}</p>
          <EmailStep
            defaultEmail={email}
            isSubmitting={requestEmailOtp.isPending}
            submitError={errorMessage(requestEmailOtp.error)}
            onSubmit={handleRequestOtp}
          />
        </>
      )}

      {step === 'otp' && (
        <OtpStep
          contact={email}
          isSubmitting={verifyEmailOtp.isPending}
          isResending={requestEmailOtp.isPending}
          submitError={errorMessage(verifyEmailOtp.error)}
          onSubmit={handleVerifyOtp}
          onResend={() => requestEmailOtp.mutate({ email })}
          onChangeContact={() => setStep('email')}
        />
      )}

      {step === 'details' && (
        <EmployerDetailsStep
          email={email}
          isSubmitting={registerEmployer.isPending}
          submitError={errorMessage(registerEmployer.error)}
          onSubmit={handleRegister}
        />
      )}

      {step === 'documents' && (
        <div className="flex flex-col gap-4">
          <p className="text-sm text-content-secondary">{t('documentsStepHint')}</p>

          {REQUIRED_DOCUMENT_TYPES.map((type) => (
            <EmployerDocumentUploadCard
              key={type}
              documentType={type}
              document={documents.find((d) => d.documentType === type)}
              isUploading={uploadDocument.isPending}
              error={
                uploadDocument.variables?.documentType === type
                  ? errorMessage(uploadDocument.error)
                  : undefined
              }
              onUpload={(file) => handleUploadDocument(type, file)}
            />
          ))}

          {errorMessage(submitForReview.error) && (
            <p className="text-sm text-danger">{errorMessage(submitForReview.error)}</p>
          )}

          <Button
            type="button"
            variant="primary"
            disabled={submitForReview.isPending}
            onClick={handleFinish}
          >
            {t('finishRegistration')}
          </Button>
          <p className="text-xs text-content-tertiary">{t('skipDocumentsForNow')}</p>
        </div>
      )}

      {step !== 'documents' && (
        <p className="mt-6 text-center text-[14px] text-content-secondary">
          {t('alreadyHaveAccount')}{' '}
          <Link to="/login" className="font-bold text-brand hover:text-brand-hover hover:underline">
            {t('loginLink')}
          </Link>
          {' · '}
          <Link
            to="/register/job-seeker"
            className="font-bold text-brand hover:text-brand-hover hover:underline"
          >
            {t('registerLink')}
          </Link>
        </p>
      )}
    </AuthScaffold>
  )
}
