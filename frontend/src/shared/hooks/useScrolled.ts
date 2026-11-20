import { useEffect, useState } from 'react'

/**
 * `true` once the window has scrolled past `threshold` px from the top.
 * Used by the AppLayout header to swap from a transparent "sitting over the
 * hero image" state to an opaque "surface" state after scroll.
 */
export function useScrolled(threshold = 24): boolean {
  const [scrolled, setScrolled] = useState<boolean>(() =>
    typeof window !== 'undefined' ? window.scrollY > threshold : false,
  )

  useEffect(() => {
    const onScroll = () => setScrolled(window.scrollY > threshold)
    onScroll()
    window.addEventListener('scroll', onScroll, { passive: true })
    return () => window.removeEventListener('scroll', onScroll)
  }, [threshold])

  return scrolled
}
