# Plan de implementación

Escrito después de la especificación y antes del código del frontend.

## Estructura

- `src/domain`: tipos del contrato, reglas de campos, borrador, filtros, sesión y cálculos decimales. RF-08/09/11/12/18.
- `src/api`: cliente HTTP con Bearer, cancelación, ProblemDetails, PDF y rutas documentadas. RF-01/02/04/05/13/14/15/16/17/21.
- `src/context.tsx`: sesión de pestaña y ventana común para avisos/confirmaciones. RF-02/03/10/17/19/21.
- `src/components.tsx`, `CatalogLookup.tsx`, `InvoicePreview.tsx`, `PdfDocument.tsx`: campos controlados, catálogos y factura/PDF. RF-04/05/06/07/10/16/19/20.
- `src/App.tsx`, `Access.tsx`, `OrderPage.tsx`, `InvoicesPage.tsx`: layout/reloj, inicio, registro, login, nueva venta e historial. RF-01/02/03/08/09/11/12/13/14/15/17/20.
- `src/**/*.test.ts`: reglas y contrato HTTP con pruebas críticas definidas desde los RF.
- `tools/serve-sdd.py`: servidor de archivos compilados y proxy local a la API, con destino fijo por configuración.
- `docs` y `specs`: proceso SDD, uso, trazabilidad y defensa. RF-22.

## Decisiones

1. Una SPA React/TypeScript consume el backend ya existente. No se introduce otro backend de ventas.
2. Vite compila y permite proxy durante el desarrollo. La presentación puede servir `dist` con Python de Debian, sin depender de un servidor de desarrollo ni instalar Node en la máquina de presentación.
3. El navegador usa `/api` en el mismo origen. El proxy retira ese prefijo y reenvía al backend configurado, por defecto `127.0.0.1:5100`. No es necesario ampliar CORS ni modificar el servicio .NET.
4. La sesión de la pestaña conserva token/vencimiento y se elimina al cerrar sesión. El backend sigue validando autenticación y propiedad; decodificar datos visibles del JWT no equivale a validar su firma.
5. Los importes del borrador usan aritmética decimal y redondeo a centavos; la factura guardada y su PDF siempre se obtienen del servidor.
6. Las cantidades se mantienen como texto durante la edición para detectar vacíos y evitar guardar silenciosamente el último número válido.
7. Las lecturas usan AbortController y control de versión. No hay reintentos automáticos de ventas.
8. Las ventanas usan el elemento dialog. Hay un único mecanismo de avisos y confirmación; la factura tiene su propia vista previa.
9. Las dependencias se obtienen del registro npm, con versiones y lockfile registrados al construir.
10. Ajuste verificado: PDF.js renderiza el PDF original con un worker local, porque el navegador integrado no dibuja un iframe PDF. El proxy declara `.mjs` como JavaScript. Ver `docs/decisions.md`.

## Estrategia de verificación

- Reglas: cuentas, correo, contraseña larga, envío, duplicados, cantidades, sustitución, filtros y redondeo.
- HTTP: rutas reales, query strings, token, payload de venta, ProblemDetails, desconexión y PDF.
- Tipos y compilación de producción.
- Recorrido visual con API real y base aislada: acceso, filtros, stock, confirmación, venta, historial y factura.
- Debian: compilar/servir el nuevo frontend sin modificar el backend publicado; verificar cuando esté accesible.
- Documentar resultados reales y limitaciones; no dar por realizada impresión física ni entregar mocks como aplicación final.
