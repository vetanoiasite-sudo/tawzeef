/**
 * Design tokens — single source for both the CSS variables below and any UI-kit theme config
 * added later. Confirmed palette: docs/UI_UX_GUIDELINES.md §2.1 and §5 (status color mapping).
 */
export const theme = {
  color: {
    primary: '#0A66C2',
    primaryDark: '#072F5F',
    primaryAccent: '#70B5F9',
    surface: '#FFFFFF',
    surfaceMuted: '#F6F7F8',
    text: '#141413',
    textMuted: '#6B7280',
    success: '#16A34A',
    warning: '#D97706',
    danger: '#DC2626',
    neutral: '#8A8F98',
    info: '#2563EB',
  },
} as const

export type StatusTone = 'success' | 'warning' | 'danger' | 'neutral' | 'info'
