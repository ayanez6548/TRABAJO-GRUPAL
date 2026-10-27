# Spec 001 — Tercer frontend NorthWind

Estado inicial: especificado antes de la implementación, 26 de octubre de 2026.

## Contexto y objetivo

El trabajo grupal exige tres frontends que reutilicen el backend .NET y los requisitos de ventas del semestre anterior. Esta entrega desarrolla solamente el tercero, con React y TypeScript, siguiendo desarrollo guiado por especificaciones. El frontend Blazor individual se conserva.

## Actores e historias

- H1: Como vendedor quiero registrarme e iniciar sesión para operar con mi cuenta.
- H2: Como vendedor quiero seleccionar clientes y productos disponibles para guardar una venta correcta.
- H3: Como vendedor quiero consultar mis facturas y descargar su PDF para revisar y entregar comprobantes.
- H4: Como expositor quiero relacionar requisitos, tareas, código y pruebas para demostrar el proceso SDD.

## Requisitos funcionales en EARS

- **RF-01 Registro:** CUANDO el usuario completa datos válidos y contraseñas coincidentes, EL SISTEMA enviará el registro a la API y mostrará su resultado; SI el correo ya existe, mostrará el error recibido sin crear una cuenta simulada.
- **RF-02 Login:** CUANDO las credenciales sean válidas, EL SISTEMA usará el token y su vencimiento devueltos por la API; SI son inválidas, mostrará una sola advertencia y conservará la posibilidad de corregirlas.
- **RF-03 Sesión:** MIENTRAS no exista una sesión vigente, EL SISTEMA ocultará ventas, facturas y reloj, y protegerá sus rutas. CUANDO se cierre o venza la sesión, eliminará el token local y volverá al acceso.
- **RF-04 Cliente:** CUANDO el vendedor busque un cliente, EL SISTEMA abrirá una ventana con filtros por código, nombre y saldo. SI el cliente tiene saldo pendiente positivo, impedirá usarlo en la venta y explicará el motivo.
- **RF-05 Producto:** CUANDO el vendedor busque productos, EL SISTEMA consultará la API con filtros por código, nombre, precio y existencias y ofrecerá solo los productos con stock positivo.
- **RF-06 Filtros:** CUANDO se elija un campo, EL SISTEMA mostrará solo sus controles y conservará los demás filtros como etiquetas removibles. Los campos se combinarán y se podrán limpiar.
- **RF-07 Búsqueda y páginas:** CUANDO transcurran 400 ms desde la última edición válida, EL SISTEMA actualizará la búsqueda y volverá a la primera página; SI llega una respuesta antigua, no reemplazará los resultados actuales. La ventana permitirá 5, 10 o 20 resultados y navegación por páginas.
- **RF-08 Detalle:** CUANDO se prepare una venta, EL SISTEMA exigirá al menos un producto distinto, seleccionado del catálogo, con cantidad entera positiva que no supere su stock ni 32767.
- **RF-09 Cambio:** CUANDO se cambie un producto del borrador, EL SISTEMA conservará la cantidad y las otras líneas; SI el nuevo producto ya está incluido, lo rechazará; SI no cubre la cantidad conservada, exigirá corregirla antes de guardar.
- **RF-10 Retirada:** CUANDO se solicite quitar una línea, EL SISTEMA pedirá confirmación identificando el producto. Cancelar o Escape conservará la línea y su cantidad; confirmar quitará únicamente esa línea.
- **RF-11 Envío:** CUANDO se guarde una orden, EL SISTEMA exigirá dirección de 1–60 caracteres, ciudad y país de 3–15 letras/espacios, y código postal opcional de hasta 10 dígitos, conservando ceros iniciales.
- **RF-12 Importes:** MIENTRAS se edite el borrador, EL SISTEMA calculará subtotal, descuento, base, IVA y total con redondeo decimal a centavos y parámetros de `/sales/settings`. Precios, códigos, existencias y totales serán de solo lectura.
- **RF-13 Guardado:** CUANDO el borrador válido se envíe, EL SISTEMA realizará un único POST y bloqueará envíos repetidos mientras espera. Al recibir el número de orden, mostrará venta completada y abrirá su factura; SI recibe error, conservará el borrador y mostrará el mensaje de la API.
- **RF-14 Integridad:** EL SISTEMA respetará los errores de cliente, stock, precio, concurrencia y correo que devuelve el backend. No simulará una venta exitosa ni disminuirá stock únicamente en memoria como sustituto del servidor.
- **RF-15 Historial:** CUANDO se abran Facturas, EL SISTEMA solicitará las ventas del token actual, con paginación de 5, 10 o 20, fecha, cliente y total; permitirá actualizar y buscar por número de orden positivo.
- **RF-16 Comprobante:** CUANDO se abra una factura, EL SISTEMA mostrará su encabezado, envío, detalle e importes guardados en una ventana al frente, y permitirá ver, descargar e imprimir el PDF devuelto por la API.
- **RF-17 Propiedad:** EL SISTEMA no enviará un propietario elegido por el cliente para consultar ventas. SI la API responde 401 o 404, no mostrará datos de otra cuenta ni reutilizará datos de una sesión anterior.
- **RF-18 Cuenta y campos:** EL SISTEMA aplicará nombres/apellidos de solo letras y espacios hasta 30 caracteres, correo válido de hasta 30 con `. _ - + @`, una arroba y sin espacios; contraseña de registro de mínimo 6 con mayúscula, minúscula, número y símbolo. Contraseña, confirmación y login no tendrán el antiguo máximo de 30.
- **RF-19 Avisos:** CUANDO haya errores de caracteres, campos, API o confirmación de retirada, EL SISTEMA usará una ventana común accesible, al frente, sin bloques rojos superiores repetidos ni globos nativos; permitirá cerrar con teclado y recuperar el foco.
- **RF-20 Idioma y uso:** EL SISTEMA mostrará su interfaz en español, importes USD con formato es-EC, etiquetas claras, controles consistentes y tablas utilizables en pantallas pequeñas.
- **RF-21 Transporte:** EL SISTEMA consumirá exclusivamente la API configurada, adjuntará Bearer a las operaciones protegidas y gestionará cancelación de lecturas, errores de conexión y vencimiento. No guardará contraseñas ni claves SMTP/JWT del servidor.
- **RF-22 Evidencia SDD:** LA ENTREGA incluirá constitución, especificación, clarificaciones, plan, tareas, contrato API, matriz de trazabilidad, resultados de pruebas y guion de defensa; cada requisito indicará su implementación y verificación real.

