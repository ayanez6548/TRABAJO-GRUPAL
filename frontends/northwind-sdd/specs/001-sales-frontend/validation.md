# Matriz de trazabilidad y verificación

Revisión del **26 de octubre de 2026**. Las pruebas de ventas escribieron exclusivamente en SQL Server LocalDB `NorthWindSddReview20261026`, API Windows `.NET 10.0.12` en 5110, servidor React/proxy en 5300 y SMTP `PickupDirectory`. No se modificó el backend publicado ni la base Debian.

| RF | Implementación | Evidencia realizada |
|---|---|---|
| 01 Registro | `src/Access.tsx`, `api/client.ts` | Navegador: cuenta ficticia creada. Integración: registro 200 y duplicado 400. |
| 02 Login | `Access.tsx`, `context.tsx` | Navegador/API: login correcto con contraseña de más de 30 caracteres y navegación privada. |
| 03 Sesión | `context.tsx`, `domain/session.ts`, `App.tsx` | Unitarias: sesión vencida/corrupta. Navegador: Salir oculta navegación/reloj; acceso directo a `/invoices` vuelve a login. Se observó vencimiento durante la revisión. |
| 04 Cliente | `CatalogLookup.tsx`, `domain/rules.ts` | Navegador: ANTON rechazado por deuda, ALFKI seleccionado. Integración: filtros por código/nombre/saldo combinados. |
| 05 Producto | `CatalogLookup.tsx` | API real: todos con stock positivo, Chang con stock 0 ausente; búsqueda por nombre/precio/stock. Selección real en navegador. |
| 06 Filtros | `CatalogLookup.tsx`, `domain/rules.ts` | Navegador: cambiar Nombre a Código conserva etiqueta y combina criterios. Pruebas de query/rangos. |
| 07 Búsqueda/páginas | `CatalogLookup.tsx`, `components.tsx` | Navegador: actualización al escribir sin Buscar. Revisión de temporizador 400 ms, AbortController y revisión de respuesta; el orden artificial de respuestas no se simuló. Integración: páginas de 5 y segunda página sin elementos repetidos. |
| 08 Detalle | `OrderPage.tsx`, `domain/rules.ts` | Unitarias: duplicados, vacíos, 0, negativos, decimales y exceso de stock. Navegador: duplicado rechazado. API real también rechaza cantidad/duplicado. |
| 09 Sustitución | `OrderPage.tsx`, `domain/rules.ts` | Unitarias: conserva cantidad y otras líneas, detecta duplicado y stock menor. Navegador: Chai se sustituyó por Aniseed Syrup conservando cantidad 2. |
| 10 Retirada | `OrderPage.tsx`, `context.tsx` | Navegador: Cancelar conserva línea/cantidad; Escape conserva línea; Sí, quitar la elimina. Foco inicial en Cancelar y restauración al botón de origen cuando permanece. |
| 11 Envío | `components.tsx`, `domain/rules.ts` | Unitarias y navegador: letras/números por campo. Ciudad con número bloqueada mediante un solo aviso. Factura guardó código postal `001234`. |
| 12 Importes | `domain/rules.ts`, `OrderPage.tsx` | Pruebas decimales; comparación con factura real. Factura #8: subtotal 290,00; descuento 29,00; base 261,00; IVA 39,15; total 300,15. Precios/stock/totales sin campo editable. |
| 13 Guardado | `OrderPage.tsx`, `api/client.ts` | Navegador: venta #8 completada y factura automática al frente. Ref de exclusión y botón deshabilitado revisados. Test HTTP: un POST, respuesta entera; sin reintento tras desconexión. No se simuló pérdida real de respuesta tras commit. |
| 14 Integridad | `api/client.ts`, `OrderPage.tsx` | API real: precio alterado 409, deuda/cantidad/duplicado rechazados. Venta correcta disminuye stock en servidor. Los errores conservan el borrador. No se modificaron transacciones del backend. |
| 15 Historial | `InvoicesPage.tsx` | Navegador: factura #8 visible. Integración: seis ventas, primera página 5 y segunda 1, sin cruces. |
| 16 Comprobante | `InvoicePreview.tsx`, `PdfDocument.tsx` | Detalle y PDF original visibles en ventana al frente. PDF.js dibujó Página 1 de 1; binario `%PDF` >1000 bytes vía API/proxy. Impresión física no realizada; la descarga del navegador integrado no terminó. Se conserva descarga estándar y Abrir PDF. |
| 17 Propiedad | `api/client.ts`, `InvoicesPage.tsx`, `context.tsx` | API real: otra cuenta recibe 404 para detalle/PDF y su historial vacío; anónimo recibe 401. Navegador: factura inexistente muestra aviso común al frente. |
| 18 Restricciones | `components.tsx`, `domain/rules.ts`, `Access.tsx` | Las 35 unitarias incluyen email inválido, límite 30, Unicode, contraseñas 31/64/128/1024 sin truncar. Cuenta y login con clave larga comprobados en API/navegador. |
| 19 Avisos | `context.tsx` | Navegador: un aviso común de campos, deuda, duplicado y 404. Sin resumen rojo ni globos nativos. Entendido/Cancelar reciben foco; Escape cierra la confirmación. |
| 20 Uso/idioma | `styles.css`, componentes/páginas, `index.html` | Revisión visual en escritorio 1280×900 y móvil: español, controles uniformes, importes es-EC, lupa integrada, acciones separadas. Login a 390 px sin desbordamiento de documento; tablas tienen desplazamiento interno. |
| 21 Transporte | `api/client.ts`, `domain/session.ts`, `tools/serve-sdd.py` | Unitarias: Bearer, ProblemDetails, cancelación y PDF. Seis pruebas del servidor: SPA, proxy/query/status/Bearer/POST/PDF, ruta segura y MIME `.mjs`. Sin contraseñas ni claves servidor en almacenamiento del frontend. |
| 22 SDD | `docs`, `specs`, `tools` | Constitución, especificación, contrato, plan y tareas anteriores al código; pruebas inicialmente fallidas por módulos aún inexistentes, después verdes. Guion, matriz y decisiones actualizados con resultados reales. |

