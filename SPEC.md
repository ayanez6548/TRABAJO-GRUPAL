# SPEC maestro — NorthWind desde cero

Versión 1.0 · 28 de octubre de 2026 · Idioma del documento: español.

Esta especificación permite reconstruir **backend .NET 10, persistencia, frontend Blazor y tercer frontend React con TypeScript** sin copiar su código actual. Reúne las prácticas 01–16, los 19 requisitos de ventas y las decisiones posteriores del usuario. SDD es el proceso de trabajo; este SPEC es el contrato del producto que ese proceso debe construir.

**Origen:** consolidación de una aplicación ya implementada. Este documento no se presenta como una especificación escrita antes del código original. En una reconstrucción nueva se lee y verifica antes de programar. La especificación histórica del frontend React conserva sus fechas y evidencias.

## 1. Paquete mínimo para reconstruir

Entrega estos archivos a la persona o herramienta que vaya a implementar:

| Archivo | Contenido obligatorio |
|---|---|
| [SPEC.md](SPEC.md) | Alcance, reglas, campos, pantallas y restricciones. |
| [architecture.md](specs/000-northwind-system/architecture.md) | Los 31 proyectos, dependencias, modelo de datos y despliegue. |
| [api-contract.md](specs/000-northwind-system/api-contract.md) | Rutas, DTO, filtros, respuestas y errores. |
| [acceptance.md](specs/000-northwind-system/acceptance.md) | Casos observables y trazabilidad de los requisitos. |
| [plan.md](specs/000-northwind-system/plan.md) | Orden de construcción, entregables y comprobación. |
| [tasks.md](specs/000-northwind-system/tasks.md) | Lista pendiente para una implementación nueva. |
| [PROMPT.md](specs/000-northwind-system/PROMPT.md) | Instrucción de arranque para reconstruir desde esos documentos. |

El paquete es autosuficiente para definir el producto: no exige los PDF originales, el código anterior, la IP de una VM ni las credenciales de otra persona. Los enlaces a documentación histórica sirven para comparar la implementación existente, no para completar reglas ausentes.

### Precedencia y cambios

1. Una nueva instrucción explícita del usuario modifica este contrato: registrar primero el cambio y sus casos de aceptación.
2. Para reconstruir, este paquete define el alcance completo. La spec histórica React detalla su implementación original y no reduce el alcance del backend/Blazor aquí descrito.
3. Prevalecen las decisiones más recientes: código en inglés, interacción en español; correo/nombres/apellidos hasta 30; **contraseña sin el antiguo máximo de 30**; productos duplicados rechazados; inventario descontado.
4. Si aparece una contradicción que afecte datos o negocio, anotarla y resolverla antes de implementar esa parte. No inventar requisitos del docente ni aprobaciones humanas.

## 2. Objetivo, actores y alcance

**SYS-01 — Producto.** Un vendedor registra su cuenta, inicia sesión, prepara una orden de venta, selecciona un cliente sin deuda, añade productos disponibles y guarda una venta transaccional. Después consulta sus facturas, ve el PDF original del backend, lo descarga o lo imprime.

Actores: visitante (inicio, registro y login), vendedor autenticado (ventas y facturas propias) y expositor/operador (ejecución, logs, pruebas y defensa). El administrador de correo es un destinatario configurado; no requiere un panel o rol nuevo.

Entregables: backend .NET 10 ejecutable en **Debian 13**, base PostgreSQL, cliente Blazor WebAssembly .NET 10 y tercer frontend React/TypeScript con SDD. Los clientes comparten contrato y reglas de servidor. Blazor sigue siendo el frontend individual; React es solo el tercer frontend grupal, no los tres del equipo.

**Fuera de alcance:** CRUD de clientes/productos, cédula (no existe ese campo), pagos, devoluciones, modificación/eliminación de ventas guardadas, recuperación de contraseña, OAuth, refresh tokens, administración de roles, facturación electrónica fiscal, dos frontends grupales adicionales y publicación pública con dominio/TLS. No añadir pantallas de ejemplo Counter/Weather para llenar carpetas.

## 3. Lineamientos técnicos obligatorios

**SYS-02 — Plataforma y arquitectura.** Cuatro capas Clean Architecture y 31 proyectos de producto, con nombres y distribución de `architecture.md`. Pruebas fuera de la solución para conservar ese número. Dominio y casos de uso dependen de abstracciones; EF Core, ASP.NET Identity, SMTP, PDF y UI se conectan mediante adaptadores e IoC. Ninguna biblioteca estará vacía solo para completar el conteo.