## Casos límite

- Vacíos, nombres con tildes/ñ, pegado de caracteres inválidos y campos en el límite de longitud.
- Contraseñas de 31, 64 y más caracteres y confirmación diferente: no truncar ni normalizar contraseñas.
- Cliente con deuda, producto agotado, producto repetido, cantidad vacía/cero/decimal o mayor al stock.
- Sustitución por un producto con menos existencias sin perder el borrador.
- Rango mínimo mayor que máximo, búsquedas sin resultados y respuesta anterior que termina después de una nueva.
- Dos clics en Guardar; conflicto 409; sesión vencida; factura inexistente o ajena.
- Desconexión tras enviar una venta: resultado no confirmado; no reintentar automáticamente el POST y recomendar revisar Facturas antes de repetirlo.
- PDF tardío o fallido: cerrar/revocar recursos correctamente y mostrar un aviso sobre la ventana.

## Requisitos no funcionales

- React y TypeScript con comprobación estricta; dependencias fijadas mediante lockfile.
- Navegación accesible con teclado y ventanas nativas con foco y Escape.
- Uso del backend existente sin cambios de esquema, credenciales ni transacciones de servidor.
- Desarrollo y compilación documentados para Windows y Linux. La presentación en Debian usará archivos compilados y proxy de API local.
- Blazor individual y API publicada permanecen en sus puertos; el nuevo frontend usa por defecto 5300.

## Fuera de alcance

Los otros dos frontends grupales, mantenimiento CRUD de catálogos, cédula, recuperación de contraseña, nuevos roles, facturación electrónica fiscal, cambios de IVA legal, publicación pública con dominio y nuevas reglas de negocio del backend.

## Criterios de finalización

Todos los RF deben tener implementación y prueba o verificación manual identificada. Deben pasar el chequeo de tipos, la compilación y las pruebas críticas. El recorrido real debe comprobar login, catálogos, errores, una venta sobre base aislada, historial y PDF. La instalación de Debian se verificará cuando la máquina esté accesible; cualquier comprobación pendiente se declarará explícitamente.
