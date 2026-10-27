import { useId } from 'react'
import type { InputHTMLAttributes } from 'react'
import { isAllowedInput } from './domain/rules'
import type { InputRule } from './domain/types'
import { useNotice } from './context'

interface FieldProps extends Omit<InputHTMLAttributes<HTMLInputElement>, 'onChange' | 'value'> {
  label: string
  value: string
  onChange: (value: string) => void
  rule?: InputRule
  hint?: string
  limit?: number
  ceiling?: number
}
export function Field({
  label,
  value,
  onChange,
  rule = 'text',
  hint,
  limit,
  ceiling,
  ...props
}: FieldProps) {
  const id = useId()
  const { show } = useNotice()
  const valid = (candidate: string) =>
    isAllowedInput(rule, candidate, limit) &&
    (ceiling === undefined || !candidate || Number(candidate.replace(',', '.')) <= ceiling)
  const reject = () =>
    show([`${label}: ${hint || 'El valor contiene caracteres no permitidos o supera el límite.'}`])
  return (
    <label className="field" htmlFor={id}>
      <span>{label}</span>
      <input
        {...props}
        id={id}
        value={value}
        aria-describedby={hint ? id + '-hint' : undefined}
        onBeforeInput={(event) => {
          const data = (event.nativeEvent as InputEvent).data
          if (
            data &&
            !valid(
              value.slice(0, event.currentTarget.selectionStart ?? value.length) +
                data +
                value.slice(event.currentTarget.selectionEnd ?? value.length),
            )
          ) {
            event.preventDefault()
            reject()
          }
        }}
        onPaste={(event) => {
          const input = event.currentTarget
          const candidate =
            value.slice(0, input.selectionStart ?? value.length) +
            event.clipboardData.getData('text') +
            value.slice(input.selectionEnd ?? value.length)
          if (!valid(candidate)) {
            event.preventDefault()
            reject()
          }
        }}
        onChange={(event) => {
          if (valid(event.target.value)) onChange(event.target.value)
          else reject()
        }}
      />
      {hint && <small id={id + '-hint'}>{hint}</small>}
    </label>
  )
}
export function SearchIcon() {
  return (
    <svg
      viewBox="0 0 24 24"
      width="19"
      height="19"
      fill="none"
      stroke="currentColor"
      strokeWidth="2"
      aria-hidden="true"
    >
      <circle cx="10.5" cy="10.5" r="6.5" />
      <path d="m16 16 5 5" />
    </svg>
  )
}
export function Pagination({
  page,
  pages,
  total,
  size,
  onPage,
  onSize,
  busy,
}: {
  page: number
  pages: number
  total: number
  size: number
  onPage: (page: number) => void
  onSize: (size: number) => void
  busy?: boolean
}) {
  return (
    <div className="pagination">
      <label>
        Por página{' '}
        <select
          aria-label="Resultados por página"
          value={size}
          disabled={busy}
          onChange={(event) => onSize(Number(event.target.value))}
        >
          {[5, 10, 20].map((value) => (
            <option key={value} value={value}>
              {value}
            </option>
          ))}
        </select>
      </label>
      <span aria-live="polite">
        Página {page} de {Math.max(1, pages)} · {total} resultados
      </span>
      <div className="button-row">
        <button className="secondary" disabled={busy || page <= 1} onClick={() => onPage(page - 1)}>
          Anterior
        </button>
        <button
          className="secondary"
          disabled={busy || page >= pages}
          onClick={() => onPage(page + 1)}
        >
          Siguiente
        </button>
      </div>
    </div>
  )
}
