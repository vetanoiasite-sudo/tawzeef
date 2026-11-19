import { useTranslation } from 'react-i18next'
import { Link, useParams } from 'react-router-dom'
import { EvaluationPanel } from '../components/EvaluationPanel'
import { HiringFollowUpPanel } from '../components/HiringFollowUpPanel'
import { InterviewPanel } from '../components/InterviewPanel'
import { InterviewReportPanel } from '../components/InterviewReportPanel'

export default function OfficeApplicationDetailPage() {
  const { t } = useTranslation('interviewsAndReports')
  const { id } = useParams<{ id: string }>()
  const applicationId = id ?? ''

  return (
    <div className="mx-auto flex max-w-2xl flex-col gap-8 px-4 py-10">
      <Link to="/office/job-advertisements" className="text-sm text-primary">
        {t('backToApplicants')}
      </Link>
      <h1 className="text-lg font-semibold text-text">{t('applicationDetailTitle')}</h1>

      <section className="flex flex-col gap-3">
        <h2 className="font-semibold text-text">{t('interviewSection')}</h2>
        <InterviewPanel applicationId={applicationId} editable={false} />
      </section>

      <section className="flex flex-col gap-3">
        <h2 className="font-semibold text-text">{t('interviewReportSection')}</h2>
        <InterviewReportPanel applicationId={applicationId} editable={false} />
      </section>

      <section className="flex flex-col gap-3">
        <h2 className="font-semibold text-text">{t('evaluationSection')}</h2>
        <EvaluationPanel applicationId={applicationId} editable={false} />
      </section>

      <section className="flex flex-col gap-3">
        <h2 className="font-semibold text-text">{t('hiringFollowUpSection')}</h2>
        <HiringFollowUpPanel applicationId={applicationId} canManage />
      </section>
    </div>
  )
}
