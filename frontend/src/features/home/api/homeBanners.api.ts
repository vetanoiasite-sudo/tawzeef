import { httpClient } from '@/shared/api/httpClient'

export type HomeBanner = {
  id: string
  imageUrl: string
}

export const homeBannersApi = {
  // Public, anonymous (api/banners) — image-only home page promotional slides, no link
  // (explicit product decision, 2026-11-24). Only the home page needs this, so it lives here
  // rather than as its own feature.
  list: () => httpClient.get<HomeBanner[]>('/api/banners'),
}
