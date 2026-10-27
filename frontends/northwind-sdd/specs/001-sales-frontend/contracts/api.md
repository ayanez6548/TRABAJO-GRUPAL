# Contrato del backend existente

Reconocido en DTO y endpoints C#, antes del código React. JSON camelCase; textos de respuesta en español. No se crean rutas de negocio nuevas.

| Método/ruta | Entrada | Respuesta correcta |
|---|---|---|
| POST `/user/Register` | `{email,password,firstName,lastName}` | 200 y un identificador de usuario JSON |
| POST `/user/Login` | `{email,password}` | 200 `{accessToken,expiresAt}` |
| GET `/sales/settings` | Bearer | `{discountPercent,vatRate}` |
| GET `/catalog/customers/search` | Bearer; `code,name,minBalance,maxBalance,page,pageSize` | `PagedResult<Customer>` |
| GET `/catalog/products/search` | Bearer; `code,name,minPrice,maxPrice,minStock,maxStock,page,pageSize` | `PagedResult<Product>` |
| POST `/CreateOrder` | Bearer; orden descrita abajo | 200 y número entero JSON |
| GET `/orders` | Bearer; `page,pageSize` | `PagedResult<InvoiceSummary>` del usuario actual |
| GET `/orders/{id}` | Bearer | Factura del usuario actual |
| GET `/orders/{id}/invoice.pdf` | Bearer | Binario application/pdf |

`PagedResult<T>`: `{items:T[],page:number,pageSize:number,totalCount:number,totalPages:number}`.

`Customer`: `{id:string,name:string,currentBalance:number}`.

`Product`: `{id:number,name:string,unitPrice:number,unitsInStock:number}`.

Orden: `{customerId,shipAddress,shipCity,shipCountry,shipPostalCode,orderDetails:[{productId,unitPrice,quantity}]}`. No incluye IVA ni descuento editables; el servidor los determina.

`InvoiceSummary`: `{id,orderDate,customerId,customerName,productsCount,total}`.

Factura: `{id,orderDate,customerId,customerName,shipAddress,shipCity,shipCountry,shipPostalCode,createdBy,discountPercent,vatRate,details,subtotal,discountAmount,taxableAmount,vatAmount,total}`.

Detalle guardado: `{productId,productName,unitPrice,quantity,amount}`.

Errores: 400 para validación/formato, 401 para acceso, 404 para factura inexistente/ajena, 409 para precio/stock concurrente y 500 para fallos de operación/notificación. ProblemDetails puede incluir `title`, `detail`, `errors` por campo y `traceId`.

La confirmación de contraseña pertenece al formulario: no se añade al DTO de registro. Las facturas no aceptan un usuario seleccionado por el cliente. El precio debe proceder del catálogo y sigue siendo verificado en la transacción del servidor.

Los parámetros numéricos HTTP usan punto decimal; el usuario puede ver o introducir coma según el campo. Los importes visibles usan USD y formato es-EC.