Backend .NET 10/C# 14; Blazor WebAssembly .NET 10 estable. Usar versiones compatibles y fijarlas en proyectos/lockfiles. La base reproducible existente usa Blazor 10.0.13; React/React DOM 19.3.1, TypeScript 7.0.2, Vite 8.3.5, React Router 7.18.4, decimal.js 10.6.0, PDF.js 6.4.299 y Vitest 5.0.5. Son versiones de la entrega, no una afirmación sobre cuál será la última versión futura. Antes de actualizar dependencias, verificar compatibilidad y repetir aceptación. Node 24 es el entorno probado; mínimo declarado de React: 22.13.

**SYS-03 — Idioma.** Identificadores, clases, archivos de código, rutas y propiedades JSON en inglés. Pantallas, ayudas, avisos, errores visibles, asunto/cuerpo de correo y PDF en español. `lang="es"`; moneda USD y formato `es-EC`. Los nombres de negocio del catálogo se conservan. Logs técnicos/auditoría pueden permanecer en inglés. No depender del traductor del navegador.

**SYS-04 — Persistencia.** PostgreSQL en Debian; SQL Server/LocalDB opcional en Windows, con modelo común y migraciones separadas por proveedor. LocalDB no se instala ni se intenta usar en Linux. Claves, FK, tipos y semilla en `architecture.md`. Las tablas Identity, `DomainLogs` y `__EFMigrationsHistory` son necesarias además de las cuatro de ventas.

## 4. Cuenta y sesión

**SYS-05 — Registro.** Nombres, apellidos, correo, contraseña y confirmación. Validar tanto interfaz como API; la confirmación solo pertenece a la interfaz. Correo único mediante Identity e índice único normalizado. No crear una sesión simulada al registrarse: indicar el resultado y permitir iniciar sesión. Contraseña almacenada solo como hash administrado por Identity.

**SYS-06 — Login y token.** Login con correo y contraseña. Emitir JWT firmado HS256 con `sub`, `email`, `name` (correo), `FullName`, `jti`, emisor `NorthWind`, audiencia `NorthWind.Client` y expiración por defecto de 30 minutos. El servidor valida firma, algoritmo, emisor, audiencia y vigencia; margen de reloj de 15 segundos. Clave privada de al menos 32 bytes, vencimiento configurable de 1 a 1440 minutos. Sin contraseña en logs, URL o almacenamiento de cliente. Bloqueo Identity tras 5 intentos fallidos por 5 minutos; no revelar si la cuenta existe mediante mensajes de login diferentes.

**SYS-07 — Autorización y navegación.** Sin sesión: solo Inicio, Crear cuenta e Iniciar sesión; ocultar Crear orden, Facturas y reloj. Proteger también rutas directas y endpoints, no solo enlaces. Con sesión: mostrar usuario, ventas, facturas, reloj y Cerrar sesión. Al cerrar/vencer sesión borrar token y datos de la cuenta anterior. React conserva token/vencimiento en `sessionStorage`; Blazor puede usar su almacén local de token, nunca contraseñas, y debe limpiar al salir/expirar. No añadir refresh automático. Una respuesta 401 de una operación protegida invalida la sesión; un login fallido muestra un único aviso.

**SYS-08 — Propiedad.** La identidad se obtiene del JWT, nunca de un `createdBy`/usuario enviado por el cliente. Historial, JSON y PDF solo de ventas propias. Otra cuenta recibe 404 al pedir una factura ajena, igual que una inexistente. El correo de creación guardado es una instantánea de auditoría; no se ofrece cambio de correo en este alcance.

## 5. Contrato de campos y edición

**SYS-09 — Validaciones.** Campos obligatorios rechazan vacío y solo espacios al enviar. Durante edición se permite vaciar para corregir. UI restringe escritura y pegado sin convertir silenciosamente una entrada inválida en otro dato. API vuelve a validar solicitudes de cualquier cliente, incluido Swagger.

