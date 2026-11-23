import { useEffect, useRef, useState } from 'react'

/**
 * `true` once the element has scrolled into view — drives the homepage's fade/slide-up section
 * reveals. Fires once (unobserves after first intersection) and starts already-`true` for anyone
 * with `prefers-reduced-motion: reduce`, so motion is pure decoration, never a requirement to see
 * content (ui-ux-pro-max `ux` domain: "no reduced-motion" is a flagged anti-pattern).
 */
export function useReveal<T extends HTMLElement>(threshold = 0.2) {
  const ref = useRef<T | null>(null)
  const [visible, setVisible] = useState(
    () =>
      typeof window !== 'undefined' &&
      window.matchMedia('(prefers-reduced-motion: reduce)').matches,
  )

  useEffect(() => {
    if (visible || !ref.current) return
    const node = ref.current
    const observer = new IntersectionObserver(
      ([entry]) => {
        if (entry?.isIntersecting) {
          setVisible(true)
          observer.unobserve(node)
        }
      },
      { threshold },
    )
    observer.observe(node)
    return () => observer.disconnect()
  }, [threshold, visible])

  return { ref, visible }
}
