import { useEffect, useRef, useState } from 'react'
import { api, cancelled } from './api/client'
import { Field, Pagination, SearchIcon } from './components'
import { Modal, useFailure, useNotice, useSession } from './context'
import { emptyFilters, filterQuery, money } from './domain/rules'
import type { Customer, Filters, Page, Product } from './domain/types'

export function CatalogLookup({
  products,
  onClose,
  onSelect,
}: {
  products: boolean
  onClose: () => void
  onSelect: (item: Customer | Product) => void
}) {
  const { session } = useSession()
  const { show } = useNotice()
  const failure = useFailure()
  const [filters, setFilters] = useState(emptyFilters)
  const [field, setField] = useState('name')
  const [page, setPage] = useState(1)
  const [size, setSize] = useState(5)
  const [refresh, setRefresh] = useState(0)
  const [data, setData] = useState<Page<Customer | Product> | null>(null)
  const [busy, setBusy] = useState(true)
  const request = useRef(0)
  const failureRef = useRef(failure)
  failureRef.current = failure
  useEffect(() => {
    const controller = new AbortController()
    const revision = ++request.current
    setBusy(true)
    const timer = setTimeout(async () => {
      try {
        const query = filterQuery(products, filters, page, size)
        const result = await api.get<Page<Customer | Product>>(
          `/catalog/${products ? 'products' : 'customers'}/search?${query}`,
          session!.accessToken,
          controller.signal,
        )
        if (revision === request.current && !controller.signal.aborted) setData(result)
      } catch (error) {
        if (!cancelled(error) && revision === request.current && !controller.signal.aborted) {
          setData(null)
          failureRef.current(error)
        }
      } finally {
        if (revision === request.current && !controller.signal.aborted) setBusy(false)
      }
    }, 400)
    return () => {
      clearTimeout(timer)
      controller.abort()
    }
  }, [products, filters, page, size, refresh, session])
  const update = (key: keyof Filters) => (value: string) => {
    setFilters((previous) => ({ ...previous, [key]: value }))
    setPage(1)
  }
  const labels: Record<keyof Filters, string> = {
    code: 'Código',
    name: 'Nombre',
    minimum: products ? 'Precio desde' : 'Saldo desde',
    maximum: products ? 'Precio hasta' : 'Saldo hasta',
    minStock: 'Stock desde',
    maxStock: 'Stock hasta',
  }
  function select(item: Customer | Product) {
    if ('currentBalance' in item && item.currentBalance > 0) {
      show(
        ['Este cliente tiene un saldo pendiente. Selecciona otro cliente para continuar.'],
        'Cliente con deuda',
      )
      return
    }
    onSelect(item)
  }
  return (
    <Modal title={products ? 'Seleccionar producto' : 'Seleccionar cliente'} onClose={onClose}>
      <p className="muted lookup-intro">
        {products
          ? 'Se muestran únicamente productos con existencias.'
          : 'Busca un cliente para esta venta.'}{' '}
        Los resultados se actualizan al escribir.
      </p>
      <form
        className="filter-bar"
        noValidate
        onSubmit={(event) => {
          event.preventDefault()
          setRefresh((value) => value + 1)
        }}
      >
        <label className="field">
          <span>Filtrar por</span>
          <select value={field} onChange={(event) => setField(event.target.value)}>
            <option value="name">Nombre</option>
            <option value="code">Código</option>
            <option value="amount">{products ? 'Precio' : 'Saldo'}</option>
            {products && <option value="stock">Existencias</option>}
          </select>
        </label>
        <div className="filter-inputs">
          {field === 'name' && (
            <Field
              label="Nombre"
              autoFocus
              value={filters.name}
              onChange={update('name')}
              limit={100}
              placeholder={products ? 'Ej.: Chai' : 'Ej.: Alfreds'}
            />
          )}{' '}
          {field === 'code' && (
            <Field
              label="Código"
              value={filters.code}
              onChange={(value) => {
                if (!products && !/^[a-zA-Z0-9]*$/.test(value)) {
                  show(['Código: usa solo letras y números.'])
                  return
                }
                update('code')(value)
              }}
              rule={products ? 'digits' : 'text'}
              limit={products ? 10 : 5}
              ceiling={products ? 2147483647 : undefined}
              placeholder={products ? 'Ej.: 1' : 'Ej.: ALFKI'}
            />
          )}{' '}
          {field === 'amount' && (
            <>
              <Field
                label={products ? 'Precio mínimo' : 'Saldo mínimo'}
                value={filters.minimum}
                onChange={update('minimum')}
                rule="money"
                inputMode="decimal"
                placeholder="Desde"
              />
              <Field
                label={products ? 'Precio máximo' : 'Saldo máximo'}
                value={filters.maximum}
                onChange={update('maximum')}
                rule="money"
                inputMode="decimal"
                placeholder="Hasta"
              />
            </>
          )}
          {field === 'stock' && (
            <>
              <Field
                label="Existencias mínimas"
                value={filters.minStock}
                onChange={update('minStock')}
                rule="digits"
                limit={5}
                ceiling={32767}
                inputMode="numeric"
                placeholder="Desde"
              />
              <Field
                label="Existencias máximas"
                value={filters.maxStock}
                onChange={update('maxStock')}
                rule="digits"
                limit={5}
                ceiling={32767}
                inputMode="numeric"
                placeholder="Hasta"
              />
            </>
          )}
        </div>
        <button type="submit" className="icon-button" aria-label="Buscar ahora">
          <SearchIcon />
        </button>
        <button
          type="button"
          className="secondary"
          onClick={() => {
            setFilters(emptyFilters())
            setPage(1)
          }}
        >
          Limpiar
        </button>
      </form>
      {Object.entries(filters).some(([, value]) => value) && (
        <div className="chips" aria-label="Filtros activos">
          {Object.entries(filters)
            .filter(([, value]) => value)
            .map(([key, value]) => (
              <button
                className="chip"
                key={key}
                onClick={() => update(key as keyof Filters)('')}
                aria-label={`Quitar filtro ${labels[key as keyof Filters]}`}
              >
                {labels[key as keyof Filters]}: {value} <span aria-hidden="true">×</span>
              </button>
            ))}
        </div>
      )}
      <div className="table-scroll" aria-busy={busy}>
        <table>
          <thead>
            <tr>
              <th>Código</th>
              <th>Nombre</th>
              <th>{products ? 'Precio' : 'Saldo'}</th>
              {products && <th>Existencias</th>}
              <th>
                <span className="sr-only">Acción</span>
              </th>
            </tr>
          </thead>
          <tbody>
            {!busy &&
              data?.items.map((item) => (
                <tr key={item.id}>
                  <td className="code">{item.id}</td>
                  <td>{item.name}</td>
                  <td className="numeric">
                    {money('unitPrice' in item ? item.unitPrice : item.currentBalance)}
                  </td>
                  {'unitsInStock' in item && <td className="numeric">{item.unitsInStock}</td>}
                  <td>
                    <button
                      className="compact"
                      aria-label={`Seleccionar ${item.name}`}
                      onClick={() => select(item)}
                    >
                      Seleccionar
                    </button>
                  </td>
                </tr>
              ))}
          </tbody>
        </table>
        {busy ? (
          <p className="empty" role="status">
            Buscando…
          </p>
        ) : (
          !data?.items.length && <p className="empty">No hay resultados para estos filtros.</p>
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
    </Modal>
  )
}
