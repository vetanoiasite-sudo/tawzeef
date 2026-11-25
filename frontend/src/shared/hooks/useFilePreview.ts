import { useState } from 'react'
import { httpClient } from '@/shared/api/httpClient'

export type FilePreviewState = {
  url: string
  fileName: string
  contentType: string
}

/** Fetches an authenticated file (via httpClient.getBlob, same as every private upload in this
 * app) and hands back an object URL to show in <FilePreviewModal> — the viewer replaces the old
 * "force a save-to-disk download just to see the file" pattern everywhere it used to exist
 * (2026-11-25 product decision: view inline first, download stays available as a secondary
 * action inside the modal). */
export function useFilePreview() {
  const [preview, setPreview] = useState<FilePreviewState | null>(null)
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState<string | undefined>()

  async function open(path: string) {
    setIsLoading(true)
    setError(undefined)
    try {
      const { blob, fileName } = await httpClient.getBlob(path)
      const url = URL.createObjectURL(blob)
      setPreview({ url, fileName, contentType: blob.type })
    } catch {
      setError('تعذّر فتح الملف. حاول مرة أخرى.')
    } finally {
      setIsLoading(false)
    }
  }

  function close() {
    if (preview) URL.revokeObjectURL(preview.url)
    setPreview(null)
  }

  return { preview, isLoading, error, open, close }
}
