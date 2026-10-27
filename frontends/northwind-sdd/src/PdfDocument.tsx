import { useEffect, useRef, useState } from 'react'
import { GlobalWorkerOptions, getDocument } from 'pdfjs-dist'
import workerUrl from 'pdfjs-dist/build/pdf.worker.min.mjs?url'
import type { RenderTask } from 'pdfjs-dist'
import { useNotice } from './context'
GlobalWorkerOptions.workerSrc = workerUrl + '?module=1'

// Render the server's original PDF, including in browsers without a built-in PDF plug-in.
export default function PdfDocument({
  url,
  onReady,
}: {
  url: string
  onReady: (ready: boolean) => void
}) {
  const container = useRef<HTMLDivElement>(null)
  const { show } = useNotice()
  const [busy, setBusy] = useState(true)
  useEffect(() => {
    const root = container.current!
    let disposed = false
    let rendering: RenderTask | undefined
    const loading = getDocument({ url })
    onReady(false)
    setBusy(true)
    void loading.promise
      .then(async (document) => {
        for (let number = 1; number <= document.numPages && !disposed; number++) {
          const page = await document.getPage(number)
          if (disposed) break
          const viewport = page.getViewport({ scale: 1.6 })
          const canvas = window.document.createElement('canvas')
          canvas.width = Math.ceil(viewport.width)
          canvas.height = Math.ceil(viewport.height)
          canvas.setAttribute('aria-label', `Página ${number} de ${document.numPages}`)
          canvas.setAttribute('role', 'img')
          root.append(canvas)
          rendering = page.render({ canvas, viewport })
          await rendering.promise
        }
        if (!disposed) {
          setBusy(false)
          onReady(true)
        }
      })
      .catch((error) => {
        if (!disposed) {
          console.warn(
            'PDF preview:',
            error instanceof Error ? error.message : 'Unable to render document',
          )
          setBusy(false)
          show(
            ['No se pudo dibujar el PDF. Puedes descargarlo o abrirlo en otra pestaña.'],
            'Vista previa del PDF',
          )
        }
      })
    return () => {
      disposed = true
      rendering?.cancel()
      void loading.destroy()
      root.replaceChildren()
      onReady(false)
    }
  }, [url, show, onReady])
  return (
    <div className="pdf-document">
      {busy && (
        <p className="empty" role="status">
          Cargando páginas del PDF…
        </p>
      )}
      <div ref={container} className="pdf-pages" />
    </div>
  )
}
