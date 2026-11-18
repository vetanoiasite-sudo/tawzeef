import { TawzeefLogo } from '@/shared/ui/TawzeefLogo'

// Placeholder for Phase 2 Foundation: providers (AppProviders) + router will replace this body.
function App() {
  return (
    <div
      style={{
        display: 'flex',
        minHeight: '100vh',
        flexDirection: 'column',
        alignItems: 'center',
        justifyContent: 'center',
        gap: '16px',
        backgroundColor: '#FFFFFF',
      }}
    >
      <TawzeefLogo height={82} />
      <p style={{ color: '#6B7280', fontFamily: 'sans-serif', fontSize: '14px' }}>
        Foundation scaffold — providers, router and layouts land in Phase 2.
      </p>
    </div>
  )
}

export default App
