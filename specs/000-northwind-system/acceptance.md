# Aceptación de una reconstrucción desde cero

Contrato complementario de [SPEC.md](../../SPEC.md). **Estos casos son requisitos de verificación, no resultados ya obtenidos por una app nueva.** Registrar evidencia real en cada implementación. Los informes anteriores no se heredan como pruebas de código generado de nuevo.

## Preparación

Base relacional aislada con semilla de `architecture.md`, PickupDirectory privado, dos cuentas A/B registradas con datos ficticios y claves locales aleatorias. Restablecer fixtures solo ahí; la concurrencia, fallo SMTP y cambios de precio/tasa no se prueban sobre la base de defensa. Ejecutar casos UI en Blazor y React cuando aplique. Para SQL Server/PostgreSQL anotar proveedor probado; no atribuir resultados al otro sin ejecutarlos.

| Caso | SYS | Dado / acción | Resultado observable requerido |
|---|---|---|---|
| AC-01 | SYS-01, SYS-02 | Crear solución y compilar desde carpeta nueva. | 31 proyectos productivos con las cuatro capas/nombres; tests aparte; sin bibliotecas vacías ni referencias circulares. Backend y ambos clientes existen. |
| AC-02 | SYS-03, SYS-28 | Recorrer inicio, cuenta, venta, error y PDF. | Texto de interacción español, es-EC/USD, código/JSON inglés; sin traducción automática; textos naturales y botones consistentes. |
| AC-03 | SYS-04 | Inicializar DB vacía y ejecutar migraciones otra vez. | Modelo, FK, PK compuesta, CHECK stock, Identity, índice email único y semilla correctos; segunda ejecución no duplica semilla ni elimina datos. |
| AC-04 | SYS-05, SYS-09 | Registrar nombres con ñ/tildes y correo válido. Repetir correo cambiando mayúsculas. | Primer registro crea un usuario/hash; duplicado rechazado sin segunda cuenta. Confirmación no enviada como dato de negocio. |
| AC-05 | SYS-05, SYS-09 | Vacíos, solo espacios, nombres de 30/31 y pegado `Muñoz123`. Probar UI y API directa. | 30 válido; 31/vacío/inválido rechazados; pegado no se transforma en otro apellido; un aviso, datos corregibles conservados. |
| AC-06 | SYS-06, SYS-09 | Correo 30/31; `ana+ventas@demo.com`, dos @, espacios, punto local inicial/final/duplicado, dominio sin punto o con guion extremo. | Aceptar válido dentro de límite; rechazar variantes inválidas en registro/login/API. |
| AC-07 | SYS-05, SYS-06, SYS-09 | Contraseña débil, confirmación diferente y contraseña válida de 31/64/128 caracteres. | Débil/diferente rechazadas al registrar; las largas se registran y permiten login sin truncado ni trim. Login no impone máximo de 30. |
| AC-08 | SYS-06 | Login correcto y luego incorrecto; cinco fallos para cuenta existente, probar durante bloqueo y tras su plazo. | JWT firmado/expiración correctos; errores genéricos sin revelar cuenta; lockout5 minutos tras 5 fallos. Sin contraseña/JWT en logs de diagnóstico. |
| AC-09 | SYS-06, SYS-07 | Sin Bearer, token vencido, firma alterada, emisor/audiencia incorrectos. | Endpoints protegidos401; validar realmente firma/emisor/audiencia/vencimiento; frontend limpia sesión cuando corresponde. |
| AC-10 | SYS-07 | Abrir sin sesión rutas de venta/facturas; iniciar, cerrar, vencer y cambiar de cuenta. | Nav/reloj ocultos sin sesión, rutas protegidas; con sesión aparecen; salir elimina token/datos y evita ver cuenta anterior. |
| AC-11 | SYS-08, SYS-24 | A crea venta; B consulta su historial, ID y PDF. | Historial B no contiene venta A; JSON/PDF ajenos404, igual que inexistentes. Propietario manipulado en payload no asigna la venta a B. |
| AC-12 | SYS-09, SYS-10 | Dirección60/61, ciudad/país3/15/16, letras con tildes, postal `001234`/letras/11dígitos y campos vacíos. | Límites SPEC tanto UI como API; ceros iniciales conservados; códigos/precio/stock/totales no editables. |
| AC-13 | SYS-11, SYS-12 | Abrir lupa cliente/producto, elegir código/nombre/rango, combinar dos filtros y quitar uno. | Ventana frontal; solo filtro elegido desplegado, etiquetas conservadas, AND correcto; Limpiar restablece todo. |
| AC-14 | SYS-09, SYS-12 | Filtro dinero negativo, más de dos decimales, min>max; stock 0/32768; nombre101; código inválido. | UI no consulta valores inválidos y API directa400; sin errorSQL ni resultados engañosos. Coma de UI convertida a punto HTTP. |
| AC-15 | SYS-13 | Escribir varias veces antes de400ms y retrasar artificialmente respuesta anterior. | Nueva consulta tras pausa válida; página1; respuesta obsoleta nunca pisa la actual. Cancelar no muestra error duplicado. |
| AC-16 | SYS-13 | Fixtures de al menos21 filas; cambiar páginas/tamaño5/10/20 y solicitar API size51/page0. | Paginación en servidor estable; límites deshabilitados, totalCount correcto; entradas fuera del contrato400. Vacío se muestra como vacío. |
| AC-17 | SYS-14 | Buscar Chang stock 0 por nombre/código/general y seleccionar ANTON con deuda. | Producto agotado ausente de listas/búsquedas de venta; cliente con deuda impide venta en UI/API. Cliente inexistente rechazado. |
| AC-18 | SYS-15 | Agregar mismo producto dos veces; enviar duplicado/nulo/lista vacía directamente a API; probar agregado y PK. | Rechazo en cada defensa aplicable; no consolidar filas ni guardar venta parcial. |
| AC-19 | SYS-09, SYS-15 | Cantidad vacía/0/-1/decimal/letras, stock+1 y32768; después cantidad válida. | Inválidos rechazados; no guarda valor anterior oculto; válido hasta stock/32767. Precio inválido o más de dos decimales400. |
| AC-20 | SYS-16 | Cambiar producto de línea con cantidad2; seleccionar otro ya incluido y uno con stock 1. | Otros datos/cantidad se conservan; duplicado rechazado; stock insuficiente exige corrección sin reducir cantidad silenciosamente. |
| AC-21 | SYS-16, SYS-27 | Quitar; Cancelar, Escape y finalmente confirmar en una línea. | Foco Cancelar; cancelar conserva todo; confirmar quita solo esa línea; catálogo/DB no se modifica. |
| AC-22 | SYS-17 | Fixture del ejemplo SPEC: Chai×2, Aniseed×1, Cajun×1, Gumbo×1. Añadir caso de mitad de centavo. | 260/26/234/35.10/269.10; redondeo AwayFromZero decimal, mismo resultado cliente/JSON/PDF. |
| AC-23 | SYS-18 | Doble clic Guardar durante latencia; intentar editar venta ya confirmada. | Solo un POST mientras pendiente; éxito devuelve entero positivo, aviso y factura frontal; venta bloqueada, Nueva orden crea borrador nuevo. |
| AC-24 | SYS-18, SYS-23 | Desconectar tras envío o responder200 con JSON/noentero inesperado. | No reintenta ni confirma éxito falso; advierte revisar Facturas antes de repetir. No afirma rollback sin evidencia. |
| AC-25 | SYS-19 | Guardar una venta válida y consultar DB/logs. | Encabezado/detalle/stock/auditoría éxito persistidos en una transacción; creador del token, fechaUTC; salida presenter después del commit. |
| AC-26 | SYS-19 | Dos conexiones solicitan simultáneamente las últimas unidades de un producto. | Total vendido <= stock inicial, stock nunca negativo; una solicitud no disponible falla400/409 según fase; sin detalle/stock parcial del fallo. Usar DB real, no EF InMemory. |
| AC-27 | SYS-19 | Leer catálogo, modificar su precio en fixture y guardar con precio anterior/manipulado. | Reserva detecta discrepancia409 y revierte todas las líneas; no vende a precio inventado. |
| AC-28 | SYS-19, SYS-22 | Provocar fallo de persistencia después de reservar stock. | Rollback de stock/orden/detalles/log de éxito; inicio y cancelación conservados; sin ID de factura de venta fallida. |
| AC-29 | SYS-20, SYS-21 | Guardar con 3 productos y con 4 distintos en PickupDirectory. |3 no notifica,4 genera `.eml` español con ID/número distinto; Specification/eventhub/manejador separados. Registro/login no generan ese correo. |
| AC-30 | SYS-19, SYS-21 | En base aislada, SMTP inaccesible y venta especial. |500 de notificación en español; stock/orden/detalles/éxito revertidos; inicio/cancelación visibles. No afirmar que un correo enviado se pueda deshacer. |
| AC-31 | SYS-22, SYS-23 | Consultar auditoría y generar error controlado/API500. | FechaUTC/usuario/información; diagnóstico por consola/journal, traceId, sin stack/secreto público. Auditoría y log de arranque diferenciados. |
| AC-32 | SYS-23, SYS-27 | Producir400/401/404/409/500 y cuerpo de error vacío/noJSON. | Mensajes útiles españoles en una ventana; conservar borrador cuando corresponde; no mostrar JSON crudo ni tres avisos. |
| AC-33 | SYS-24 | A tiene6 facturas y B otras; consultar5 por página, actualizar/buscar ID propio/noexistente. | A ve5+1 solo propias en orden descendente; actualización real de DB; vacío/noencontrado diferenciados. |
| AC-34 | SYS-25 | Guardar venta, cambiar precio y tasa de configuración; volver a consultar. | Importes históricos conservados desde línea/tasa guardadas, no desde catálogo actual; no prometer snapshot de nombres. |
| AC-35 | SYS-26 | Abrir factura inmediatamente y desde historial; obtener PDF, cerrar/reabrir, simular carga tardía/fallida. | Vista frontal, encabezado/detalles/importes correctos, PDF original; recursos liberados, error sobre modal sin borrar venta. |
| AC-36 | SYS-26 | PDF de varias páginas; ver, descargar, abrir/impresión en navegador real. | Todas las páginas visibles, binario válido `application/pdf`, nombre correcto, autenticación presente. Registrar si impresión física no se probó. |
| AC-37 | SYS-27, SYS-28 | Navegación solo teclado y diálogos anidados; viewport390 y1280px. | Foco perceptible/restituido, modal sobre modal adecuado, Escape correcto; no overflow global, acciones separadas/tamaños consistentes. |
| AC-38 | SYS-29, SYS-30 | Ejecutar API en Debian13; servir dist React/PDFworker con proxy, refrescar rutaSPA. | Health muestra OS/runtime reales; rutas200, privadas401 sin token, `.mjs`MIMEJS, Bearer reenviado y PDF renderizado. |
| AC-39 | SYS-30 | Pedir traversal/dotfile/listado y cuerpo>1MiB al proxy; cambiar query rápidamente. | Acceso fuera de dist/configuración bloqueado, petición grande rechazada, destino fijo y sin secretos en logs. |
| AC-40 | SYS-31 | Nuevo clon en otra máquina con Docker/Node, inicializar dos veces, parar y reanudar. | Claves propias aleatorias y env preservado; tres URLs funcionales, DB/correos persisten; sin necesidad de contraseñas/IP del autor. |
| AC-41 | SYS-31 | Revisar archivos a publicar y documentación. | Sin `.env`, claves, passwords SMTP, usuarios/ventas reales ni artefactos de build; instrucciones permiten crear datos propios y no prometen web pública. |
| AC-42 | SYS-32 | Entrega de reconstrucción y revisión de trazabilidad. | SPEC→plan→tareas→implementación→prueba con IDs; comandos/resultados/pendientes reales, sin heredar checks de la app anterior. |

