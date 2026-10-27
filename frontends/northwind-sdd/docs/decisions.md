# Decisiones verificables

- **26/10/2026 — Debian recuperado y comprobado:** `ens33` no tenía enlace y el servicio VMware NAT de Windows estaba detenido. El usuario lo inició con permisos de administrador. Se recuperó `192.168.220.147`, se verificó la API .NET 10.0.13 en Debian 13 y se instaló React compilado en su propia carpeta. Las seis pruebas del servidor pasaron en Linux; HTML/SPA/proxy respondieron y las facturas anónimas devolvieron 401. La configuración privada mantuvo el mismo hash antes y después. `manage-sdd.py` deja React en segundo plano sin requerir Node ni privilegios elevados.

- **26/10/2026 — Respuesta ilegible después de guardar:** se añadió un caso RF-13 que fallaba al recibir HTML en un POST exitoso. Se corrigió el aviso para recomendar revisar Facturas antes de repetir la venta, igual que ante desconexión. El cliente nunca reintenta el POST automáticamente.

- **26/10/2026 — Vista previa PDF:** el navegador integrado devolvió el PDF pero no dibujó el `iframe`; el intento de descarga del navegador integrado no terminó. Se incorpora PDF.js, distribuido dentro del build, para renderizar las páginas del PDF original en canvas, conservando descarga y apertura independiente. No se afirma haber probado impresión física ni descarga del navegador integrado. La API y el proxy sí entregan un binario `%PDF` válido.
- **26/10/2026 — Redondeo:** el primer verificador HTTP usó números binarios y esperaba 36,22 para una factura real de 36,23. Se corrigió el verificador con Decimal y redondeo HALF_UP; la aplicación ya usaba Decimal. Se verificó de nuevo contra el servidor.
- **26/10/2026 — Entorno inicial:** Debian no respondió en la IP conocida al iniciar la tarea. Las ventas de verificación se hicieron exclusivamente en `NorthWindSddReview20261026`, con correo PickupDirectory. La conectividad se recuperó posteriormente como se registra arriba; no se transfirieron esos datos de prueba a Debian.

Referencia de la implementación del visor: [ejemplos oficiales de Mozilla PDF.js](https://mozilla.github.io/pdf.js/examples/).
