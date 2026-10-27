import { Decimal } from 'decimal.js'
import type {
  Account,
  Amounts,
  CartLine,
  Customer,
  Filters,
  InputRule,
  Issue,
  Product,
  SalesSettings,
  Shipping,
} from './types'

export class ValidationFailure extends Error {
  messages: string[]
  constructor(messages: string[]) {
    super(messages.join('\n'))
    this.messages = messages
  }
}
const letters = /^ *\p{L}[\p{L}\p{M} ]*$/u
const emailCharacters = /^[a-zA-Z0-9._+@-]*$/
export const money = (value: number) =>
  new Intl.NumberFormat('es-EC', { style: 'currency', currency: 'USD' }).format(value)
export const dateTime = (value: string) =>
  new Intl.DateTimeFormat('es-EC', { dateStyle: 'medium', timeStyle: 'short' }).format(
    new Date(value),
  )
export const emptyFilters = (): Filters => ({
  code: '',
  name: '',
  minimum: '',
  maximum: '',
  minStock: '',
  maxStock: '',
})
export const defaultShipping = (): Shipping => ({
  shipAddress: '',
  shipCity: '',
  shipCountry: '',
  shipPostalCode: '',
})
export const issueMessages = (issues: Issue[]) => [
  ...new Set(issues.map((issue) => `${issue.label}: ${issue.message}`)),
]

