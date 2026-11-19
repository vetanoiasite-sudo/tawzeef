import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'

// Latin font — Vitamin's default typography stack is Roboto.
import '@fontsource/roboto/400.css'
import '@fontsource/roboto/500.css'
import '@fontsource/roboto/700.css'

// Vitamin design system (Decathlon). Using the `-unreset` bundle so we don't
// double up on resets — Tailwind v4 (loaded by globals.css) already ships its
// own preflight. Icon font is Vitamix (Vitamin's icon set).
import '@vtmn/css/dist/index-unreset.css'
import '@vtmn/icons/dist/vitamix/font/vitamix.css'

// Our own overrides, loaded last so they win: Noto Kufi Arabic for Arabic
// content, RTL mirroring tweaks, and the project token palette.
import './app/styles/globals.css'
import App from './app/App.tsx'

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <App />
  </StrictMode>,
)
