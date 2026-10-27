import { useEffect, useRef, useState } from 'react'
import { Link } from 'react-router-dom'
import { api } from './api/client'
import { CatalogLookup } from './CatalogLookup'
import { Field, SearchIcon } from './components'
import { useFailure, useNotice, useSession } from './context'
import {
  changeProduct,
  defaultShipping,
  issueMessages,
  money,
  newLine,
  orderIssues,
  orderPayload,
  totals,
  ValidationFailure,
} from './domain/rules'
import type { CartLine, Customer, Product, SalesSettings, Shipping } from './domain/types'
import { InvoicePreview } from './InvoicePreview'

export function OrderPage() {
  const { session } = useSession()
  const { show, confirm } = useNotice()
  const failure = useFailure()
  const failureRef = useRef(failure)
  failureRef.current = failure
  const [settings, setSettings] = useState<SalesSettings | null>(null)
  const [customer, setCustomer] = useState<Customer | null>(null)
  const [shipping, setShipping] = useState(defaultShipping)
  const [lines, setLines] = useState<CartLine[]>([])
  const [lookup, setLookup] = useState<{ products: boolean; row?: string } | null>(null)
  const [busy, setBusy] = useState(false)
  const pending = useRef(false)
  const [saved, setSaved] = useState<number | null>(null)
  const [preview, setPreview] = useState<number | null>(null)
  useEffect(() => {
    const controller = new AbortController()
    void api
      .get<SalesSettings>('/sales/settings', session!.accessToken, controller.signal)
      .then((result) => {
        if (!controller.signal.aborted) setSettings(result)
      })
      .catch((error) => {
        if (!controller.signal.aborted) failureRef.current(error)
      })
    return () => controller.abort()
  }, [session])
  const amounts = totals(lines, settings ?? { discountPercent: 0, vatRate: 0 })
  const updateShipping = (field: keyof Shipping) => (value: string) =>
    setShipping((previous) => ({ ...previous, [field]: value }))
  function select(item: Customer | Product) {
    try {
      if ('currentBalance' in item) setCustomer(item)
      else if (lookup?.row) setLines(changeProduct(lines, lookup.row, item))
      else {
        if (lines.some((line) => line.product.id === item.id))
          throw new ValidationFailure([
            'Este producto ya está en la orden. Modifica su cantidad en la línea existente.',
          ])
        setLines((previous) => [...previous, newLine(item)])
      }
      setLookup(null)
    } catch (error) {
      failure(error)
    }
  }
  async function save() {
    if (pending.current || saved !== null) return
    if (!settings) {
      show(['No se pudo cargar la configuración de ventas. Recarga la página antes de guardar.'])
      return
    }
    const issues = orderIssues(customer, lines, shipping)
    if (issues.length) {
      show(issueMessages(issues))
      return
    }
    pending.current = true
    setBusy(true)
    try {
      const id = await api.post<number>(
        '/CreateOrder',
        orderPayload(customer!, lines, shipping),
        session!.accessToken,
      )
      setSaved(id)
      setPreview(id)
    } catch (error) {
      failure(error)
    } finally {
      pending.current = false
      setBusy(false)
    }
  }
  return (
    <>
      <div className="page-heading">
        <div>
          <p className="eyebrow">Ventas / Nueva orden</p>
          <h1>Nueva venta</h1>
          <p className="muted">Prepara el pedido y revisa los importes antes de guardar.</p>
        </div>
        <Link className="text-link" to="/invoices">
          Consultar facturas →
        </Link>
      </div>
      {saved !== null && (
        <section className="saved-notice" role="status">
          <div>
            <strong>Venta #{saved} registrada</strong>
            <p>La transacción se completó y las existencias se actualizaron.</p>
          </div>
          <div className="button-row">
            <button className="secondary" onClick={() => setPreview(saved)}>
              Ver factura
            </button>
            <button
              onClick={() => {
                setSaved(null)
                setCustomer(null)
                setLines([])
                setShipping(defaultShipping())
              }}
            >
              Nueva orden
            </button>
          </div>
        </section>
      )}
      <form
        noValidate
        className="order-layout"
        onSubmit={(event) => {
          event.preventDefault()
          void save()
        }}
      >
        <fieldset className="order-fields" disabled={busy || saved !== null}>
          <section className="panel">
            <h2>Cliente y entrega</h2>
            <div className="customer-control">
              <label className="field">
                <span>Cliente</span>
                <div className="input-action">
                  <input
                    aria-label="Cliente seleccionado"
                    readOnly
                    value={customer ? `${customer.id} · ${customer.name}` : ''}
                    placeholder="Selecciona un cliente"
                  />
                  <button
                    type="button"
                    className="icon-button"
                    aria-label="Buscar cliente"
                    onClick={() => setLookup({ products: false })}
                  >
                    <SearchIcon />
                  </button>
                </div>
              </label>
            </div>
            <div className="shipping-grid">
              <Field
                label="Dirección de entrega"
                value={shipping.shipAddress}
                onChange={updateShipping('shipAddress')}
                rule="address"
                limit={60}
                hint="Máximo 60 caracteres. Símbolos: # . , / -"
                autoComplete="street-address"
              />
              <Field
                label="Ciudad"
                value={shipping.shipCity}
                onChange={updateShipping('shipCity')}
                rule="letters"
                limit={15}
                hint="De 3 a 15 letras y espacios."
                autoComplete="address-level2"
              />
              <Field
                label="País"
                value={shipping.shipCountry}
                onChange={updateShipping('shipCountry')}
                rule="letters"
                limit={15}
                hint="De 3 a 15 letras y espacios."
                autoComplete="country-name"
              />
              <Field
                label="Código postal (opcional)"
                value={shipping.shipPostalCode}
                onChange={updateShipping('shipPostalCode')}
                rule="digits"
                inputMode="numeric"
                limit={10}
                hint="Hasta 10 dígitos."
                autoComplete="postal-code"
              />
            </div>
          </section>
          <section className="panel products-panel">
            <div className="section-heading">
              <h2>
                Productos <span className="count">{lines.length}</span>
              </h2>
              <button type="button" onClick={() => setLookup({ products: true })}>
                + Agregar producto
              </button>
            </div>
            <div className="table-scroll">
              <table>
                <thead>
                  <tr>
                    <th>Producto</th>
                    <th className="numeric">Precio</th>
                    <th>Cantidad</th>
                    <th className="numeric">Importe</th>
                    <th>Acciones</th>
                  </tr>
                </thead>
                <tbody>
                  {lines.map((line) => (
                    <tr key={line.id}>
                      <td>
                        <strong>{line.product.name}</strong>
                        <small className="stock-label">
                          Código {line.product.id} · Stock {line.product.unitsInStock}
                        </small>
                      </td>
                      <td className="numeric">{money(line.product.unitPrice)}</td>
                      <td className="quantity-cell">
                        <Field
                          label={`Cantidad de ${line.product.name}`}
                          aria-label={`Cantidad de ${line.product.name}`}
                          className="quantity"
                          value={line.quantity}
                          onChange={(value) =>
                            setLines((previous) =>
                              previous.map((item) =>
                                item.id === line.id ? { ...item, quantity: value } : item,
                              ),
                            )
                          }
                          rule="digits"
                          inputMode="numeric"
                          limit={5}
                          ceiling={Math.min(32767, line.product.unitsInStock)}
                          hint={`Entero de 1 a ${Math.min(32767, line.product.unitsInStock)}.`}
                        />
                      </td>
                      <td className="numeric">
                        {money(totals([line], { discountPercent: 0, vatRate: 0 }).subtotal)}
                      </td>
                      <td>
                        <div className="line-actions">
                          <button
                            type="button"
                            className="secondary compact"
                            onClick={() => setLookup({ products: true, row: line.id })}
                          >
                            Cambiar
                          </button>
                          <button
                            type="button"
                            className="danger-text compact"
                            onClick={async () => {
                              if (
                                await confirm(`¿Quieres quitar ${line.product.name} de esta venta?`)
                              )
                                setLines((previous) =>
                                  previous.filter((item) => item.id !== line.id),
                                )
                            }}
                          >
                            Quitar
                          </button>
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
              {!lines.length && (
                <div className="empty">
                  <h3>Agrega el primer producto</h3>
                  <p>Solo verás artículos con stock disponible.</p>
                </div>
              )}
            </div>
          </section>
        </fieldset>
        <aside className="panel order-summary">
          <p className="eyebrow">Resumen de venta</p>
          <h2>Importes</h2>
          {!settings && <p role="status">Cargando configuración…</p>}
          <dl className="amounts">
            <div>
              <dt>Subtotal</dt>
              <dd>{money(amounts.subtotal)}</dd>
            </div>
            <div>
              <dt>Descuento ({settings?.discountPercent ?? '…'} %)</dt>
              <dd>−{money(amounts.discountAmount)}</dd>
            </div>
            <div>
              <dt>Base imponible</dt>
              <dd>{money(amounts.taxableAmount)}</dd>
            </div>
            <div>
              <dt>IVA ({settings ? settings.vatRate * 100 : '…'} %)</dt>
              <dd>{money(amounts.vatAmount)}</dd>
            </div>
            <div className="grand-total">
              <dt>Total</dt>
              <dd>{money(amounts.total)}</dd>
            </div>
          </dl>
          <button type="submit" className="full" disabled={busy || saved !== null || !settings}>
            {busy ? 'Guardando venta…' : saved !== null ? 'Venta registrada' : 'Guardar venta'}
          </button>
          <p className="summary-help">
            Al guardar se confirma la venta y se descuenta el inventario disponible.
          </p>
        </aside>
      </form>
      {lookup && (
        <CatalogLookup
          products={lookup.products}
          onClose={() => setLookup(null)}
          onSelect={select}
        />
      )}{' '}
      {preview !== null && <InvoicePreview id={preview} onClose={() => setPreview(null)} />}
    </>
  )
}
