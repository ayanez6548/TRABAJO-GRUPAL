import { describe, expect, it } from 'vitest'
import {
  accountIssues,
  shippingIssues,
  orderIssues,
  isAllowedInput,
  filterQuery,
  totals,
  changeProduct,
  newLine,
} from './rules'
import { readSession } from './session'

const customer = { id: 'ALFKI', name: 'Cliente de prueba', currentBalance: 0 }
const product = { id: 1, name: 'Chai', unitPrice: 1.11, unitsInStock: 20 }
const shipping = {
  shipAddress: 'Calle Principal 123',
  shipCity: 'Quito',
  shipCountry: 'Ecuador',
  shipPostalCode: '00170101',
}
const account = {
  firstName: 'María José',
  lastName: 'Muñoz',
  email: 'ana+ventas@example.test',
  password: 'Clave123!',
  confirmPassword: 'Clave123!',
}

describe('RF-18 cuenta y restricciones de entrada', () => {
  it('acepta letras Unicode y rechaza números en nombres, sin alterar el dato', () => {
    expect(accountIssues(account)).toEqual([])
    expect(
      accountIssues({ ...account, firstName: 'Ana1' }).some((issue) => issue.field === 'firstName'),
    ).toBe(true)
    expect(isAllowedInput('letters', 'Muñoz123', 30)).toBe(false)
    expect(isAllowedInput('letters', 'Peña', 30)).toBe(true)
    expect(isAllowedInput('letters', 'A'.repeat(31), 30)).toBe(false)
  })
  it.each([
    'ana!@example.test',
    'ana@@example.test',
    '.ana@example.test',
    'ana..x@example.test',
    'ana@-example.test',
    'ana@localhost',
    'ana x@example.test',
  ])('rechaza correo inválido: %s', (email) =>
    expect(accountIssues({ ...account, email }).some((issue) => issue.field === 'email')).toBe(
      true,
    ),
  )
  it('conserva el límite de correo de 30 caracteres', () => {
    expect(
      accountIssues({ ...account, email: 'a'.repeat(18) + '@example.com' }).some(
        (issue) => issue.field === 'email',
      ),
    ).toBe(false)
    expect(
      accountIssues({ ...account, email: 'a'.repeat(19) + '@example.com' }).some(
        (issue) => issue.field === 'email',
      ),
    ).toBe(true)
  })
  it.each([31, 64, 128, 1024])('admite contraseñas de %i caracteres sin truncar', (length) => {
    const password = 'Aa1!' + 'x'.repeat(length - 4)
    expect(accountIssues({ ...account, password, confirmPassword: password })).toEqual([])
  })
  it('exige composición y coincidencia de contraseña', () => {
    expect(
      accountIssues({ ...account, password: 'abc' }).some((issue) => issue.field === 'password'),
    ).toBe(true)
    expect(
      accountIssues({ ...account, confirmPassword: 'Diferente123!' }).some(
        (issue) => issue.field === 'confirmPassword',
      ),
    ).toBe(true)
  })
})

describe('RF-08/09/11 borrador, inventario y envío', () => {
  it('acepta ceros iniciales postales y rechaza ciudad corta, signos y dirección vacía', () => {
    expect(shippingIssues(shipping)).toEqual([])
    expect(
      shippingIssues({ ...shipping, shipCity: ' Q ', shipAddress: ' ', shipCountry: 'Ecuador1' }),
    ).toHaveLength(3)
    expect(
      shippingIssues({ ...shipping, shipPostalCode: '012A' }).some(
        (issue) => issue.field === 'shipPostalCode',
      ),
    ).toBe(true)
  })
  it('requiere cliente, al menos una línea y cliente sin deuda', () => {
    expect(orderIssues(null, [], shipping).some((issue) => issue.field === 'customer')).toBe(true)
    expect(orderIssues(customer, [], shipping).some((issue) => issue.field === 'lines')).toBe(true)
    expect(
      orderIssues({ ...customer, currentBalance: 10 }, [newLine(product, 'a')], shipping).some(
        (issue) => issue.field === 'customer',
      ),
    ).toBe(true)
  })
  it.each(['', '0', '-1', '1.5', '21', '32768'])('rechaza cantidad inválida %s', (quantity) => {
    expect(
      orderIssues(customer, [{ ...newLine(product, 'a'), quantity }], shipping).some(
        (issue) => issue.field === 'quantity',
      ),
    ).toBe(true)
  })
  it('rechaza duplicados aunque el payload se prepare fuera de la ventana', () => {
    expect(
      orderIssues(customer, [newLine(product, 'a'), newLine(product, 'b')], shipping).some(
        (issue) => issue.field === 'lines',
      ),
    ).toBe(true)
  })
  it('cambia el producto conservando cantidad y otras líneas; no oculta stock insuficiente', () => {
    const lines = [
      { ...newLine(product, 'a'), quantity: '10' },
      newLine({ ...product, id: 2 }, 'b'),
    ]
    const changed = changeProduct(lines, 'a', { ...product, id: 3, unitsInStock: 3 })
    expect(changed[0].quantity).toBe('10')
    expect(changed[1]).toEqual(lines[1])
    expect(
      orderIssues(customer, changed, shipping).some((issue) => issue.field === 'quantity'),
    ).toBe(true)
    expect(() => changeProduct(lines, 'a', { ...product, id: 2 })).toThrow()
  })
})

describe('RF-06/07 filtros y RF-12 redondeo', () => {
  it('combina filtros por campos, omite vacíos y envía decimales con punto', () => {
    const query = new URLSearchParams(
      filterQuery(
        false,
        {
          code: 'ALF',
          name: 'Alfreds',
          minimum: '0,50',
          maximum: '100',
          minStock: '',
          maxStock: '',
        },
        2,
        5,
      ),
    )
    expect(query.get('code')).toBe('ALF')
    expect(query.get('name')).toBe('Alfreds')
    expect(query.get('minBalance')).toBe('0.5')
    expect(query.get('page')).toBe('2')
    expect(query.has('minStock')).toBe(false)
  })
  it('rechaza rango invertido y existencias no enteras', () => {
    expect(() =>
      filterQuery(
        true,
        { code: '', name: '', minimum: '20', maximum: '10', minStock: '', maxStock: '' },
        1,
        5,
      ),
    ).toThrow()
    expect(() =>
      filterQuery(
        true,
        { code: '', name: '', minimum: '', maximum: '', minStock: '1.5', maxStock: '' },
        1,
        5,
      ),
    ).toThrow()
  })
  it('coincide con el redondeo AwayFromZero positivo del backend', () => {
    const result = totals([{ ...newLine(product, 'a'), quantity: '3' }], {
      discountPercent: 10,
      vatRate: 0.15,
    })
    expect(result).toEqual({
      subtotal: 3.33,
      discountAmount: 0.33,
      taxableAmount: 3,
      vatAmount: 0.45,
      total: 3.45,
    })
    expect(
      totals([newLine({ ...product, unitPrice: 75 }, 'a')], { discountPercent: 10, vatRate: 0.15 })
        .total,
    ).toBe(77.63)
  })
})

describe('RF-03/21 sesión local', () => {
  it('descarta sesiones vencidas o corruptas', () => {
    expect(readSession('not-json', Date.now())).toBeNull()
    expect(
      readSession(
        JSON.stringify({ accessToken: 'token', expiresAt: '2000-01-01T00:00:00Z' }),
        Date.now(),
      ),
    ).toBeNull()
    expect(
      readSession(
        JSON.stringify({ accessToken: '', expiresAt: '2099-01-01T00:00:00Z' }),
        Date.now(),
      ),
    ).toBeNull()
  })
})
