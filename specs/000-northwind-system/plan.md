# Plan SDD para construir desde cero

Este plan implementa [SPEC.md](../../SPEC.md). Se aplica a una copia nueva, no ordena volver a crear la aplicación operativa. Las tareas pendientes están en [tasks.md](tasks.md), y la definición observable de terminado en [acceptance.md](acceptance.md).

## Fase 0 — Contrato y entorno

Leer el paquete completo, extraer IDs SYS/AC y registrar clarificaciones. Mantener los requisitos explícitos ya resueltos; no volver a preguntar tecnología, idioma o contraseña máxima. Si falta una decisión, usar el valor por defecto del SPEC. Resolver solo ambigüedades materiales que no cubra el contrato. Seleccionar un directorio/base nuevos y comprobar herramientas disponibles sin cambiar servicios existentes.

Entregar: constitución del proyecto nuevo, copia versionada del SPEC, decisiones y plan de pruebas con base aislada. Puertos pueden configurarse para evitar colisión, conservando los defaults documentados.

## Fase 1 — Estructura, dominio y contratos

Crear solución de 31 proyectos según arquitectura; referencias hacia dentro, pruebas aparte. Definir DTO de API, límites de campos, Result, errores, Specification, validadores, eventos, puertos de entrada/salida, repositorios, logger y transacción. Implementar Order/OrderAggregate y redondeo decimal. Definir pruebas críticas de duplicados, límites y cálculo derivadas de AC antes de cerrar implementación.

Entregar: solución compilable y pruebas de reglas. Sin servicios HTTP/EF dentro del agregado ni proyectos vacíos usados como relleno. AC-01/05/06/07/12/18/19/22.

## Fase 2 — Persistencia y seguridad

Crear modelo común EF/Identity, migraciones por proveedor y semilla. Implementar repositorios sobre contextos abstractos, reserva atómica ordenada, consulta paginada y facturas propias. Añadir membresía, hash/lockout y JWT con validación completa. Configuración privada inyectada, no claves constantes.

Entregar: DB nueva migrada, registro/login reales y pruebas relacionales. Medir concurrencia con conexiones/transacciones distintas; InMemory no valida bloqueos SQL. AC-03/04/08/09/11/16/17/26/27.

## Fase 3 — Caso de uso y API

Implementar validación de forma y validadores de negocio condicionados a que no existan errores anteriores. Flujo completo de creación con auditoría de inicio, transacción, reserva, encabezado/detalles, auditoría éxito, evento/correo, commit y presenter. En fallo rollback/limpieza/cancelación. Conectar controllers/presenters, ProblemDetails, búsquedas y settings. PDF QuestPDF desde factura autorizada.

Entregar: las 12 operaciones del contrato, Swagger para defensa y health real. Pruebas HTTP para todos los caminos importantes, `.eml` válido, rollback forzado y aislamiento entre cuentas. AC-11/14/17/18/19/25/26/27/28/29/30/31/32/33/34/36.

## Fase 4 — Blazor individual

Crear WebAssembly y bibliotecas de vistas/gateways/ViewModels, autenticación, nav/reloj y los formularios. Reutilizar las reglas de contrato sin duplicar lógica de servidor como autoridad. Catálogos modales con filtros compactos, auto búsqueda/paginación; venta editable, confirmación para quitar, importes de solo lectura, historial y factura frontal/PDF. Aviso común sin bloques duplicados.

Entregar: recorrido integral contra API real. Comprobar teclado, textos, contraste, carga/errores, viewport móvil/escritorio, token, cantidad vacía y envío incierto. AC-02/05/06/07/10/12/13/15/16/19/20/21/22/23/24/32/33/35/36/37.

## Fase 5 — Tercer frontend React SDD

Antes del código React, crear su constitución, spec, clarificaciones, plan, tareas, contrato y matriz derivados del maestro. Organizar TypeScript estricto, domain/rules, API client, sesión/avisos y páginas/componentes. decimal.js para dinero, PDF.js/worker local para el PDF original. Proxy `/api`, abort/revisión de lecturas y sin reintento de ventas.

Entregar: mismas reglas de experiencia que Blazor, sesión de pestaña, builds reproducibles y pruebas de dominio/HTTP. Desarrollar con API real: mocks solo en pruebas automatizadas. No modificar contrato backend para adaptarlo a nombres arbitrarios del frontend. AC-02/05/06/07/10/12/13/15/16/19/20/21/22/23/24/32/33/35/36/37/39.

## Fase 6 — Debian y entrega independiente

Crear Dockerfile Debian13/Compose y scripts de claves/arranque. Configurar PostgreSQL y volúmenes, dependencias PDF y health real. Publicación nativa opcional con systemd. Compilar React y servir dist con proxy local Python, MIME de worker, fallback SPA y validaciones de rutas. Generar guía de defensa y guía para compañero con instalación propia. No fijar la IP ni copiar secretos del autor.

Entregar: arranque nuevo y reanudación sin perder datos; ensayo en Debian y clon limpio en otra carpeta. Revisar exclusiones Git y package-lock. AC-38/39/40/41.

## Fase 7 — Aceptación y cierre

Ejecutar matriz completa; corregir el contrato primero si cambia una regla. Registrar evidencia por versión y repetir solo comprobaciones afectadas por cambios/fallos. No confundir specs existentes con pruebas ya ejecutadas. Si falta una comprobación manual, declararla como pendiente.

Comandos de verificación que la implementación debe proporcionar, desde la raíz salvo donde se indica:

```text
dotnet restore NorthWind.sln
dotnet build NorthWind.sln -c Release -m:1
dotnet test tests/NorthWind.Tests/NorthWind.Tests.csproj --filter "TestCategory!=SqlServer"
node --test tools/init-demo-env.test.mjs
docker compose --profile frontend config --quiet
```

En `frontends/northwind-sdd`:

```text
npm ci
npm run check
npm test
npm run build
python3 -m unittest discover -s tools -p test_serve_sdd.py -v
```

En Windows usar `npm.cmd` si el lanzador `.ps1` está restringido y el nombre del ejecutable Python disponible. El primer filtro .NET omite las integraciones históricamente categorizadas `SqlServer`; esas pruebas deben ejecutarse **también**, separadamente, con `NORTHWIND_TEST_CONNECTION` y `NORTHWIND_TEST_PROVIDER=PostgreSql` o `SqlServer` dirigidos únicamente a base de pruebas. No pegar una contraseña real en documentación o salida compartida. Los tests que recrean la base deben rechazar un destino no aislado.

Después del build, ensayar con API real: registro/login, filtros, cambio/quitar, error de stock/deuda, venta, historial y PDF. Ensayar falloSMTP y concurrencia solo en entorno de prueba. Registrar la impresión física como pendiente si solo se abrió la vista de impresión.

No hay un número de tests impuesto para aparentar equivalencia. La cobertura de los casos AC y la evidencia real determinan la aceptación. La documentación del producto anterior puede servir de referencia, sin atribuir sus resultados a código nuevo.
