/** Mirrors backend/src/Tawzeef.Application/Features/Offices/OfficeDtos.cs. */

export type OfficeStatusCode = 'Active' | 'Inactive'

export type Office = {
  id: string
  name: string
  arName: string | null
  code: string
  address: string | null
  city: string | null
  phone: string | null
  email: string | null
  status: OfficeStatusCode
}

export type CreateOfficePayload = {
  name: string
  arName?: string
  code: string
  address?: string
  city?: string
  phone?: string
  email?: string
}

export type UpdateOfficePayload = {
  name: string
  arName?: string
  address?: string
  city?: string
  phone?: string
  email?: string
}

export type OfficePlatformStats = {
  totalOfficesCount: number
  activeOfficesCount: number
  inactiveOfficesCount: number
}