| Campo | Regla de aceptación |
|---|---|
| Nombres / apellidos | Obligatorios, cada uno 1–30 caracteres, letras Unicode, marcas de acento y espacios; al menos una letra. Admitir tildes y ñ. Trim de extremos para persistir. |
| Correo de registro y login | Obligatorio, ASCII letras/números y `. _ - + @`, máximo 30 caracteres; una sola arroba, sin espacios. Local no vacío, sin punto inicial/final ni `..`. Dominio con al menos dos segmentos, cada uno 1–63 caracteres, letras/números y guiones interiores; sin segmentos vacíos. |
| Contraseña de registro | Mínimo 6, con mayúscula, minúscula, número y símbolo. **Sin límite máximo propio de la aplicación.** No truncar, quitar símbolos, hacer trim ni transformar su contenido. |
| Contraseña de login | Obligatoria, sin máximo propio de la aplicación; comprobar credenciales sin imponer de nuevo composición a una contraseña ya creada. |
| Confirmación | Igualdad exacta con contraseña; sin máximo propio. No enviar a la API. |
| Cliente de la venta | Selección de catálogo; ID exactamente 5 letras/dígitos ASCII. Debe existir y saldo pendiente <= 0. |
| Dirección de envío | Obligatoria, 1–60 caracteres, letras Unicode/marcas/números, espacios y `# . , / -`; no solo espacios. |
| Ciudad / país | Obligatorios, 3–15 caracteres, letras Unicode/marcas y espacios. Medir mínimo después de trim; máximo sobre el valor completo. |
| Código postal | Opcional, hasta 10 dígitos ASCII, sin signos ni espacios; cadena para conservar ceros iniciales. Vacío puede enviarse como null. |
| Cantidad por producto | Entero ASCII de 1 a stock disponible, máximo 32767; rechazar signos, letras, decimales y vacío al guardar. No reutilizar silenciosamente una cantidad anterior. |
| Precio de venta | Solo lectura desde catálogo. Positivo, hasta 999999.99, dos decimales como máximo. El backend verifica de nuevo el precio vigente. |
| Número de factura/orden | Entero positivo hasta 2147483647, máximo 10 dígitos; no confundir longitud con rango permitido. |
| Filtro código cliente | Opcional, hasta 5 letras/números ASCII; coincidencia parcial sin distinguir mayúsculas. |
| Filtro código producto | Opcional, entero positivo hasta 2147483647; coincidencia exacta. |
| Filtro nombre | Opcional, hasta 100 caracteres, búsqueda parcial; no restringir nombres del catálogo a solo letras. |
| Filtros saldo / precio | Opcionales, 0–999999.99, máximo dos decimales. UI admite coma/punto; query HTTP usa punto. Mínimo no puede superar máximo. |
| Filtros existencias | Opcionales, enteros de 1 a 32767; mínimo <= máximo. |
| Página / tamaño | API: página 1–100000, tamaño 1–50. UI: tamaños 5, 10 o 20. |

**SYS-10 — Campos bloqueados.** Editables: selección de cliente/productos, dirección/ciudad/país/postal, cantidad y filtros. No editables: precios, stock, IDs del catálogo, ID/fecha/propietario de venta guardada, descuento, IVA e importes calculados. No interpretar los límites del correo como una restricción aplicable a contraseña o búsqueda de producto.

## 6. Catálogos y filtros

**SYS-11 — Búsqueda.** Cliente se muestra en un control compacto de solo lectura con botón de lupa al extremo. Seleccionar cliente/producto abre una ventana al frente; no un bloque que empuje el formulario hacia abajo. Cliente muestra código, nombre, saldo y Seleccionar. Producto muestra código, nombre, precio, existencias y Seleccionar.

**SYS-12 — Filtros por campos.** Un selector compacto «Filtrar por» muestra el campo/rango elegido. Cliente: código, nombre y saldo mínimo/máximo. Producto: código, nombre, precio mínimo/máximo y stock mínimo/máximo. Combinar filtros mediante AND; conservar los anteriores como etiquetas visibles, eliminables individualmente. Limpiar elimina todos. No mostrar todos los campos desplegados de entrada. Mantener tamaño de página y botones Buscar/Limpiar como alternativas claras.

**SYS-13 — Actualización automática y páginas.** Consultar tras 400 ms sin editar una entrada válida, reiniciar a página 1 al cambiar filtro/tamaño, cancelar lecturas previas y descartar respuestas antiguas. No enviar filtros inválidos ni reemplazar resultados nuevos con los de una solicitud vieja. Server-side Count/Skip/Take con orden estable; no descargar todo para paginar localmente. Anterior/Siguiente se deshabilitan en límites. Estado de carga, sin resultados y fallo explícitos.

