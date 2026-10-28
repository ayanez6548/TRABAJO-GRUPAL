# Contrato HTTP para una implementación nueva

Parte normativa de [SPEC.md](../../SPEC.md). JSON camelCase, UTF-8, números JSON sin símbolos monetarios y fechas ISO 8601 con zona UTC. La API escucha por defecto en `http://127.0.0.1:5100`. El navegador React añade `/api` delante de las rutas; ese prefijo lo elimina su proxy, no forma parte de la API .NET.

## Operaciones

| Método y ruta exacta | Autorización | Entrada | Éxito |
|---|---|---|---|
| GET `/health` | Pública | Ninguna | 200 Health |
| POST `/user/Register` | Pública | Registration | 200 string JSON con ID de usuario |
| POST `/user/Login` | Pública | Credentials | 200 Tokens |
| GET `/sales/settings` | Bearer | Ninguna | 200 SalesSettings |
| GET `/catalog/customers` | Bearer | Ninguna | 200 Customer[]; compatibilidad Blazor |
| GET `/catalog/products` | Bearer | Ninguna | 200 Product[] con stock > 0; compatibilidad Blazor |
| GET `/catalog/customers/search` | Bearer | Query de clientes | 200 PagedResult<Customer> |
| GET `/catalog/products/search` | Bearer | Query de productos | 200 PagedResult<Product> |
| POST `/CreateOrder` | Bearer | CreateOrder | 200 número entero JSON de orden |
| GET `/orders` | Bearer | `page`, `pageSize` | 200 PagedResult<InvoiceSummary> propio |
| GET `/orders/{id}` | Bearer | `id` entero de ruta | 200 Invoice propia |
| GET `/orders/{id}/invoice.pdf` | Bearer | `id` entero de ruta | 200 bytes `application/pdf` propios |

`Authorization: Bearer <accessToken>` en cada operación protegida. Swagger `/swagger` y documento `/swagger/v1/swagger.json` se habilitan en Development o mediante configuración explícita. No confundir abrir Swagger con iniciar el frontend.

## Tipos de cuenta

```typescript
interface Registration {
  email: string;       // regla de correo, máximo 30
  password: string;    // mínimo/composición SPEC, sin máximo propio
  firstName: string;   // letras/espacios, 1–30
  lastName: string;    // letras/espacios, 1–30
}
interface Credentials { email: string; password: string }
interface Tokens {
  accessToken: string;
  expiresAt: string;   // DateTimeOffset ISO 8601
}
interface Health {
  status: 'ok';
  framework: 'net10.0';
  runtime: string;     // real, por ejemplo .NET 10.x
  os: string;          // real, no texto Debian fijo para falsear evidencia
}
```

Register no devuelve un JWT ni envía un correo por defecto. Confirmación de contraseña no es propiedad del DTO. Login válido devuelve token; email mal formado es 400 y credenciales incorrectas/bloqueadas 401 con mensaje genérico. Un correo ya registrado se rechaza 400, sin duplicar usuario.

## Catálogos y paginación

```typescript
interface Customer { id: string; name: string; currentBalance: number }
interface Product { id: number; name: string; unitPrice: number; unitsInStock: number }
interface SalesSettings { discountPercent: number; vatRate: number }
interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number; // ceil(totalCount/pageSize), 0 cuando no hay filas
}
```

Query de clientes: `q?`, `code?`, `name?`, `minBalance?`, `maxBalance?`, `page?`, `pageSize?`.

Query de productos: `q?`, `code?`, `name?`, `minPrice?`, `maxPrice?`, `minStock?`, `maxStock?`, `page?`, `pageSize?`.

- Todos los filtros opcionales. Vacío equivale a filtro omitido, no enviar `null` literal en query. Rangos inclusivos, campos combinados con AND.
- `q`: compatibilidad de búsqueda general, hasta 100 caracteres; código o nombre mediante OR, case-insensitive para texto. La nueva UI usa campos explícitos.
- `code` cliente: hasta 5 alfanuméricos, substring case-insensitive; producto: entero positivo, igualdad exacta.
- `name`: substring case-insensitive hasta 100; trim para consultar. Precios/saldos/existencias y límites de página como SPEC.
- Default de búsqueda: page=1, pageSize=10; historial: page=1, pageSize=5. El cliente envía tamaño explícito 5/10/20.
- Ordenar búsqueda por ID ascendente antes de paginar; historial por fecha e ID descendentes. Una página válida más allá del final devuelve items vacío sin inventar filas ni cambiar totalCount.
- Buscar productos nunca devuelve stock cero/negativo, aun si q o código coincide. Consultas parametrizadas con LINQ/EF; no concatenar SQL de entradas.

Ejemplo: `/catalog/products/search?name=Chef&minPrice=50&maxStock=40&page=1&pageSize=5`.