export function isAllowedInput(rule: InputRule, value: string, maximum?: number): boolean {
  if (maximum !== undefined && value.length > maximum) return false
  switch (rule) {
    case 'letters':
      return /^[\p{L}\p{M} ]*$/u.test(value)
    case 'email':
      return emailCharacters.test(value) && value.split('@').length <= 2
    case 'address':
      return /^[\p{L}\p{M}\p{N} #.,/\-]*$/u.test(value)
    case 'digits':
      return /^[0-9]*$/.test(value)
    case 'money':
      return /^[0-9]{0,6}([.,][0-9]{0,2})?$/.test(value)
    default:
      return true
  }
}
function emailValid(value: string): boolean {
  if (!value || value.length > 30 || !emailCharacters.test(value)) return false
  const parts = value.split('@')
  if (parts.length !== 2) return false
  const [local, domain] = parts
  if (!local || local.startsWith('.') || local.endsWith('.') || local.includes('..')) return false
  const labels = domain.split('.')
  return (
    labels.length >= 2 &&
    labels.every((label) => /^[a-zA-Z0-9](?:[a-zA-Z0-9-]*[a-zA-Z0-9])?$/.test(label))
  )
}
export function credentialsIssues(email: string, password: string): Issue[] {
  const issues: Issue[] = []
  if (!emailValid(email))
    issues.push({
      field: 'email',
      label: 'Correo electrónico',
      message:
        'Ingresa un correo válido de hasta 30 caracteres, sin espacios y con una sola arroba.',
    })
  if (!password.trim())
    issues.push({ field: 'password', label: 'Contraseña', message: 'Ingresa tu contraseña.' })
  return issues
}
export function accountIssues(account: Account): Issue[] {
  const issues: Issue[] = []
  for (const [field, label] of [
    ['firstName', 'Nombres'],
    ['lastName', 'Apellidos'],
  ] as const) {
    if (account[field].length > 30 || !letters.test(account[field]))
      issues.push({ field, label, message: 'Usa solo letras y espacios, hasta 30 caracteres.' })
  }
  issues.push(...credentialsIssues(account.email, account.password))
  if (
    account.password.trim() &&
    (account.password.length < 6 ||
      !/\p{Ll}/u.test(account.password) ||
      !/\p{Lu}/u.test(account.password) ||
      !/\p{Nd}/u.test(account.password) ||
      !/[^\p{L}\p{N}]/u.test(account.password))
  ) {
    issues.push({
      field: 'password',
      label: 'Contraseña',
      message: 'Usa al menos 6 caracteres con mayúscula, minúscula, número y símbolo.',
    })
  }
  if (account.password !== account.confirmPassword)
    issues.push({
      field: 'confirmPassword',
      label: 'Confirmar contraseña',
      message: 'Las contraseñas no coinciden.',
    })
  return issues
}
export function shippingIssues(shipping: Shipping): Issue[] {
  const issues: Issue[] = []
  if (
    !shipping.shipAddress.trim() ||
    shipping.shipAddress.length > 60 ||
    !isAllowedInput('address', shipping.shipAddress)
  )
    issues.push({
      field: 'shipAddress',
      label: 'Dirección',
      message: 'Ingresa de 1 a 60 caracteres: letras, números, espacios o # . , / -.',
    })
  for (const [field, label] of [
    ['shipCity', 'Ciudad'],
    ['shipCountry', 'País'],
  ] as const) {
    if (
      shipping[field].trim().length < 3 ||
      shipping[field].length > 15 ||
      !letters.test(shipping[field])
    )
      issues.push({
        field,
        label,
        message: 'Ingresa entre 3 y 15 caracteres, solo letras y espacios.',
      })
  }
  if (shipping.shipPostalCode && !/^[0-9]{1,10}$/.test(shipping.shipPostalCode))
    issues.push({
      field: 'shipPostalCode',
      label: 'Código postal',
      message: 'Usa hasta 10 dígitos o deja el campo vacío.',
    })
  return issues
}
export const newLine = (product: Product, id: string = crypto.randomUUID()): CartLine => ({
  id,
  product,
  quantity: '1',
})
export function changeProduct(lines: CartLine[], id: string, product: Product): CartLine[] {
  if (!lines.some((line) => line.id === id))
    throw new ValidationFailure(['La línea seleccionada ya no está disponible.'])
  if (lines.some((line) => line.id !== id && line.product.id === product.id))
    throw new ValidationFailure([
      'Este producto ya está en la orden. Modifica su cantidad en la línea existente.',
    ])
  return lines.map((line) => (line.id === id ? { ...line, product } : line))
}
export function orderIssues(
  customer: Customer | null,
  lines: CartLine[],
  shipping: Shipping,
): Issue[] {
  const issues = shippingIssues(shipping)
  if (!customer || !/^[a-zA-Z0-9]{5}$/.test(customer.id))
    issues.unshift({
      field: 'customer',
      label: 'Cliente',
      message: 'Selecciona un cliente válido.',
    })
  else if (customer.currentBalance > 0)
    issues.unshift({
      field: 'customer',
      label: 'Cliente',
      message: 'El cliente tiene un saldo pendiente.',
    })
  if (!lines.length)
    issues.push({ field: 'lines', label: 'Productos', message: 'Agrega al menos un producto.' })
  if (new Set(lines.map((line) => line.product.id)).size !== lines.length)
    issues.push({
      field: 'lines',
      label: 'Productos',
      message: 'Un producto solo puede aparecer una vez.',
    })
  for (const [index, line] of lines.entries()) {
    const product = line.product
    if (!Number.isInteger(product.id) || product.id <= 0 || product.unitsInStock <= 0)
      issues.push({
        field: 'product',
        label: `Producto ${index + 1}`,
        message: 'Selecciona un producto con existencias.',
      })
    const quantity = Number(line.quantity)
    if (
      !/^[0-9]+$/.test(line.quantity) ||
      !Number.isInteger(quantity) ||
      quantity < 1 ||
      quantity > Math.min(32767, product.unitsInStock)
    )
      issues.push({
        field: 'quantity',
        label: `Cantidad de ${product.name}`,
        message: `Ingresa un entero entre 1 y ${Math.min(32767, product.unitsInStock)}.`,
      })
    if (
      !Number.isFinite(product.unitPrice) ||
      product.unitPrice <= 0 ||
      product.unitPrice > 999999.99 ||
      new Decimal(product.unitPrice).decimalPlaces() > 2
    )
      issues.push({
        field: 'price',
        label: `Precio de ${product.name}`,
        message: 'Actualiza el catálogo para obtener un precio válido.',
      })
  }
  return issues
}
export function totals(lines: CartLine[], settings: SalesSettings): Amounts {
  const round = (value: Decimal) => value.toDecimalPlaces(2, Decimal.ROUND_HALF_UP)
  const subtotal = lines.reduce(
    (sum, line) =>
      sum.plus(
        round(
          new Decimal(line.product.unitPrice).times(
            /^[0-9]+$/.test(line.quantity) ? line.quantity : 0,
          ),
        ),
      ),
    new Decimal(0),
  )
  const discount = round(subtotal.times(settings.discountPercent).dividedBy(100))
  const taxable = subtotal.minus(discount)
  const vat = round(taxable.times(settings.vatRate))
  return {
    subtotal: subtotal.toNumber(),
    discountAmount: discount.toNumber(),
    taxableAmount: taxable.toNumber(),
    vatAmount: vat.toNumber(),
    total: taxable.plus(vat).toNumber(),
  }
}
export function orderPayload(customer: Customer, lines: CartLine[], shipping: Shipping) {
  return {
    customerId: customer.id,
    ...shipping,
    shipPostalCode: shipping.shipPostalCode || null,
    orderDetails: lines.map((line) => ({
      productId: line.product.id,
      unitPrice: line.product.unitPrice,
      quantity: Number(line.quantity),
    })),
  }
}
export function filterQuery(
  products: boolean,
  filters: Filters,
  page: number,
  pageSize: number,
): string {
  const parameters = new URLSearchParams({ page: String(page), pageSize: String(pageSize) })
  const fail = (message: string): never => {
    throw new ValidationFailure([message])
  }
  if (!Number.isInteger(page) || page < 1 || page > 100000 || pageSize < 1 || pageSize > 50)
    fail('Revisa la página y el número de resultados.')
  if (filters.name.length > 100) fail('El nombre admite hasta 100 caracteres.')
  if (filters.name.trim()) parameters.set('name', filters.name.trim())
  if (filters.code) {
    if (
      products
        ? !/^[0-9]{1,10}$/.test(filters.code) ||
          Number(filters.code) < 1 ||
          Number(filters.code) > 2147483647
        : !/^[a-zA-Z0-9]{1,5}$/.test(filters.code)
    )
      fail('El código de búsqueda no es válido.')
    parameters.set('code', filters.code)
  }
  const range = (
    from: string,
    to: string,
    minimumKey: string,
    maximumKey: string,
    stock = false,
  ) => {
    const parse = (value: string): number | undefined => {
      if (!value) return undefined
      if (stock ? !/^[0-9]+$/.test(value) : !/^[0-9]{1,6}(?:[.,][0-9]{0,2})?$/.test(value))
        fail(
          stock
            ? 'Las existencias deben ser números enteros.'
            : 'Usa importes con hasta dos decimales.',
        )
      const result = Number(value.replace(',', '.'))
      if (
        !Number.isFinite(result) ||
        result < (stock ? 1 : 0) ||
        result > (stock ? 32767 : 999999.99)
      )
        fail(
          stock
            ? 'Las existencias deben estar entre 1 y 32767.'
            : 'Los importes deben estar entre 0 y 999999,99.',
        )
      return result
    }
    const first = parse(from),
      last = parse(to)
    if (first !== undefined && last !== undefined && first > last)
      fail('El mínimo no puede superar el máximo.')
    if (first !== undefined) parameters.set(minimumKey, String(first))
    if (last !== undefined) parameters.set(maximumKey, String(last))
  }
  range(
    filters.minimum,
    filters.maximum,
    products ? 'minPrice' : 'minBalance',
    products ? 'maxPrice' : 'maxBalance',
  )
  if (products) range(filters.minStock, filters.maxStock, 'minStock', 'maxStock', true)
  return parameters.toString()
}
export function orderNumber(value: string): number | null {
  const number = Number(value)
  return /^[0-9]{1,10}$/.test(value) &&
    Number.isInteger(number) &&
    number > 0 &&
    number <= 2147483647
    ? number
    : null
}
