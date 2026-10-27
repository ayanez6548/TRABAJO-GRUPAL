export class ApiError extends Error {
  status: number
  messages: string[]
  constructor(status: number, messages: string[]) {
    super(messages.join('\n'))
    this.status = status
    this.messages = messages
  }
}
export const cancelled = (error: unknown) => error instanceof Error && error.name === 'AbortError'
const fieldNames: Record<string, string> = {
  Email: 'Correo electrónico',
  Password: 'Contraseña',
  FirstName: 'Nombres',
  LastName: 'Apellidos',
  CustomerId: 'Cliente',
  ShipAddress: 'Dirección',
  ShipCity: 'Ciudad',
  ShipCountry: 'País',
  ShipPostalCode: 'Código postal',
  OrderDetails: 'Productos',
  ProductId: 'Producto',
  Quantity: 'Cantidad',
  UnitPrice: 'Precio',
  Search: 'Búsqueda',
  History: 'Historial',
}
function fieldLabel(field: string): string {
  const match = /^OrderDetails\[(\d+)\](?:\.(\w+))?$/.exec(field)
  if (match)
    return `Producto ${Number(match[1]) + 1}${match[2] ? ', ' + (fieldNames[match[2]] || 'datos').toLowerCase() : ''}`
  return fieldNames[field] || 'Datos ingresados'
}
export class ApiClient {
  private base: string
  constructor(base = '/api') {
    this.base = base.replace(/\/$/, '')
  }
  private async request(
    path: string,
    method: 'GET' | 'POST',
    token?: string,
    signal?: AbortSignal,
    body?: unknown,
    pdf = false,
  ): Promise<Response> {
    if (!/^\/(?!\/)/.test(path))
      throw new ApiError(0, ['La dirección de la operación no es válida.'])
    const headers = new Headers({ Accept: pdf ? 'application/pdf' : 'application/json' })
    if (token) headers.set('Authorization', `Bearer ${token}`)
    if (body !== undefined) headers.set('Content-Type', 'application/json')
    let response: Response
    try {
      response = await fetch(this.base + path, {
        method,
        headers,
        signal,
        credentials: 'omit',
        redirect: 'error',
        ...(body !== undefined ? { body: JSON.stringify(body) } : {}),
      })
    } catch (error) {
      if (cancelled(error)) throw error
      throw new ApiError(0, [
        method === 'POST' && path === '/CreateOrder'
          ? 'No se pudo confirmar el resultado de la venta. Revisa Facturas antes de volver a guardarla.'
          : 'No se pudo conectar con el servidor. Comprueba la conexión e inténtalo nuevamente.',
      ])
    }
    if (!response.ok) {
      let problem: { errors?: Record<string, unknown>; title?: unknown; detail?: unknown } = {}
      try {
        problem = await response.json()
      } catch {
        /* A proxy may return a non-JSON error. */
      }
      const messages = Object.entries(problem.errors || {}).flatMap(([field, values]) =>
        Array.isArray(values)
          ? values
              .filter((message): message is string => typeof message === 'string')
              .map((message) => `${fieldLabel(field)}: ${message}`)
          : [],
      )
      const fallback =
        response.status === 401
          ? 'Tu sesión venció o las credenciales son incorrectas. Inicia sesión nuevamente.'
          : response.status === 404
            ? 'No se encontró la factura solicitada.'
            : response.status === 409
              ? 'El stock o el precio cambió. Actualiza el catálogo y revisa la venta.'
              : 'No se pudo completar la operación. Inténtalo nuevamente.'
      throw new ApiError(response.status, [
        ...new Set(
          messages.length
            ? messages
            : [
                typeof problem.detail === 'string'
                  ? problem.detail
                  : typeof problem.title === 'string'
                    ? problem.title
                    : fallback,
              ],
        ),
      ])
    }
    return response
  }
  private async json<T>(response: Response): Promise<T> {
    try {
      return (await response.json()) as T
    } catch (error) {
      if (cancelled(error)) throw error
      throw new ApiError(0, ['La respuesta del servidor no pudo leerse. Inténtalo nuevamente.'])
    }
  }
  async get<T>(path: string, token?: string, signal?: AbortSignal): Promise<T> {
    return this.json<T>(await this.request(path, 'GET', token, signal))
  }
  async post<T>(path: string, body: unknown, token?: string): Promise<T> {
    const response = await this.request(path, 'POST', token, undefined, body)
    try {
      return await this.json<T>(response)
    } catch (error) {
      if (path === '/CreateOrder') {
        throw new ApiError(0, [
          'No se pudo confirmar el resultado de la venta. Revisa Facturas antes de volver a guardarla.',
        ])
      }
      throw error
    }
  }
  async pdf(id: number, token: string, signal?: AbortSignal): Promise<Blob> {
    const response = await this.request(
      `/orders/${id}/invoice.pdf`,
      'GET',
      token,
      signal,
      undefined,
      true,
    )
    if (!response.headers.get('Content-Type')?.toLowerCase().includes('application/pdf'))
      throw new ApiError(0, ['No se pudo preparar el PDF de la factura.'])
    return response.blob()
  }
}
export const api = new ApiClient()
