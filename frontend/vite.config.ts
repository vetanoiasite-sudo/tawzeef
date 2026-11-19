import path from 'node:path'
import tailwindcss from '@tailwindcss/vite'
import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

// https://vite.dev/config/
export default defineConfig({
  plugins: [tailwindcss(), react()],
  resolve: {
    alias: {
      '@': path.resolve(import.meta.dirname, './src'),
    },
  },
  build: {
    // `npm run build` writes straight into the .NET API's wwwroot, so `dotnet run`
    // (after a build) serves the SPA itself — no separate Vite server needed in production.
    outDir: path.resolve(import.meta.dirname, '../backend/src/Tawzeef.Api/wwwroot'),
    emptyOutDir: true,
  },
})
