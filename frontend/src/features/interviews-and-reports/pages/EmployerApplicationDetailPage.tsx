import { useTranslation } from 'react-i18next'
import { Link, useParams } from 'react-router-dom'
import { EvaluationPanel } from '../components/EvaluationPanel'
import { HiringFollowUpPanel } from '../components/HiringFollowUpPanel'
import { InterviewPanel } from '../components/InterviewPanel'
import { InterviewReportPanel } from '../components/InterviewReportPanel'
import { PageHeader } from '@/shared/ui/PageHeader'

export default function EmployerApplicationDetailPage() {
  const { t } = useTranslation('interviewsAndReports')
  const { id } = useParams<{ id: string }>()
  const applicationId = id ?? ''

  return (
    <div className="mx-auto flex max-w-2xl flex-col gap-6 px-4 py-8 sm:px-6">
      <Link
        to="/employer/job-advertisements"
        className="text-sm font-medium text-primary hover:underline"
      >
        {t('backToApplicants')}
      </Link>
      <PageHeader title={t('applicationDetailTitle')} />

      <section className="flex flex-col gap-3">
        <h2 className="text-base font-semibold text-text">{t('interviewSection')}</h2>
        <InterviewPanel applicationId={applicationId} editable />
      </section>

      <section className="flex flex-col gap-3">
        <h2 className="text-base font-semibold text-text">{t('interviewReportSection')}</h2>
        <InterviewReportPanel applicationId={applicationId} editable />
      </section>

      <section className="flex flex-col gap-3">
        <h2 className="text-base font-semibold text-text">{t('evaluationSection')}</h2>
        <EvaluationPanel applicationId={applicationId} editable />
      </section>

      <section className="flex flex-col gap-3">
        <h2 className="text-base font-semibold text-text">{t('hiringFollowUpSection')}</h2>
        <HiringFollowUpPanel applicationId={applicationId} canManage={false} />
      </section>
    </div>
  )
}
