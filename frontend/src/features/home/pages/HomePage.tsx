import { TawzeefLogo } from '@/shared/ui/TawzeefLogo'

export default function HomePage() {
  return (
    <div className="flex min-h-[60vh] flex-col items-center justify-center gap-4">
      <TawzeefLogo height={82} />
      <p className="text-sm text-text-muted">Foundation scaffold — feature pages land next.</p>
    </div>
  )
}