## Trazabilidad de los 19 requisitos del semestre

La enumeración normaliza el texto original (que tenía 11/12 y16/17 intercalados), sin eliminar ninguno.

| N.º | Requisito | SPEC | Aceptación |
|---|---|---|---|
|1| Modelo normalizado | SYS-04 | AC-03 |
|2| Orden completa y tablas lógicas | SYS-01, SYS-19 | AC-12, AC-25 |
|3| No duplicar producto | SYS-15 | AC-18 |
|4| Subtotales, totales, IVA | SYS-17 | AC-22, AC-34 |
|5| Editar solo necesario | SYS-10 | AC-12, AC-23 |
|6| Validar letras/números/vacíos/email donde aplique | SYS-09 | AC-05, AC-06, AC-07, AC-12, AC-14, AC-19 |
|7| Disminuir inventario automáticamente | SYS-19 | AC-25 |
|8| Solo productos stock positivo | SYS-14 | AC-17 |
|9| No vender más que stock | SYS-15, SYS-19 | AC-19, AC-26 |
|10| Quitar producto antes de guardar | SYS-16 | AC-21 |
|11| Cambiar producto sin perder datos | SYS-16 | AC-20 |
|12| Reconstruir factura/PDF/descargar/imprimir | SYS-24, SYS-25, SYS-26 | AC-33, AC-34, AC-35, AC-36 |
|13| Usabilidad | SYS-11, SYS-12, SYS-27, SYS-28 | AC-13, AC-37 |
|14| Errores y mensajes personalizados | SYS-23, SYS-27 | AC-24, AC-31, AC-32 |
|15| Ventana, búsqueda>=2 campos, páginas | SYS-11, SYS-12, SYS-13 | AC-13, AC-15, AC-16 |
|16| Buenas prácticas/código inglés | SYS-02, SYS-03 | AC-01, AC-02 |
|17| Transacción/concurrencia y resultado en UI | SYS-18, SYS-19 | AC-23, AC-25, AC-26, AC-27, AC-28 |
|18| Catálogos sin CRUD obligatorio/datos SQL | SYS-04, SYS-14 | AC-03, AC-17 |
|19| Programación por capas | SYS-02 | AC-01 |

