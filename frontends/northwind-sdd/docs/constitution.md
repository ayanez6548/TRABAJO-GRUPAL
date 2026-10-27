# Constitución del tercer frontend NorthWind

Versión 1.0 · 26 de octubre de 2026.

1. El alcance es un tercer frontend React con TypeScript, construido mediante SDD. Los otros dos frontends del grupo no forman parte de esta entrega.
2. La API .NET 10 existente es la autoridad sobre usuarios, precios, stock, transacciones, facturas y correos. No se crean bases ni reglas de servidor alternativas.
3. Blazor y el backend publicado conservan su funcionamiento. El nuevo frontend ocupa su propia carpeta y puerto.
4. Identificadores y código en inglés; interfaz, mensajes y documentos de defensa en español.
5. Los requisitos se escriben antes del código. Cada tarea y evidencia identifica los RF que verifica; los cambios de alcance actualizan primero la especificación.
6. Ninguna contraseña o credencial del servidor se incluye en código, logs, capturas, repositorio o paquetes. La aplicación no persiste contraseñas.
7. La validación de interfaz mejora la interacción; el backend mantiene sus validaciones y autorización. Ningún control visual sustituye la protección de la API.
8. Las pruebas de ventas usan una base aislada. No se modifican ventas, usuarios, inventario ni configuración privada de Debian para preparar datos de prueba.
9. El flujo final usa la API real. Los dobles de prueba se limitan a pruebas automatizadas del cliente.
10. Se comprueban tipos, compilación, reglas críticas, contrato HTTP y recorrido visual; los resultados se registran con sus límites.

Esta constitución aplica únicamente a `frontends/northwind-sdd`. Las decisiones del usuario se conservan en la clarificación de la especificación.
