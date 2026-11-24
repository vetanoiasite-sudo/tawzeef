/** Mirrors backend/src/Tawzeef.Application/Features/Courses/CourseDtos.cs (office surface). */

export type CourseStatusCode = 'Active' | 'Paused'

export type OfficeCourse = {
  id: string
  title: string
  description: string | null
  contactPhone: string
  imageUrl: string | null
  status: CourseStatusCode
  creationDate: string
}

export type CourseFormValues = {
  title: string
  description: string
  contactPhone: string
}

export type CourseQueryFilter = {
  keyword?: string
  pageIndex: number
  pageSize: number
}
