# Tareas pendientes de una reconstrucción nueva

**Todas las casillas comienzan vacías deliberadamente.** La aplicación existente ya tiene su propia implementación/evidencia; esta lista corresponde a crear otra desde [SPEC.md](../../SPEC.md). Marcar solo tras implementar y comprobar. Para cada tarea registrar archivos/commit, casos ejecutados, resultado y pendientes.

- [ ] **NEW-01 — Consolidar contrato y constitución.** SYS-01 a SYS-32; AC-42. Leer los siete documentos, registrar decisiones/versión y separar requisito/evidencia histórica.
- [ ] **NEW-02 — Crear 31 proyectos y referencias.** SYS-02; AC-01. Distribución física/solución exacta, dependencias internas, tests aparte, primera compilación.
- [ ] **NEW-03 — DTO y políticas de campos.** SYS-03/05/09/10/23; AC-02/05/06/07/12/14/19. Tipos, validadores y mensajes españoles; contraseña sin máximo de 30.
- [ ] **NEW-04 — Dominio de orden y cálculos.** SYS-15/17/20; AC-18/19/22/29. Agregado sin duplicados, decimal, Specification y evento especial.
- [ ] **NEW-05 — EF/Identity y migraciones.** SYS-04/05/31; AC-03/04. Modelo normalizado, proveedores, claves/FK/constraints, índice email, semilla sin cuentas predeterminadas.
- [ ] **NEW-06 — Registro/login/JWT/servicio de usuario.** SYS-05/06/07/08; AC-04/07/08/09/11. Hash Identity, lockout, firma/emisor/audiencia/vencimiento y propiedad.
- [ ] **NEW-07 — Consultas reales y paginación.** SYS-11/12/13/14/24/25; AC-14/16/17/33/34. AND por campos, stock positivo, orden estable, historial/JSON propios.
- [ ] **NEW-08 — Comandos, transacción y concurrencia.** SYS-15/19; AC-18/25/26/27/28. UPDATE condicional, misma transacción/contexto, rollback sin cambios parciales.
- [ ] **NEW-09 — Logger, correo y manejadores.** SYS-20/21/22; AC-28/29/30/31. Inicio/éxito/cancelación; `.eml`, SMTP configurable y fallo forzado aislado.
- [ ] **NEW-10 — API completa/presenters/errores.** SYS-06/08/18/23; AC-09/11/23/24/32. Doce operaciones, Swagger/health, códigos de error y ningún reintento de venta.
- [ ] **NEW-11 — PDF autorizado.** SYS-25/26; AC-11/22/34/35/36. Mismos importes históricos, todas las páginas, español y autenticación.
- [ ] **NEW-12 — Layout/cuenta/sesión Blazor.** SYS-03/05/06/07/09/27/28; AC-02/05/06/07/10/37. Rutas, nav/reloj privado, token y aviso común.
- [ ] **NEW-13 — Catálogos/venta/facturas Blazor.** SYS-10/11/12/13/14/15/16/17/18/24/25/26; AC-12/13/15/16/17/19/20/21/22/23/24/33/35/36. Flujo completo, filtros compactos, 400 ms y factura frontal.
- [ ] **NEW-14 — Especificación y dominio React.** SYS-02/09/17/30/32; AC-05/06/07/12/14/19/22/42. Docs antes del código React, TypeScript estricto, versiones/lock y reglas probadas.
- [ ] **NEW-15 — HTTP/sesión/avisos React.** SYS-06/07/08/18/23/27/30; AC-09/10/11/24/32. Bearer, expiración, respuesta incierta y un solo modal.
- [ ] **NEW-16 — Pantallas React/PDF.js.** SYS-03/10/11/12/13/14/15/16/17/18/24/25/26/28; AC-02/12/13/15/16/17/19/20/21/22/23/24/33/35/36/37. Paridad funcional sin mocks finales.
- [ ] **NEW-17 — Debian y proxy React.** SYS-29/30; AC-38/39. API .NET10/Linux real, dist sin Node al servir, rutasSPA, MIME del worker PDF y controles de rutas/cuerpo.
- [ ] **NEW-18 — Compose, claves y arranque equipo.** SYS-31; AC-40/41. .env privado generado/preservado, volúmenes y wrappers independientes Windows/Debian.
- [ ] **NEW-19 — Validación relacional y visual.** SYS-01 a SYS-32; AC-01 a AC-42. Ejecutar matriz completa sobre fixtures aislados y registrar fallos/pendientes con evidencia.
- [ ] **NEW-20 — Clon limpio, defensa y entrega.** SYS-29/31/32; AC-38/40/41/42. Arranque de otro compañero, guía de comandos, exclusión de secretos y trazabilidad sin afirmaciones no comprobadas.

No usar el progreso de esta lista como autorización para borrar bases, alterar una VM existente o publicar secretos. La reconstrucción se mantiene separada de la instalación de defensa.
