import { useRef, type ChangeEvent } from 'react'
import { useTranslation } from 'react-i18next'
import { useCreateBanner, useOfficeBanners, useToggleBannerPause } from '../api/officeBanners.queries'
import { ApiError } from '@/shared/api/types'
import { env } from '@/shared/config/env'
import { PageHeader } from '@/shared/ui/PageHeader'
import { Button } from '@/shared/ui/Button'
import { Card } from '@/shared/ui/Card'
import { EmptyState } from '@/shared/ui/EmptyState'
import { LoadingState } from '@/shared/ui/LoadingState'

export default function BannersManagementPage() {
  const { t } = useTranslation('officeBanners')
  const { data: banners, isLoading } = useOfficeBanners()
  const create = useCreateBanner()
  const togglePause = useToggleBannerPause()
  const inputRef = useRef<HTMLInputElement>(null)

  const createError = create.error instanceof ApiError ? create.error.messages[0] : undefined

  function handleFileChange(e: ChangeEvent<HTMLInputElement>) {
    const file = e.target.files?.[0]
    if (file) create.mutate(file)
    e.target.value = ''
  }

  return (
    <div className="mx-auto flex max-w-4xl flex-col gap-6 px-4 py-8 sm:px-6">
      <PageHeader
        title={t('pageTitle')}
        action={
          <>
            <input
              ref={inputRef}
              type="file"
              accept="image/jpeg,image/png"
              className="hidden"
              onChange={handleFileChange}
            />
            <Button variant="primary" disabled={create.isPending} onClick={() => inputRef.current?.click()}>
              {t('uploadAction')}
            </Button>
          </>
        }
      />
      <p className="text-sm text-content-secondary">{t('pageSubtitle')}</p>
      <p className="text-xs text-content-tertiary">{t('uploadHint')}</p>
      {createError && <p className="text-sm text-danger">{createError}</p>}

      {isLoading && <LoadingState />}

      {!isLoading && banners && banners.length === 0 && <EmptyState message={t('emptyList')} />}

      {!isLoading && banners && banners.length > 0 && (
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          {banners.map((banner) => (
            <Card key={banner.id} className="flex flex-col gap-3">
              <div className="flex h-32 w-full items-center justify-center overflow-hidden rounded-200 bg-surface-muted">
                <img
                  src={`${env.apiBaseUrl}${banner.imageUrl}`}
                  alt=""
                  className="size-full object-cover"
                />
              </div>
              <div className="flex items-center justify-between gap-3">
                <span
                  className={
                    banner.status === 'Active'
                      ? 'inline-flex items-center gap-1.5 rounded-full bg-success/10 px-2.5 py-1 text-xs font-medium text-success'
                      : 'inline-flex items-center gap-1.5 rounded-full bg-neutral/10 px-2.5 py-1 text-xs font-medium text-neutral'
                  }
                >
                  {t(`status.${banner.status}`)}
                </span>
                <Button
                  variant="secondary"
                  size="sm"
                  disabled={togglePause.isPending}
                  onClick={() => togglePause.mutate({ id: banner.id, pause: banner.status === 'Active' })}
                >
                  {banner.status === 'Active' ? t('pauseAction') : t('resumeAction')}
                </Button>
              </div>
            </Card>
          ))}
        </div>
      )}
    </div>
  )
}
