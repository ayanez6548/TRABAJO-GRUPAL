import { useEffect, useRef, useState } from 'react'
import { Link } from 'react-router-dom'
import { api } from './api/client'
import { Field, Pagination, SearchIcon } from './components'
import { useFailure, useNotice, useSession } from './context'
import { dateTime, money, orderNumber } from './domain/rules'
import type { InvoiceSummary, Page } from './domain/types'
import { InvoicePreview } from './InvoicePreview'

export function InvoicesPage() {
  const { session } = useSession()
  const { show } = useNotice()
  const failure = useFailure()
  const failureRef = useRef(failure)
  failureRef.current = failure
  const [data, setData] = useState<Page<InvoiceSummary> | null>(null)
  const [page, setPage] = useState(1)
  const [size, setSize] = useState(5)
  const [refresh, setRefresh] = useState(0)
  const [busy, setBusy] = useState(true)
  const [number, setNumber] = useState('')
  const [invoiceId, setInvoiceId] = useState<number | null>(null)
  useEffect(() => {
    const controller = new AbortController()
    setBusy(true)
    void api
      .get<Page<InvoiceSummary>>(
        `/orders?page=${page}&pageSize=${size}`,
        session!.accessToken,
        controller.signal,
      )
      .then((result) => {
        if (!controller.signal.aborted) {
          if (page > Math.max(1, result.totalPages)) setPage(Math.max(1, result.totalPages))
          else setData(result)
        }
      })
      .catch((error) => {
        if (!controller.signal.aborted) {
          setData(null)
          failureRef.current(error)
        }
      })
      .finally(() => {
        if (!controller.signal.aborted) setBusy(false)
      })
    return () => controller.abort()
  }, [page, size, refresh, session])
  return (
    <>
      <div className="page-heading">
        <div>
          <p className="eyebrow">Ventas / Comprobantes</p>
          <h1>Mis facturas</h1>
          <p className="muted">Consulta las ventas que has registrado y abre su comprobante.</p>
        </div>
        <Link className="button" to="/orders/new">
          Nueva venta
        </Link>
      </div>
      <section className="panel">
        <div className="history-toolbar">
          <form
            className="button-row"
            noValidate
            onSubmit={(event) => {
              event.preventDefault()
              const id = orderNumber(number)
              if (id) setInvoiceId(id)
              else show(['Ingresa un número de factura entero entre 1 y 2147483647.'])
            }}
          >
            <Field
              label="Número de factura"
              value={number}
              onChange={setNumber}
              rule="digits"
              inputMode="numeric"
              limit={10}
              ceiling={2147483647}
              placeholder="Ej.: 1"
            />
            <button type="submit" className="icon-button" aria-label="Buscar factura">
              <SearchIcon />
            </button>
          </form>
          <button
            className="secondary"
            disabled={busy}
            onClick={() => setRefresh((value) => value + 1)}
          >
            Actualizar historial
          </button>
        </div>
        <div className="table-scroll" aria-busy={busy}>
          <table>
            <thead>
              <tr>
                <th>Factura</th>
                <th>Fecha</th>
                <th>Cliente</th>
                <th className="numeric">Productos</th>
                <th className="numeric">Total</th>
                <th>
                  <span className="sr-only">Acción</span>
                </th>
              </tr>
            </thead>
            <tbody>
              {!busy &&
                data?.items.map((item) => (
                  <tr key={item.id}>
                    <td className="code">#{item.id}</td>
                    <td>{dateTime(item.orderDate)}</td>
                    <td>{item.customerName}</td>
                    <td className="numeric">{item.productsCount}</td>
                    <td className="numeric">{money(item.total)}</td>
                    <td>
                      <button
                        className="secondary compact"
                        onClick={() => setInvoiceId(item.id)}
                        aria-label={`Ver factura ${item.id}`}
                      >
                        Ver factura
                      </button>
                    </td>
                  </tr>
                ))}
            </tbody>
          </table>
          {busy ? (
            <p className="empty" role="status">
              Cargando historial…
            </p>
          ) : (
            !data?.items.length && (
              <p className="empty">Todavía no tienes facturas en esta página.</p>
            )
          )}
        </div>
        <Pagination
          page={page}
          pages={data?.totalPages ?? 1}
          total={data?.totalCount ?? 0}
          size={size}
          onPage={setPage}
          onSize={(value) => {
            setSize(value)
            setPage(1)
          }}
          busy={busy}
        />
      </section>
      {invoiceId !== null && (
        <InvoicePreview key={invoiceId} id={invoiceId} onClose={() => setInvoiceId(null)} />
      )}
    </>
  )
}
