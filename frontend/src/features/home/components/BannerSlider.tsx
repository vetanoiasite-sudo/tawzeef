import { useEffect, useState } from 'react'
import { useHomeBanners } from '../api/homeBanners.queries'
import { env } from '@/shared/config/env'

const AUTO_ADVANCE_MS = 5000

/** Home-page-only promotional slides (office-authored, image-only, no link) — a plain in-page
 * block between sections, never a popup/overlay, so it never blocks the visitor from scrolling
 * past it. Renders nothing at all if there are no active banners. */
export function BannerSlider() {
  const { data: banners } = useHomeBanners()
  const [index, setIndex] = useState(0)
  const count = banners?.length ?? 0

  useEffect(() => {
    if (count <= 1) return
    const id = setInterval(() => setIndex((i) => (i + 1) % count), AUTO_ADVANCE_MS)
    return () => clearInterval(id)
  }, [count])

  if (!banners || banners.length === 0) return null

  return (
    <section className="w-full px-4 py-10 sm:px-8 xl:px-16 2xl:px-24">
      <div className="relative mx-auto aspect-[16/5] w-full max-w-5xl overflow-hidden rounded-300 bg-surface-muted">
        {banners.map((banner, i) => (
          <img
            key={banner.id}
            src={`${env.apiBaseUrl}${banner.imageUrl}`}
            alt=""
            className={
              'absolute inset-0 size-full object-cover transition-opacity duration-700 ' +
              (i === index ? 'opacity-100' : 'opacity-0')
            }
          />
        ))}

        {banners.length > 1 && (
          <div className="absolute inset-x-0 bottom-3 flex items-center justify-center gap-1.5">
            {banners.map((banner, i) => (
              <button
                key={banner.id}
                type="button"
                aria-label={`الشريحة ${i + 1}`}
                onClick={() => setIndex(i)}
                className={
                  'h-1.5 rounded-full transition-[width,background-color] duration-200 ' +
                  (i === index ? 'w-6 bg-white' : 'w-1.5 bg-white/60')
                }
              />
            ))}
          </div>
        )}
      </div>
    </section>
  )
}
