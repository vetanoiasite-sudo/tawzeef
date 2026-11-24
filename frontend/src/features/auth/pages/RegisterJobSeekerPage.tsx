import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Link, useNavigate } from 'react-router-dom'
import { ApiError } from '@/shared/api/types'
import { useSessionStore } from '@/shared/auth/sessionStore'
import {
  DocumentUploadCard,
  REQUIRED_DOCUMENT_TYPES,
  useSubmitJobSeekerProfileForReview,
  useUploadJobSeekerDocument,
  type JobSeekerDocument,
  type JobSeekerDocumentTypeCode,
} from '@/features/jobseeker-profile'
import { toSessionUser } from '../api/auth.mapper'
import {
  useRegisterJobSeeker,
  useRequestEmailOtp,
  useVerifyEmailOtp,
} from '../api/auth.queries'
import { AuthHeader } from '../components/AuthHeader'
import { AuthScaffold } from '../components/AuthScaffold'
import { DetailsStep } from '../components/DetailsStep'
import { EmailStep } from '../components/EmailStep'
import { OtpStep } from '../components/OtpStep'
import { JOB_SEEKER_REGISTRATION_STEPS, RegistrationStepper } from '../components/RegistrationStepper'
import type { JobSeekerDetailsFormValues } from '../schemas/auth.schemas'
import { Button } from '@/shared/ui/Button'

function errorMessage(error: unknown): string | undefined {
  return error instanceof ApiError ? error.messages[0] : undefined
}

// Step 4 of registration, confirmed 2026-11-24: document upload happens inside the wizard itself
// (the account already exists by then, so the authenticated upload endpoint works), not on a
// separate page reached after the wizard ends.
export default function RegisterJobSeekerPage() {
  const { t } = useTranslation('auth')
  const navigate = useNavigate()
  const setSession = useSessionStore((s) => s.setSession)

  const [step, setStep] = useState<'email' | 'otp' | 'details' | 'documents'>('email')
  const [email, setEmail] = useState('')
  const [otpToken, setOtpToken] = useState('')
  const [documents, setDocuments] = useState<JobSeekerDocument[]>([])

  const requestEmailOtp = useRequestEmailOtp()
  const verifyEmailOtp = useVerifyEmailOtp()
  const registerJobSeeker = useRegisterJobSeeker()
  const uploadDocument = useUploadJobSeekerDocument()
  const submitForReview = useSubmitJobSeekerProfileForReview()

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

  function handleRegister(values: JobSeekerDetailsFormValues) {
    registerJobSeeker.mutate(
      {
        fullName: values.fullName,
        nationalId: values.nationalId,
        mobileNumber: values.mobileNumber,
        email,
        otpToken,
        password: values.password,
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

  function handleUploadDocument(documentType: JobSeekerDocumentTypeCode, file: File) {
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
        onSuccess: () => void navigate('/dashboard', { replace: true }),
        onError: () => void navigate('/dashboard', { replace: true }),
      })
    } else {
      void navigate('/dashboard', { replace: true })
    }
  }

  return (
    <AuthScaffold maxWidth={520}>
      <AuthHeader title={t('registerTitle')} />

      <RegistrationStepper current={step} steps={JOB_SEEKER_REGISTRATION_STEPS} />

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
        <DetailsStep
          email={email}
          isSubmitting={registerJobSeeker.isPending}
          submitError={errorMessage(registerJobSeeker.error)}
          onSubmit={handleRegister}
        />
      )}

      {step === 'documents' && (
        <div className="flex flex-col gap-4">
          <p className="text-sm text-content-secondary">{t('documentsStepHint')}</p>

          {REQUIRED_DOCUMENT_TYPES.map((type) => (
            <DocumentUploadCard
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
            to="/register/employer"
            className="font-bold text-brand hover:text-brand-hover hover:underline"
          >
            {t('registerEmployerLink')}
          </Link>
        </p>
      )}
    </AuthScaffold>
  )
}
