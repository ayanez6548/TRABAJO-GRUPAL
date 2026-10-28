# Prompt para reconstruir NorthWind con este SPEC

Uso: copia el bloque siguiente en la herramienta de desarrollo que vayas a usar y adjunta `SPEC.md` y los seis archivos de `specs/000-northwind-system/`. Si la herramienta puede leer la carpeta, basta indicar esas rutas. El prompt solo, sin los documentos, no contiene todas las reglas.

```text
Implementa desde cero la aplicación NorthWind definida en este paquete:

1. SPEC.md
2. specs/000-northwind-system/architecture.md
3. specs/000-northwind-system/api-contract.md
4. specs/000-northwind-system/acceptance.md
5. specs/000-northwind-system/plan.md
6. specs/000-northwind-system/tasks.md
7. specs/000-northwind-system/PROMPT.md

El resultado requerido es código funcional completo: backend .NET 10 ejecutable
en Debian 13 y PostgreSQL, cliente Blazor WebAssembly .NET 10, tercer frontend
React con TypeScript y documentación/pruebas del proceso SDD. No te limites a
generar otra especificación, un mockup o una propuesta.

Lee todos los documentos antes del primer cambio. Trabaja en una carpeta/base
nueva e independiente. No necesitas copiar código, datos ni credenciales de una
instalación anterior. No modifiques ni borres una instalación de defensa existente.

Respeta la precedencia del SPEC y sus valores por defecto. Conserva:
- 31 proyectos C# de producto con su distribución de cuatro capas; tests aparte.
- Identificadores y payloads en inglés; toda interacción visible en español.
- Nombres/apellidos/correo máximo de 30; contraseña SIN máximo de 30, sin truncar/trim.
- Correo con formato y caracteres del contrato; restricciones de texto/números.
- Una sola aparición por producto, cliente sin deuda y stock positivo.
- Descuento10%, IVA académico0.15 configurable, cálculo decimal consistente.
- Reserva atómica de inventario, autorización por token y facturas propias.
- Cuatro productos distintos activan evento/correo; fallo antes del commit revierte.
- Avisos únicos al frente, confirmación al quitar y factura en subventana.
- Filtros compactos por campos, combinables, paginados, búsqueda tras 400 ms.
- Ocultar ventas/facturas/reloj sin sesión y proteger también rutas y API.
- PDF original del backend; historial persistido y precios/tasa históricos.
- Sin reintentar automáticamente una venta de resultado incierto.
- Claves propias privadas, configuración reproducible y documentación de Debian.

Sigue SPEC -> clarificaciones -> plan -> tareas -> implementación/pruebas ->
verificación. Registra tus decisiones y relaciona cada tarea con SYS y AC.
No afirmes que el SPEC maestro precedió al código del proyecto original: es una
consolidación; sí precede a ESTA implementación nueva. No inventes aprobaciones
del docente ni el uso de una CLI de Spec Kit. SDD no obliga a usar una CLI.

Avanza por las fases del plan hasta entregar el flujo completo. Cuando una
dependencia no esté disponible, conserva el contrato y registra el bloqueo real;
no sustituyas la API final por datos simulados ni marques el trabajo terminado.
Los mocks solo son aceptables en pruebas. Verifica versiones/compatibilidad si
cambias las versiones de referencia y fija dependencias reproducibles.

Implementa los scripts de arranque, migraciones, semilla sin cuentas por defecto,
contrato HTTP, pruebas de reglas/HTTP/concurrencia, ambos clientes y despliegue
local Debian. Usa bases aisladas para las pruebas que guardan ventas o fuerzan
fallos. No lleves contraseñas, SMTP/JWT ni .env al frontend, repositorio o logs.

Ejecuta los casos de acceptance.md y los comandos del plan que correspondan a
tu implementación. Aporta comandos/resultados reales, versiones, entorno,
trazabilidad y pendientes. Las pruebas anteriores del repositorio no cuentan
como pruebas de tu código nuevo. No asegures impresión física o entrega SMTP
externa si solo comprobaste vista previa o archivos .eml.

La entrega debe permitir que otro compañero la clone, genere sus propias claves,
registre su cuenta y demuestre registro/login, búsquedas, validaciones, venta,
stock, transacción, historial, PDF y correo sin depender de la máquina del autor.
Incluye README, guía del equipo, modelo ER y TXT con comandos de defensa.
```

## Para usarlo sobre el proyecto ya existente

Si el objetivo es revisar o ampliar la solución actual, cambia la primera frase por «Revisa la solución existente contra este paquete y corrige solo los incumplimientos demostrados». Conserva datos, configuración privada y compatibilidad. La solicitud de un SPEC no exige rehacer ni reinstalar la app actual.
