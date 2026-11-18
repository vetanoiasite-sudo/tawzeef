type TawzeefLogoProps = {
  /** Height in px; width scales to match. */
  height?: number
  primary?: string
  cross?: string
  accent?: string
}

const DEFAULT_PRIMARY = '#0A66C2'
const DEFAULT_CROSS = '#072F5F'
const DEFAULT_ACCENT = '#70B5F9'

/**
 * The Tawzeef wordmark: a cross for the T (with a tilted briefcase on top)
 * and a small raised flag for the F. See assets/brand/README.md for the
 * approved direction and color roles.
 */
export function TawzeefLogo({
  height = 82,
  primary = DEFAULT_PRIMARY,
  cross = DEFAULT_CROSS,
  accent = DEFAULT_ACCENT,
}: TawzeefLogoProps) {
  const letterSize = (height * 84) / 82

  return (
    <div
      role="img"
      aria-label="Tawzeef"
      // The brand name is Latin and must stay T-A-W-Z-E-E-F regardless of the page's
      // RTL direction — otherwise an RTL ancestor mirrors the flex row and the word reverses.
      style={{ display: 'flex', alignItems: 'flex-end', gap: 4, direction: 'ltr' }}
    >
      <svg width={(height * 60) / 100} height={height} viewBox="0 0 60 100">
        <rect x="0" y="30" width="60" height="13" fill={cross} />
        <rect x="21" y="18" width="14" height="82" fill={cross} />
        <g transform="rotate(18 28 8.5)">
          <rect
            x="24.5"
            y="4"
            width="7"
            height="6"
            rx="1.2"
            fill="none"
            stroke={accent}
            strokeWidth="2.2"
          />
          <rect x="19" y="9" width="18" height="9" rx="1.8" fill={accent} />
        </g>
      </svg>
      {['A', 'W', 'Z', 'E', 'E'].map((letter, i) => (
        <span
          key={i}
          style={{
            fontFamily: "'Space Grotesk', sans-serif",
            fontWeight: 700,
            fontSize: letterSize,
            color: primary,
            lineHeight: 0.8,
            letterSpacing: '-1px',
          }}
        >
          {letter}
        </span>
      ))}
      <svg width={(height * 60) / 100} height={height} viewBox="0 0 60 100">
        <rect x="8" y="0" width="12" height="100" fill={primary} />
        <polygon points="20,6 58,20 20,34" fill={accent} />
        <polygon points="20,42 46,52 20,62" fill={primary} />
      </svg>
    </div>
  )
}
