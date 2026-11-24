import { useState, type FormEvent } from 'react'
import { useTranslation } from 'react-i18next'
import { useNavigate } from 'react-router-dom'
import { useCreateCourse } from '../api/officeCourses.queries'
import { ApiError } from '@/shared/api/types'
import { PageHeader } from '@/shared/ui/PageHeader'
import { Card } from '@/shared/ui/Card'
import { Button } from '@/shared/ui/Button'
import { TextInput, TextArea } from '@/shared/ui/TextInput'

export default function CourseCreatePage() {
  const { t } = useTranslation('officeCourses')
  const navigate = useNavigate()
  const [title, setTitle] = useState('')
  const [description, setDescription] = useState('')
  const [contactPhone, setContactPhone] = useState('')

  const create = useCreateCourse()
  const error = create.error instanceof ApiError ? create.error.messages[0] : undefined

  function handleSubmit(e: FormEvent) {
    e.preventDefault()
    create.mutate(
      { title: title.trim(), description: description.trim(), contactPhone: contactPhone.trim() },
      { onSuccess: (course) => void navigate(`/office/courses/${course.id}`) },
    )
  }

  return (
    <div className="mx-auto flex max-w-2xl flex-col gap-6 px-4 py-8 sm:px-6">
      <PageHeader title={t('createAction')} />

      <form onSubmit={handleSubmit}>
        <Card className="flex flex-col gap-4">
          <TextInput
            id="courseTitle"
            label={t('titleLabel')}
            value={title}
            onChange={(e) => setTitle(e.target.value)}
          />
          <TextArea
            id="courseDescription"
            label={t('descriptionLabel')}
            rows={4}
            value={description}
            onChange={(e) => setDescription(e.target.value)}
          />
          <TextInput
            id="courseContactPhone"
            type="tel"
            label={t('contactPhoneLabel')}
            value={contactPhone}
            onChange={(e) => setContactPhone(e.target.value)}
          />

          {error && <p className="text-sm text-danger">{error}</p>}

          <Button
            type="submit"
            variant="primary"
            disabled={create.isPending || !title.trim() || !contactPhone.trim()}
            className="self-start"
          >
            {t('save')}
          </Button>
        </Card>
      </form>
    </div>
  )
}
