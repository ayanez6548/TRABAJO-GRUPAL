import { afterEach, describe, expect, it, vi } from 'vitest'
import { ApiClient, ApiError } from './client'

afterEach(() => vi.unstubAllGlobals())

describe('RF-13/17/21 contrato HTTP', () => {
  it('envía Bearer al historial sin aceptar propietario del navegador', async () => {
    const fetcher = vi
      .fn()
      .mockResolvedValue(
        new Response(
          JSON.stringify({ items: [], page: 1, pageSize: 5, totalCount: 0, totalPages: 0 }),
        ),
      )
    vi.stubGlobal('fetch', fetcher)
    await new ApiClient().get('/orders?page=1&pageSize=5', 'test-token')
    const [url, options] = fetcher.mock.calls[0]
    expect(url).toBe('/api/orders?page=1&pageSize=5')
    expect(new Headers(options.headers).get('Authorization')).toBe('Bearer test-token')
    expect(options.credentials).toBe('omit')
    expect(url).not.toContain('userName')
  })
  it('conserva errores por campo y conflictos de la API', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValue(
        new Response(
          JSON.stringify({
            title: 'Revisa los datos.',
            errors: { Email: ['El correo ya existe.'] },
          }),
          { status: 400 },
        ),
      ),
    )
    await expect(new ApiClient().post('/user/Register', {})).rejects.toMatchObject({
      status: 400,
      messages: ['Correo electrónico: El correo ya existe.'],
    })
  })
  it('una venta usa un solo POST y recibe el entero de orden', async () => {
    const fetcher = vi.fn().mockResolvedValue(new Response('42'))
    vi.stubGlobal('fetch', fetcher)
    expect(
      await new ApiClient().post(
        '/CreateOrder',
        { customerId: 'ALFKI', orderDetails: [] },
        'token',
      ),
    ).toBe(42)
    expect(fetcher).toHaveBeenCalledTimes(1)
    expect(fetcher.mock.calls[0][1].method).toBe('POST')
  })
  it('no reintenta un POST cuya respuesta se perdió; explica el resultado incierto', async () => {
    const fetcher = vi.fn().mockRejectedValue(new TypeError('network unavailable'))
    vi.stubGlobal('fetch', fetcher)
    await expect(new ApiClient().post('/CreateOrder', {}, 'token')).rejects.toMatchObject({
      messages: [expect.stringContaining('Facturas')],
    })
    expect(fetcher).toHaveBeenCalledTimes(1)
  })
  it('preserva cancelación de lecturas', async () => {
    const controller = new AbortController()
    controller.abort()
    vi.stubGlobal('fetch', vi.fn().mockRejectedValue(new DOMException('Aborted', 'AbortError')))
    await expect(new ApiClient().get('/orders', 'token', controller.signal)).rejects.toMatchObject({
      name: 'AbortError',
    })
  })
  it('una respuesta de venta ilegible exige revisar Facturas sin repetir el POST', async () => {
    const fetcher = vi.fn().mockResolvedValue(new Response('<html>gateway response</html>'))
    vi.stubGlobal('fetch', fetcher)
    await expect(new ApiClient().post('/CreateOrder', {}, 'token')).rejects.toMatchObject({
      messages: [expect.stringContaining('Facturas')],
    })
    expect(fetcher).toHaveBeenCalledTimes(1)
  })
  it('entrega PDF binario y rechaza respuestas que no son PDF', async () => {
    vi.stubGlobal(
      'fetch',
      vi
        .fn()
        .mockResolvedValue(
          new Response('%PDF-1.7 sample', { headers: { 'Content-Type': 'application/pdf' } }),
        ),
    )
    expect((await new ApiClient().pdf(1, 'token')).type).toBe('application/pdf')
    vi.stubGlobal(
      'fetch',
      vi
        .fn()
        .mockResolvedValue(
          new Response('<html>not a PDF</html>', { headers: { 'Content-Type': 'text/html' } }),
        ),
    )
    await expect(new ApiClient().pdf(1, 'token')).rejects.toBeInstanceOf(ApiError)
  })
})
