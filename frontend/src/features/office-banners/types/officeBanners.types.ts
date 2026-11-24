/** Mirrors backend/src/Tawzeef.Application/Features/Courses/BannerDtos.cs (office surface). */

export type BannerStatusCode = 'Active' | 'Paused'

export type OfficeBanner = {
  id: string
  imageUrl: string
  status: BannerStatusCode
  creationDate: string
}
