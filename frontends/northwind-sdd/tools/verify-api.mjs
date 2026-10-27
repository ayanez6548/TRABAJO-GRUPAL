// Explicitly opt in: this creates fictitious users and sales in an isolated database.
import assert from 'node:assert/strict'
import { writeFile } from 'node:fs/promises'
import { Decimal } from 'decimal.js'
const [base, confirmation, reportPath] = process.argv.slice(2)
if (!base || confirmation !== '--isolated-test-database') throw new Error('Usage: node tools/verify-api.mjs http://127.0.0.1:5300/api --isolated-test-database [report.json]')
const origin = new URL(base)
if (!['127.0.0.1', 'localhost'].includes(origin.hostname)) throw new Error('Only a loopback test API is allowed')
const results = []
async function request(path, method = 'GET', body, token) {
  return fetch(base + path, { method, headers: { ...(body ? { 'Content-Type': 'application/json' } : {}), ...(token ? { Authorization: `Bearer ${token}` } : {}) }, ...(body ? { body: JSON.stringify(body) } : {}) })
}
const check = async (name, run) => { await run(); results.push({ name, passed: true }); console.log('PASS ' + name) }
const suffix = Date.now().toString(36)
const account = { firstName: 'Prueba', lastName: 'Integración', email: `sdd.${suffix}@example.test`, password: 'ApiSdd!UnaClaveLarga2026DePrueba' }
let token, otherToken, products, orderId
await check('RF-01/02 registro, duplicado y contraseña mayor a 30', async () => {
  assert.equal((await request('/user/Register', 'POST', account)).status, 200)
  assert.equal((await request('/user/Register', 'POST', account)).status, 400)
  const login = await request('/user/Login', 'POST', { email: account.email, password: account.password })
  assert.equal(login.status, 200); token = (await login.json()).accessToken; assert.ok(token)
})
await check('RF-17 API protegida exige sesión', async () => { assert.equal((await request('/orders')).status, 401) })
await check('RF-04/05/06 catálogos, filtros combinados y stock positivo', async () => {
  const customers = await (await request('/catalog/customers/search?code=ALF&name=Alfred&minBalance=0&maxBalance=0&page=1&pageSize=5', 'GET', undefined, token)).json()
  assert.equal(customers.items.length, 1); assert.equal(customers.items[0].id, 'ALFKI')
  const debtors = await (await request('/catalog/customers/search?minBalance=1&page=1&pageSize=5', 'GET', undefined, token)).json(); assert.ok(debtors.items.some(item => item.currentBalance > 0))
  products = (await (await request('/catalog/products/search?page=1&pageSize=20', 'GET', undefined, token)).json()).items
  assert.ok(products.length >= 4); assert.ok(products.every(item => item.unitsInStock > 0)); assert.ok(!products.some(item => item.id === 2))
  const filtered = await (await request('/catalog/products/search?name=Chai&minPrice=30&maxPrice=40&minStock=1&page=1&pageSize=5', 'GET', undefined, token)).json()
  assert.ok(filtered.items.length); assert.ok(filtered.items.every(item => item.name.includes('Chai') && item.unitPrice >= 30 && item.unitPrice <= 40))
})
const payload = () => ({ customerId: 'ALFKI', shipAddress: 'Av. Central 123', shipCity: 'Quito', shipCountry: 'Ecuador', shipPostalCode: '001234', orderDetails: [{ productId: products[0].id, unitPrice: products[0].unitPrice, quantity: 1 }] })
await check('RF-08/14 servidor rechaza cantidad, duplicado, deuda y precio alterado', async () => {
  let body = payload(); body.orderDetails[0].quantity = products[0].unitsInStock + 1; assert.ok([400, 409].includes((await request('/CreateOrder', 'POST', body, token)).status))
  body = payload(); body.orderDetails.push({ ...body.orderDetails[0] }); assert.equal((await request('/CreateOrder', 'POST', body, token)).status, 400)
  body = payload(); body.customerId = 'ANTON'; assert.equal((await request('/CreateOrder', 'POST', body, token)).status, 400)
  body = payload(); body.orderDetails[0].unitPrice += 1; assert.equal((await request('/CreateOrder', 'POST', body, token)).status, 409)
})
await check('RF-12/13/14 venta real, inventario y totales guardados', async () => {
  const response = await request('/CreateOrder', 'POST', payload(), token); assert.equal(response.status, 200); orderId = await response.json(); assert.ok(Number.isInteger(orderId))
  const invoice = await (await request(`/orders/${orderId}`, 'GET', undefined, token)).json()
  assert.equal(invoice.shipPostalCode, '001234'); assert.equal(invoice.details.length, 1)
  const settings = await (await request('/sales/settings', 'GET', undefined, token)).json()
  const cents = value => value.toDecimalPlaces(2, Decimal.ROUND_HALF_UP)
  const price = new Decimal(products[0].unitPrice)
  const discount = cents(price.times(settings.discountPercent).dividedBy(100))
  const taxable = price.minus(discount)
  assert.equal(invoice.total, taxable.plus(cents(taxable.times(settings.vatRate))).toNumber())
  const after = (await (await request(`/catalog/products/search?code=${products[0].id}&page=1&pageSize=5`, 'GET', undefined, token)).json()).items[0]
  assert.equal(after.unitsInStock, products[0].unitsInStock - 1)
})
await check('RF-15 historial paginado de ventas reales', async () => {
  for (let i = 0; i < 5; i++) assert.equal((await request('/CreateOrder', 'POST', payload(), token)).status, 200)
  const first = await (await request('/orders?page=1&pageSize=5', 'GET', undefined, token)).json()
  const second = await (await request('/orders?page=2&pageSize=5', 'GET', undefined, token)).json()
  assert.equal(first.items.length, 5); assert.equal(second.items.length, 1); assert.equal(first.totalCount, 6)
  assert.ok(!first.items.some(a => second.items.some(b => a.id === b.id)))
})
await check('RF-16 PDF binario real', async () => {
  const response = await request(`/orders/${orderId}/invoice.pdf`, 'GET', undefined, token)
  assert.equal(response.status, 200); assert.ok(response.headers.get('content-type').includes('application/pdf'))
  const bytes = Buffer.from(await response.arrayBuffer()); assert.equal(bytes.subarray(0, 4).toString(), '%PDF'); assert.ok(bytes.length > 1000)
})
await check('RF-17 aislamiento de facturas entre cuentas y 404', async () => {
  const other = { ...account, email: `other.${suffix}@example.test` }; assert.equal((await request('/user/Register', 'POST', other)).status, 200)
  otherToken = (await (await request('/user/Login', 'POST', { email: other.email, password: other.password })).json()).accessToken
  assert.equal((await request(`/orders/${orderId}`, 'GET', undefined, otherToken)).status, 404)
  assert.equal((await request(`/orders/${orderId}/invoice.pdf`, 'GET', undefined, otherToken)).status, 404)
  const history = await (await request('/orders?page=1&pageSize=5', 'GET', undefined, otherToken)).json(); assert.equal(history.totalCount, 0)
  assert.equal((await request('/orders/2147483647', 'GET', undefined, token)).status, 404)
})
const report = { date: new Date().toISOString(), base, environment: 'Windows, .NET 10, isolated LocalDB NorthWindSddReview20261026; PickupDirectory', results, passed: results.length }
if (reportPath) await writeFile(reportPath, JSON.stringify(report, null, 2))
console.log(`${results.length} verificaciones de integración correctas. No se guardaron tokens ni contraseñas en el informe.`)
