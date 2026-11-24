import { Link } from 'react-router-dom'
import { env } from '@/shared/config/env'
import type { PublicCourseListItem } from '../types/courses.types'

export function CourseCard({ course }: { course: PublicCourseListItem }) {
  return (
    <Link
      to={`/courses/${course.id}`}
      className="group flex h-full flex-col overflow-hidden rounded-300 border border-border-primary bg-surface transition-[transform,box-shadow,border-color] duration-200 hover:-translate-y-1 hover:border-brand hover:shadow-200"
    >
      <div className="flex h-36 w-full items-center justify-center overflow-hidden bg-surface-muted">
        {course.imageUrl ? (
          <img
            src={`${env.apiBaseUrl}${course.imageUrl}`}
            alt={course.title}
            className="size-full object-cover"
          />
        ) : (
          <CoursePlaceholderIcon />
        )}
      </div>
      <div className="flex flex-col gap-1 p-4">
        <h3 className="line-clamp-2 text-[15px] font-bold leading-[20px] text-content-primary group-hover:text-brand">
          {course.title}
        </h3>
        <p className="line-clamp-1 text-[13px] text-content-secondary">{course.officeName}</p>
      </div>
    </Link>
  )
}

function CoursePlaceholderIcon() {
  return (
    <svg width="32" height="32" viewBox="0 0 24 24" fill="none" aria-hidden className="text-content-tertiary">
      <path
        d="M4 6.5 12 3l8 3.5-8 3.5-8-3.5Z"
        stroke="currentColor"
        strokeWidth="1.6"
        strokeLinejoin="round"
      />
      <path d="M7 10v5c0 1.4 2.2 2.5 5 2.5s5-1.1 5-2.5v-5" stroke="currentColor" strokeWidth="1.6" />
      <path d="M20 8v6" stroke="currentColor" strokeWidth="1.6" strokeLinecap="round" />
    </svg>
  )
}