La petición posterior del usuario precisa interfaz/mensajes en español aunque el requisito16 original dijera «todo en inglés». No se añade validación de cédula sin un campo de cédula.

## Trazabilidad de prácticas 01–16

| Práctica | Elemento reconstruido | SPEC | Casos |
|---|---|---|---|
|01 Introducción | Capas/dirección de dependencias | SYS-02 | AC-01 |
|02 Caso de uso | Entrada/salida y flujo de creación | SYS-01, SYS-18, SYS-19 | AC-23, AC-25 |
|03 Reglas empresariales | DTO/entidades/agregado/valores | SYS-02, SYS-15, SYS-17 | AC-18, AC-22 |
|04 Reglas de aplicación | Interactor, puertos y validadores de negocio | SYS-14, SYS-19 | AC-17, AC-25 |
|05 Adaptadores | Controllers/presenters/repos/gateways/views | SYS-02 | AC-01, AC-25 |
|06 Frameworks/Drivers | EFCore/Identity/DI/host/Blazor | SYS-02, SYS-04, SYS-29 | AC-01, AC-03, AC-38 |
|07 Validación | Hub/forma/reglas condicionadas | SYS-09, SYS-14, SYS-15 | AC-05, AC-12, AC-17, AC-19 |
|08 Excepciones | Excepciones tipadas/manejador/resultados | SYS-23 | AC-28, AC-32 |
|09 Eventos | Evento especial/manejador/correo | SYS-20, SYS-21 | AC-29, AC-30 |
|10 Registro de sucesos | Auditoría persistida | SYS-22 | AC-25, AC-28, AC-31 |
|11 Transacciones | Commit/rollback/limpieza/presenter | SYS-19 | AC-25, AC-26, AC-28, AC-30 |
|12 Specification | Regla de más de 3 productos distintos | SYS-20 | AC-29 |
|13 Autenticación/autorización | JWT y servicios de usuario | SYS-06, SYS-07, SYS-08 | AC-08, AC-09, AC-10, AC-11 |
|14 Registro | Identity/validación/resultado | SYS-05 | AC-04, AC-05, AC-07 |
|15 Login | Credenciales/token/sesión | SYS-06, SYS-07 | AC-06, AC-08, AC-10 |
|16 Configuración de seguridad | Middleware/DI/firma/emisor/audiencia/CORS | SYS-06, SYS-08, SYS-30 | AC-09, AC-11, AC-38 |

## Registro de evidencia requerido

Por caso/grupo: fecha, commit o versión, entorno/OS/runtime/proveedor, fixture aislado, comando o pasos, resultado esperado/real, evidencia y limitación. Las pruebas automáticas deben salir0; para manuales registrar observado, fallido o pendiente. La matriz no reemplaza ejecutar los casos.

Revisar también [la validación histórica React](../../frontends/northwind-sdd/specs/001-sales-frontend/validation.md) para contrastar lo comprobado antes. No marcar AC nuevos con esas evidencias sin volver a verificar la reconstrucción.