**SYS-14 — Disponibilidad y deuda.** Todas las consultas de productos para vender incluyen `UnitsInStock > 0`. Cliente con saldo positivo no se acepta para crear venta; explicar motivo y permitir elegir otro. El servidor comprueba existencia/deuda/stock aunque la UI ya los haya comprobado. La consulta previa de saldo no constituye un bloqueo concurrente del crédito del cliente.

## 7. Borrador, importes y guardado

**SYS-15 — Detalle sin duplicados.** Al menos una línea, un producto una sola vez. Rechazar duplicado en UI, validador, agregado y clave compuesta de detalle. No fusionar dos filas silenciosamente. Validar cantidad contra stock. Precios y totales no se aceptan como autoridad del navegador.

**SYS-16 — Cambiar y quitar antes de guardar.** Cambiar abre catálogo, conserva cantidad de esa línea y resto de datos. Rechazar reemplazo por producto ya incluido. Si el nuevo stock no cubre la cantidad conservada, exigir corregir antes de guardar; no reducirla silenciosamente. Quitar abre confirmación con nombre de producto y botones «Cancelar» / «Sí, quitar»; foco inicial en Cancelar. Cancelar o Escape deja línea/cantidad intactas; confirmar retira solo esa línea. No eliminar el producto del catálogo.

**SYS-17 — Cálculo decimal.** Sea `R(x)` el redondeo decimal a dos posiciones, punto medio alejándose de cero (`AwayFromZero`; `ROUND_HALF_UP` para importes positivos en React):

```text
line.amount     = R(unitPrice * quantity)
subtotal        = sum(line.amount)
discountAmount  = R(subtotal * discountPercent / 100)
taxableAmount   = subtotal - discountAmount
vatAmount       = R(taxableAmount * vatRate)
total           = taxableAmount + vatAmount
```

`discountPercent = 10`; IVA académico inicial `vatRate = 0.15`, configurable en servidor entre 0 y 1. Consultar `/sales/settings` para el borrador; guardar descuento y tasa en cada orden. Ejemplo reproducible: dos Chai a 35, un Aniseed a 65, un Cajun a 75 y un Gumbo a 50: subtotal 260.00, descuento 26.00, base 234.00, IVA 35.10, total 269.10. No usar punto flotante binario para reglas monetarias ni añadir tasas legales supuestas. El PDF es un comprobante académico.

**SYS-18 — Guardado y resultado incierto.** Un único POST por intento, botón/estado bloqueado mientras está pendiente. Éxito solo tras respuesta con número entero positivo de orden confirmada. Mostrar resultado y abrir factura al frente; bloquear edición de esa venta y ofrecer Nueva orden. Ante 400/409 conservar borrador para corregir/actualizar catálogo. No reintentar automáticamente el POST. Si hay desconexión después del envío o respuesta de éxito ilegible, avisar «No se pudo confirmar el resultado. Revisa Facturas antes de repetir la venta». No afirmar rollback basándose únicamente en una desconexión.

**SYS-19 — Transacción e inventario.** Validar usuario, forma, cliente y productos; persistir auditoría de inicio; crear agregado con propietario del token/fecha UTC/descuento/tasa del servidor. Iniciar transacción; reservar stock en orden ascendente de ID mediante UPDATE condicional (`Id`, `UnitsInStock >= quantity`, `UnitPrice == submittedPrice`); exigir una fila afectada por producto. Los bloqueos de escritura duran hasta commit. Guardar encabezado/detalles, obtener ID, auditar éxito y ejecutar evento especial; commit; invocar presenter después del commit. Un fallo dentro de esa transacción revierte stock, encabezado, detalles y auditoría de éxito; limpiar tracking y escribir cancelación fuera. La auditoría inicial permanece. El ID puede tener saltos después de rollback.

Dos ventas concurrentes no pueden consumir las mismas últimas unidades. Un conflicto durante reserva devuelve 409; un stock insuficiente ya detectado en prevalidación puede devolver 400. Validación previa por sí sola no protege contra concurrencia. No sustituir este comportamiento por una resta local del stock.

## 8. Prácticas: eventos, correo, auditoría y errores

**SYS-20 — Specification y evento.** `Specification<T>` expresa el criterio; `SpecialOrderSpecification` se satisface con **más de tres productos distintos**. `SpecialOrderCreatedEvent` transporta ID y cantidad de productos; un event hub despacha al manejador que usa `IMailService`. Tres productos no notifican; cuatro o más sí. Filas repetidas son inválidas y no cuentan como productos diferentes.

