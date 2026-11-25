/** Mirrors backend/src/Tawzeef.Application/Features/Offices/OfficeDtos.cs's OfficeDto. */

export type OfficeProfile = {
  id: string
  name: string
  arName: string | null
  code: string
  address: string | null
  city: string | null
  phone: string | null
  email: string | null
  status: 'Active' | 'Inactive'
  logoUrl: string | null
}

export type UpdateOfficeProfilePayload = {
  name: string
  arName?: string
  address?: string
  city?: string
  phone?: string
  email?: string
}
