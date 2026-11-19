/**
 * Design tokens mirror — values come from Vitamin (@vtmn/css) re-themed to the
 * Tawzeef indigo brand `#1A23C3`. Mirrors globals.css @theme for the rare JS
 * consumer (inline styles, chart colors, StatusTag badges). Keep both in sync
 * by hand — the set is small.
 */
export const theme = {
  color: {
    // Brand (Tawzeef indigo)
    brand: '#1A23C3',
    brandHover: '#161DA2',
    brandActive: '#121887',
    brandSoft: '#EEF0FA',

    // Legacy aliases — same values as the brand tokens
    primary: '#1A23C3',
    primaryDark: '#121887',
    primaryAccent: '#EEF0FA',

    // Content
    contentPrimary: '#001018',
    contentSecondary: '#4E5D6B',
    contentTertiary: '#687787',
    contentReversed: '#FFFFFF',
    text: '#001018',
    textMuted: '#4E5D6B',

    // Surfaces & borders
    surface: '#FFFFFF',
    surfaceMuted: '#F3F5F7',
    borderPrimary: '#D9DDE1',
    borderInactive: '#8996A2',

    // Status
    success: '#23A942',
    danger: '#E3262F',
    warning: '#D97706',
    info: '#1A23C3',
    neutral: '#8A8F98',

    // Accent (brand yellow — used by the logo arrows and highlight chips)
    accent: '#FFE343',
  },
} as const

export type StatusTone = 'success' | 'warning' | 'danger' | 'neutral' | 'info'