**SYS-21 — Notificación.** Correo al administrador configurado, asunto «Orden especial creada», cuerpo español con ID de orden y número de productos distintos. `PickupDirectory` genera `.eml` en `App_Data/mail` y permite la defensa sin claves externas. SMTP alternativo configurable (Mailtrap Sandbox puede usar `sandbox.smtp.mailtrap.io`, 587, STARTTLS); host/puerto/SSL/remitente/destinatario/usuario/contraseña se leen de configuración privada, nunca se copian contraseñas de la instalación previa.

Si falla la notificación de una orden especial antes del commit, cancelar la transacción y devolver error en español. El correo es un efecto externo: **un mensaje ya aceptado no se revierte con SQL**; no prometer entrega exactamente una vez ni implementar outbox sin cambiar el alcance. Mailtrap Sandbox recibe en su bandeja de pruebas; no equivale a envío a Gmail del destinatario. Login/registro no generan un correo nuevo por este requisito.

**SYS-22 — Logs.** `DomainLogs`: ID, fecha UTC, información y usuario autenticado. Inicio antes de transacción; éxito dentro; cancelación posterior al rollback. Errores de campos rechazados antes de empezar el caso de uso no necesitan un log de venta iniciada. Repository + Unit of Work + `IDomainLogger` para persistencia; `ILogger` para diagnóstico de consola/journal. No mezclar mensajes de arranque de ASP.NET con filas de auditoría esperadas. No registrar contraseña, JWT completo ni secretos SMTP.

**SYS-23 — Errores y resultados.** Puertos/presenters y `Result<T>` para membresía, errores de validación por propiedad, excepciones tipadas para persistencia/conflicto/notificación y manejador HTTP común. 400 validación/formato; 401 acceso/credenciales; 404 factura inexistente/ajena; 409 stock/precio concurrente; 500 fallo interno/persistencia/notificación. Usar ProblemDetails y `traceId` para errores de aplicación, sin stack trace público. El 404 de factura puede ser `{message}`; 401 Bearer puede no tener cuerpo. El frontend contempla esos formatos.

## 9. Facturas e historial

**SYS-24 — Historial persistido.** Facturas consulta al servidor las ventas del token actual; paginación 5/10/20, orden fecha descendente e ID descendente, número, fecha, cliente, número de productos y total. Botones Actualizar y abrir factura; búsqueda por número de orden positivo. Estado vacío no se presenta como error de conexión. No es una lista de consultas recientes guardada solo en el navegador.

**SYS-25 — Reconstrucción.** Encabezado con ID/fecha/cliente/creador/envío, detalle con código/nombre/precio/cantidad/importe, subtotal/descuento/base/IVA/total. Precios históricos de `OrderDetails` y porcentajes guardados en `Orders`; no recalcular con precio o IVA actual. En el modelo existente nombre de cliente/producto se consulta de catálogo: no prometer una instantánea histórica de esos nombres ni añadir columnas de snapshot sin una decisión nueva.

**SYS-26 — PDF y ventana.** Vista previa encima del contenido, no al pie largo de la página. PDF generado por backend desde factura autorizada, con textos españoles y todas sus páginas. Ver, descargar y abrir/imprimir el mismo PDF, no reconstruir un comprobante alternativo con importes del borrador. React usa PDF.js/worker local para navegadores cuyo visor PDF nativo no funcione; revocar object URLs y cancelar cargas al cerrar. Mostrar fallo del PDF sobre la vista previa conservando factura. No afirmar impresión física sin probarla.

## 10. Interacción y presentación

**SYS-27 — Avisos únicos.** Un componente común de ventana emergente al frente para errores de formulario/API y confirmaciones. Agrupar mensajes por campo sin duplicar; eliminar resúmenes rojos superiores, toasts repetidos y globos nativos que repitan el mismo error. El aviso debe quedar sobre catálogos/factura. Etiqueta/título accesible, foco dentro de ventana, Escape cuando corresponda y restitución de foco al cerrar. Las ayudas permanentes bajo campos no son alertas duplicadas.

