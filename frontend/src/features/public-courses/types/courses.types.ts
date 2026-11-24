/** Mirrors backend/src/Tawzeef.Application/Features/Courses/CourseDtos.cs (public surface). */

export type PublicCourseListItem = {
  id: string
  title: string
  imageUrl: string | null
  officeName: string
}

export type PublicCourseDetail = {
  id: string
  title: string
  description: string | null
  contactPhone: string
  imageUrl: string | null
  officeName: string
}

export type CourseSearchFilter = {
  keyword?: string
  pageIndex: number
  pageSize: number
}
