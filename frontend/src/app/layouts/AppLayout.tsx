import { Outlet } from 'react-router-dom'
import { TawzeefLogo } from '@/shared/ui/TawzeefLogo'

export function AppLayout() {
  return (
    <div className="flex min-h-screen flex-col">
      <header className="flex items-center border-b border-surface-muted px-6 py-3">
        <TawzeefLogo height={32} />
      </header>
      <main className="flex-1">
        <Outlet />
      </main>
    </div>
  )
}