## Crear orden

```typescript
interface CreateOrder {
  customerId: string;
  shipAddress: string;
  shipCity: string;
  shipCountry: string;
  shipPostalCode?: string | null;
  orderDetails: CreateOrderDetail[]; // al menos una, productId únicos
}
interface CreateOrderDetail {
  productId: number; // int positivo
  unitPrice: number; // decimal positivo <= 999999.99, hasta 2 decimales
  quantity: number;  // short, 1–32767 y <= stock
}
```

No se envían `createdBy`, `orderDate`, tasa, descuento, total, stock ni número de orden para decidir esos valores. El servidor los establece. El precio enviado es el leído del catálogo y se compara durante la reserva transaccional; cambiarlo en Swagger no permite una venta con precio inventado.

Ejemplo válido contra semilla nueva y usuario autenticado:

```json
{
  "customerId": "ALFKI",
  "shipAddress": "Av. Central 123",
  "shipCity": "Quito",
  "shipCountry": "Ecuador",
  "shipPostalCode": "001234",
  "orderDetails": [
    { "productId": 1, "unitPrice": 35.00, "quantity": 2 },
    { "productId": 3, "unitPrice": 65.00, "quantity": 1 },
    { "productId": 4, "unitPrice": 75.00, "quantity": 1 },
    { "productId": 5, "unitPrice": 50.00, "quantity": 1 }
  ]
}
```

Respuesta: `1` (ejemplo; el ID real lo genera DB y puede tener saltos). Con cuatro productos distintos se genera correo especial antes del commit. Un JSON inválido/desbordamiento de tipos no entra como dato válido a la transacción. El backend no garantiza idempotencia del POST ante dos solicitudes independientes; el frontend bloquea doble envío y no hace reintentos automáticos.

## Factura e historial

```typescript
interface InvoiceSummary {
  id: number;
  orderDate: string;
  customerId: string;
  customerName: string;
  productsCount: number; // líneas/productos distintos, no suma de cantidades
  total: number;
}
interface InvoiceLine {
  productId: number;
  productName: string;
  unitPrice: number;
  quantity: number;
  amount: number;
}
interface Invoice {
  id: number;
  orderDate: string;
  customerId: string;
  customerName: string;
  shipAddress: string;
  shipCity: string;
  shipCountry: string;
  shipPostalCode: string | null;
  createdBy: string;
  discountPercent: number; // 10 en nuevas órdenes
  vatRate: number;         // 0.15, no 15, para nuevas órdenes demo
  details: InvoiceLine[];
  subtotal: number;
  discountAmount: number;
  taxableAmount: number;
  vatAmount: number;
  total: number;
}
```

JSON e historial calculan importes desde valores guardados, con la fórmula SPEC. PDF usa la misma factura y propiedad; nombre descargable `NorthWind-Factura-{id}.pdf`. Cambiar precio de catálogo o IVA de configuración después no altera importes de venta histórica. `GET /orders/{id}` y PDF responden 404 tanto para ID no existente como de otra cuenta.

## Errores y cliente

| Código | Situación observable |
|---|---|
| 400 | Campos/JSON/filtros inválidos; correo duplicado; cliente inexistente/con deuda; producto inexistente o stock insuficiente detectado antes de reservar. |
| 401 | Bearer ausente/inválido/vencido o credenciales incorrectas. |
| 404 | Factura/PDF inexistente o ajeno. |
| 409 | La reserva atómica no pudo mantener precio/stock; pedir actualizar catálogo. |
| 500 | Fallo inesperado, persistencia o notificación. Sin stack trace público. |

Forma ProblemDetails habitual:

```json
{
  "title": "Los datos ingresados no son válidos.",
  "status": 400,
  "instance": "/CreateOrder",
  "errors": {
    "OrderDetails[0].Quantity": ["La cantidad debe ser mayor que cero."]
  },
  "traceId": "identificador-de-solicitud"
}
```

`detail`, `type`, `instance`, `errors` y `traceId` pueden variar/ser opcionales según origen (binding, Identity, manejador). El cliente soporta ProblemDetails, 404 `{ "message": "No se encontró la orden solicitada." }`, texto/cuerpo vacío y error de red. No mostrar JSON crudo ni duplicar mensajes en tres componentes. Deducir estado de sesión solo según operación: 401 de login no equivale a éxito ni a una sesión previa válida.

Para lecturas: abortar/ignorar respuesta antigua. Para escritura: no reintentar automáticamente ni fingir éxito si un 200 no contiene el entero positivo esperado. Un fallo de transporte tras enviar puede ocurrir después del commit: dirigir al historial antes de repetir. Descarga PDF con Bearer mediante fetch/gateway; un enlace directo sin token no autoriza mágicamente la lectura.
