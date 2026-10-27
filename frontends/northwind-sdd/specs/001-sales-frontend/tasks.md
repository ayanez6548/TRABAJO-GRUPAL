# Tareas derivadas de Spec 001

- [x] **T01 — Definir alcance y contratos.** RF-01 a RF-22. Hecho cuando existen constitución, especificación, clarificaciones, plan y contrato API anteriores al código.
- [x] **T02 — Preparar proyecto y pruebas de aceptación de dominio.** RF-08/09/11/12/18/21. Hecho cuando hay scripts reproducibles, lockfile y pruebas derivadas de los RF.
- [x] **T03 — Implementar dominio y cálculos.** RF-08/09/11/12/18. Hecho cuando pasan casos válidos, límites y errores de reglas.
- [x] **T04 — Implementar HTTP y sesión.** RF-01/02/03/14/17/21. Hecho cuando el contrato se verifica con Bearer, errores y cancelación.
- [x] **T05 — Construir layout, acceso y avisos.** RF-01/02/03/18/19/20. Hecho cuando registro/login usan las mismas reglas y una ventana común.
- [x] **T06 — Construir búsquedas.** RF-04/05/06/07. Hecho cuando filtros, debounce, cancelación y páginas consultan la API real.
- [x] **T07 — Construir venta.** RF-08/09/10/11/12/13/14. Hecho cuando una venta válida se guarda y los errores conservan el borrador.
- [x] **T08 — Construir historial y comprobante.** RF-15/16/17. Hecho cuando se consultan solo facturas propias y se obtiene el PDF del servidor.
- [x] **T09 — Verificar con API real y navegador.** RF-01 a RF-21. Hecho cuando hay evidencias sobre base aislada, chequeo de tipos, pruebas y build satisfactorios.
- [x] **T10 — Preparar ejecución Linux, trazabilidad y defensa.** RF-22. Hecho cuando instrucciones, matriz y resultados reflejan exactamente lo comprobado.

Las casillas se marcan después de verificar; no representan aprobaciones humanas adicionales. Los hallazgos o cambios se anotan primero en la especificación.

Verificación final: 35 pruebas de dominio/HTTP, 6 del servidor, 8 grupos de integración real y recorrido visual sobre la base aislada. T10 incluye instalación React en Debian 13 y repetición de las 6 pruebas del servidor allí; HTML/proxy, API Linux y autenticación 401 comprobados. No se crearon ventas de prueba en la base de defensa de Debian.