## Resultados automáticos

- `npm run check`: correcto, TypeScript estricto.
- `npm test`: **35/35** pruebas de reglas/sesión/cliente HTTP correctas.
- `npm run build`: correcto, Vite 8.3.5; worker PDF local y visor separado en carga diferida.
- `python3 -m unittest discover -s tools -p test_serve_sdd.py -v`: **6/6** correctas.
- `node tools/verify-api.mjs ... --isolated-test-database`: **8/8 grupos** de integración real correctos; crea usuarios y ventas ficticias, no ejecutarlo contra datos de defensa.

Los informes saneados están en `docs/results/verification.json` y `docs/results/api-tests.json`. Las salidas completas inicial/final y capturas se conservan en `NorthWind/artifacts/SddFrontend` (no se empaquetan como código fuente). Ver `docs/decisions.md` para las correcciones de redondeo del verificador y del visor PDF.

## Pendientes y límites declarados

- **Debian comprobado posteriormente:** después de iniciar VMware NAT en Windows se recuperó `192.168.220.147`. React se ejecutó desde `/home/alex/NorthWind-React-SDD/northwind-sdd`, con Python 3.13.5 y sin instalar Node. Pasaron las seis pruebas del servidor; `/`, `/invoices` y `/api/health` respondieron 200; `/api/orders` sin token respondió 401. La API publicada sigue activa en .NET 10.0.13 / Debian GNU/Linux 13, y su archivo privado mantuvo el mismo hash. Informe: `docs/results/debian-tests.json`.
- No se hizo impresión física. El visor y el binario PDF se verificaron; la descarga nativa del navegador integrado no terminó y no se afirma como comprobada.
- No se enviaron mensajes reales a Mailtrap en esta revisión. La venta con cuatro productos utiliza la regla existente del backend y el modo PickupDirectory de la API aislada.
- No se usó Spec Kit CLI: se siguió manualmente el flujo de desarrollo guiado por especificaciones. La suite de backend previa no se volvió a ejecutar porque no se cambió código .NET.
