import { lazy, Suspense, useEffect, useRef, useState } from 'react'
import { api } from './api/client'
import { Modal, useFailure, useNotice, useSession } from './context'
import { dateTime, money } from './domain/rules'
import type { Invoice } from './domain/types'

const PdfDocument = lazy(() => import('./PdfDocument'))

export function InvoicePreview({ id, onClose }: { id: number; onClose: () => void }) {
  const { session } = useSession()
  const failure = useFailure()
  const { show } = useNotice()
  const failureRef = useRef(failure)
  failureRef.current = failure
  const [invoice, setInvoice] = useState<Invoice | null>(null)
  const [url, setUrl] = useState('')
  const [busy, setBusy] = useState(true)
  const [pdf, setPdf] = useState(false)
  const [pdfReady, setPdfReady] = useState(false)
  useEffect(() => {
    const clear = () => document.body.classList.remove('printing-invoice')
    window.addEventListener('afterprint', clear)
    return () => {
      clear()
      window.removeEventListener('afterprint', clear)
    }
  }, [])
  useEffect(() => {
    const controller = new AbortController()
    let objectUrl = ''
    void Promise.all([
      api.get<Invoice>(`/orders/${id}`, session!.accessToken, controller.signal),
      api.pdf(id, session!.accessToken, controller.signal),
    ])
      .then(([result, blob]) => {
        if (!controller.signal.aborted) {
          objectUrl = URL.createObjectURL(blob)
          setInvoice(result)
          setUrl(objectUrl)
        }
      })
      .catch((error) => {
        if (!controller.signal.aborted) failureRef.current(error)
      })
      .finally(() => {
        if (!controller.signal.aborted) setBusy(false)
      })
    return () => {
      controller.abort()
      if (objectUrl) URL.revokeObjectURL(objectUrl)
    }
  }, [id, session])
  return (
    <Modal title={`Factura de venta #${id}`} onClose={onClose}>
      {busy ? (
        <p className="empty" role="status">
          Preparando comprobante…
        </p>
      ) : !invoice ? (
        <p className="empty">
          No se pudo cargar la factura. Cierra esta ventana e inténtalo nuevamente.
        </p>
      ) : (
        <>
          <div className="invoice-toolbar">
            <div className="segmented">
              <button className={!pdf ? 'selected' : 'secondary'} onClick={() => setPdf(false)}>
                Detalle
              </button>
              <button className={pdf ? 'selected' : 'secondary'} onClick={() => setPdf(true)}>
                Ver PDF
              </button>
            </div>
            <div className="button-row">
              <a className="button secondary" href={url} download={`Factura-${id}.pdf`}>
                Descargar PDF
              </a>
              <a className="button secondary" href={url} target="_blank" rel="noreferrer">
                Abrir PDF
              </a>
              <button
                className="secondary"
                disabled={!pdfReady}
                onClick={() => {
                  setPdf(true)
                  document.body.classList.add('printing-invoice')
                  requestAnimationFrame(() => {
                    try {
                      window.print()
                    } catch {
                      document.body.classList.remove('printing-invoice')
                      show(['Descarga el PDF para imprimirlo desde tu equipo.'], 'Imprimir factura')
                    }
                  })
                }}
              >
                Imprimir
              </button>
            </div>
          </div>
          <article className={pdf ? 'invoice-sheet hidden' : 'invoice-sheet'}>
            <header className="invoice-heading">
              <div>
                <p className="eyebrow">NorthWind</p>
                <h2>Factura de venta</h2>
              </div>
              <div className="right">
                <strong>#{invoice.id}</strong>
                <p>{dateTime(invoice.orderDate)}</p>
              </div>
            </header>
            <div className="invoice-parties">
              <section>
                <small>Cliente</small>
                <h3>{invoice.customerName}</h3>
                <p>{invoice.customerId}</p>
              </section>
              <section>
                <small>Entrega</small>
                <p>
                  {invoice.shipAddress}
                  <br />
                  {invoice.shipCity}, {invoice.shipCountry}
                  {invoice.shipPostalCode && (
                    <>
                      <br />
                      {invoice.shipPostalCode}
                    </>
                  )}
                </p>
              </section>
            </div>
            <div className="table-scroll">
              <table>
                <thead>
                  <tr>
                    <th>Producto</th>
                    <th className="numeric">Cantidad</th>
                    <th className="numeric">Precio</th>
                    <th className="numeric">Importe</th>
                  </tr>
                </thead>
                <tbody>
                  {invoice.details.map((line) => (
                    <tr key={line.productId}>
                      <td>{line.productName}</td>
                      <td className="numeric">{line.quantity}</td>
                      <td className="numeric">{money(line.unitPrice)}</td>
                      <td className="numeric">{money(line.amount)}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
            <dl className="amounts invoice-amounts">
              <div>
                <dt>Subtotal</dt>
                <dd>{money(invoice.subtotal)}</dd>
              </div>
              <div>
                <dt>Descuento ({invoice.discountPercent} %)</dt>
                <dd>−{money(invoice.discountAmount)}</dd>
              </div>
              <div>
                <dt>Base imponible</dt>
                <dd>{money(invoice.taxableAmount)}</dd>
              </div>
              <div>
                <dt>IVA ({invoice.vatRate * 100} %)</dt>
                <dd>{money(invoice.vatAmount)}</dd>
              </div>
              <div className="grand-total">
                <dt>Total</dt>
                <dd>{money(invoice.total)}</dd>
              </div>
            </dl>
            <p className="muted">Atendido por {invoice.createdBy}</p>
          </article>
          {url && (
            <div className={pdf ? 'pdf-container' : 'pdf-container hidden'}>
              <Suspense
                fallback={
                  <p className="empty" role="status">
                    Preparando visor PDF…
                  </p>
                }
              >
                <PdfDocument url={url} onReady={setPdfReady} />
              </Suspense>
            </div>
          )}
        </>
      )}
    </Modal>
  )
}