**SYS-28 — Estilo y usabilidad.** Pantallas sencillas con jerarquía clara y texto natural. Inicio orientado a entrar y vender, sin párrafos publicitarios ni tarjetas decorativas innecesarias. Botones con altura y anchos consistentes por función; en filas usar «Seleccionar» con ancho estable y código en su columna, no botones cuyo tamaño cambie por el ID. Separar Cambiar y Quitar con espacio. Etiquetas visibles, foco perceptible, contraste legible, controles deshabilitados distinguibles, teclado usable y tablas con desplazamiento en móvil sin desbordar toda la página. Reloj solo autenticado.

Pantallas mínimas de ambos clientes:

| Pantalla | Contenido y estados |
|---|---|
| Inicio | Presentación breve y accesos adecuados a sesión. |
| Crear cuenta | Cinco campos, ayuda de límites, envío pendiente, resultado/aviso único. |
| Iniciar sesión | Correo/contraseña, carga, error sin borrar silenciosamente lo corregible. |
| Crear orden | Cliente con lupa, envío, líneas, catálogo, totales, Guardar/Nueva orden. |
| Catálogo cliente/producto | Filtro compacto, etiquetas activas, resultados, selección, carga/vacío/error y paginación. |
| Facturas | Historial propio paginado, búsqueda por número y Actualizar. |
| Vista previa | Encabezado/detalle/importes, PDF/descargar/imprimir/cerrar en primer plano. |
| Aviso/confirmación | Mecanismo común; confirmar quitar identifica el producto. |

Rutas React: `/`, `/register`, `/login`, `/orders/new`, `/invoices`. Rutas Blazor: `/`, `/user/register`, `/user/login`, `/orders/create`, `/invoices` y `/orders/{OrderId:int}`. No alterar las rutas API por traducir la pantalla.

## 11. Ejecución independiente y seguridad de entrega

**SYS-29 — Debian.** Mostrar backend ejecutándose en Debian 13 con `/health` informando OS/runtime reales. Se admite ejecutar con SDK o publicar como servicio `northwind-api`; no es obligatoria una web pública. Puertos por defecto: API/Swagger 5100, Blazor 5200, React 5300. API no depende de Visual Studio, Windows ni de la sesión SSH para datos.

**SYS-30 — React y proxy.** SPA TypeScript estricta, Vite y lockfile. Navegador usa `/api`; proxy retira prefijo y reenvía a API configurada, por defecto `http://127.0.0.1:5100`. Adjuntar Bearer solo a API autorizada. Desarrollo Vite; presentación Debian puede servir `dist` con Python 3, fallback SPA y proxy de destino fijo; sin instalar Node para servir. MIME `.mjs` JavaScript para worker PDF; bloquear traversal/dotfiles/listados, limitar cuerpo a 1 MiB y no imprimir headers sensibles. No introducir otro backend de ventas ni usar mocks en el flujo final.

**SYS-31 — Entrega al equipo.** Scripts para clonar, generar `.env` privado con valores aleatorios sin sobreescribir uno existente, arrancar PostgreSQL/API/Blazor con Docker Compose y React con Node. Base y correos en volúmenes persistentes; parar sin borrar datos. Variables y procedimiento en `architecture.md`. Git contiene fuentes, migraciones, semilla, lockfiles, specs y pruebas; excluye `.env`, secretos, cuentas/ventas existentes, volúmenes, bin/obj/node_modules/dist y claves privadas. Cada compañero usa sus propias cuentas/datos y puede presentar sin IP ni contraseña del autor.

## 12. Definición de terminado

**SYS-32 — SDD y evidencia.** En una implementación nueva seguir SPEC → clarificaciones → plan → tareas → pruebas/código → validación. Cada tarea referencia IDs SYS y AC. Registrar versiones realmente usadas, comandos, fecha, resultado y limitaciones. No marcar pruebas como realizadas por existir en un documento.

La reconstrucción termina cuando pasan los casos de `acceptance.md`, build .NET y React, pruebas críticas de dominio/HTTP/servidor, integración relacional concurrente sobre base **aislada**, y recorrido visual de ambos clientes. Repetir el arranque en Debian y desde clon limpio; comprobar PDF original y notificación real `.eml` o sandbox. Documentar cualquier prueba manual pendiente (por ejemplo impresión física). No probar fallos de SMTP/concurrencia ni generar ventas de ensayo sobre la base de defensa.

No se cambian ni se reinstalan servicios, bases de datos o credenciales de una instalación existente por leer este paquete. Una reconstrucción utiliza carpeta, base y configuración independientes.
