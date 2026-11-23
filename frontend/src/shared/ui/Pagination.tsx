import { useTranslation } from 'react-i18next'
import { Button } from './Button'

/** Replaces the identical prev/next/"page X of Y" block that used to be hand-written on every
 * paged list in the app. */
export function Pagination({
  pageIndex,
  totalPages,
  onChange,
}: {
  pageIndex: number
  totalPages: number
  onChange: (pageIndex: number) => void
}) {
  const { t } = useTranslation('common')
  if (totalPages <= 1) return null

  return (
    <div className="flex items-center justify-center gap-4">
      <Button
        variant="secondary"
        disabled={pageIndex === 0}
        onClick={() => onChange(pageIndex - 1)}
      >
        {t('prevPage')}
      </Button>
      <span className="text-sm text-text-muted">
        {t('pageOf', { current: pageIndex + 1, total: totalPages })}
      </span>
      <Button
        variant="secondary"
        disabled={pageIndex + 1 >= totalPages}
        onClick={() => onChange(pageIndex + 1)}
      >
        {t('nextPage')}
      </Button>
    </div>
  )
}
