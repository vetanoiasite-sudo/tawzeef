import { useEffect, useState } from 'react'

/** Animates from 0 to `target` over `durationMs`, only while `start` is true — pairs with
 * `useReveal` so the stat strip counts up once it scrolls into view, not on every render. Skips
 * straight to `target` under `prefers-reduced-motion: reduce`. */
export function useCountUp(target: number, start: boolean, durationMs = 900): number {
  const [value, setValue] = useState(0)

  useEffect(() => {
    if (!start) return
    if (typeof window !== 'undefined' && window.matchMedia('(prefers-reduced-motion: reduce)').matches) {
      setValue(target)
      return
    }

    let frame: number
    const startTime = performance.now()
    const tick = (now: number) => {
      const progress = Math.min((now - startTime) / durationMs, 1)
      const eased = 1 - (1 - progress) ** 3 // ease-out cubic
      setValue(Math.round(target * eased))
      if (progress < 1) frame = requestAnimationFrame(tick)
    }
    frame = requestAnimationFrame(tick)
    return () => cancelAnimationFrame(frame)
  }, [start, target, durationMs])

  return value
}
