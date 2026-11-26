/** Mirrors backend/src/Tawzeef.Application/Features/JobAdvertisementPackages/JobAdvertisementPackageDtos.cs. */

export type JobAdvertisementPackage = {
  id: string
  name: string
  level: number
  price: number
  currency: string
  /** Null means unlimited. */
  cvViewLimit: number | null
  durationDays: number
  isActive: boolean
}

export type JobAdvertisementPackagePayload = {
  name: string
  level: number
  price: number
  currency: string
  cvViewLimit: number | null
  durationDays: number
}
