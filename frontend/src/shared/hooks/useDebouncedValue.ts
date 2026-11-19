import { useEffect, useState } from 'react'

/** The "debounced fetch" pattern: the returned value only updates `delayMs` after the input
 * stops changing, so a search box can bind a query straight to the committed value instead of
 * firing a request per keystroke. */
export function useDebouncedValue<T>(value: T, delayMs = 400): T {
  const [debounced, setDebounced] = useState(value)

  useEffect(() => {
    const timer = setTimeout(() => setDebounced(value), delayMs)
    return () => clearTimeout(timer)
  }, [value, delayMs])

  return debounced
}
