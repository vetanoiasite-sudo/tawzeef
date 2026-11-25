/** Mirrors backend/src/Tawzeef.Application/Features/JobSeekers/JobSeekerCourseDtos.cs. */

export type JobSeekerCourse = {
  id: string
  title: string
  institution: string
  whatLearned: string | null
  hasCertificate: boolean
  certificateFileName: string | null
  creationDate: string
}

export type CreateJobSeekerCoursePayload = {
  title: string
  institution: string
  whatLearned?: string
}

export type UpdateJobSeekerCoursePayload = CreateJobSeekerCoursePayload
