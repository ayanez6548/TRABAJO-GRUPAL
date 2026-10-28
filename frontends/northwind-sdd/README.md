# NorthWind — tercer frontend con React, TypeScript y SDD

Esta entrega implementa únicamente el tercer frontend grupal. Consume la API .NET 10 existente y conserva Blazor como frontend individual. La interfaz es española; identificadores, rutas y payloads son ingleses.

Para reconstruir **toda** la aplicación, usa el [SPEC maestro](../../SPEC.md) y su [prompt](../../specs/000-northwind-system/PROMPT.md). Ese paquete consolida backend, datos, Blazor y React; esta carpeta conserva el proceso SDD y los resultados históricos del tercer frontend.

## Arranque desde PowerShell

Primero mantén el backend accesible en `http://127.0.0.1:5100`. Si está en Debian, en otra consola abre el túnel SSH que ya utilizas. No inicies una segunda API en el mismo puerto.

```powershell
cd 'C:\Users\Usuario\Desktop\AplicacionesMoviles\Estudiantes\NorthWind\frontends\northwind-sdd'
npm ci
npm run dev
```

Abre `http://127.0.0.1:5300`. Node debe ser 22.13 o posterior compatible con las dependencias; la verificación se realizó con Node 24.14. El proxy `/api` apunta a 5100 por defecto. Para otra API, antes de iniciar Vite:

```powershell
$env:API_TARGET = 'http://127.0.0.1:5110'
npm run dev
```

`5110` fue la API local aislada de la revisión; no es el puerto habitual de tu backend Debian. La aplicación no tiene usuarios ni ventas simuladas en memoria.

## Debian: servir la compilación con la API publicada

En Debian, desde la raíz de NorthWind que contiene esta carpeta:

```bash
cd ~/NorthWind-Full/NorthWind
sudo systemctl status northwind-api --no-pager
curl -fsS http://127.0.0.1:5100/health
```

Si tienes Node compatible, compila una vez:

```bash
cd frontends/northwind-sdd
npm ci
npm run build
cd ../..
bash tools/start-sdd.sh
```

Si copias el paquete `NorthWind-React-SDD.zip`, ya incluye `dist`; basta Python 3, sin instalar Node para servir:

```bash
mkdir -p ~/NorthWind-React-SDD
unzip -o ~/NorthWind-React-SDD.zip -d ~/NorthWind-React-SDD
cd ~/NorthWind-React-SDD/northwind-sdd
python3 tools/manage-sdd.py --start
python3 tools/manage-sdd.py --status
```

El servidor escucha solo en loopback, puerto 5300. `manage-sdd.py --start` lo deja en segundo plano y permite cerrar SSH; después de reiniciar Debian hay que volver a iniciarlo. `--status` comprueba su estado y `--stop` detiene únicamente esa ejecución. También puedes usar `python3 tools/serve-sdd.py --api-url http://127.0.0.1:5100` en primer plano. Desde Windows puedes usar una consola PowerShell dedicada al túnel:

```powershell
ssh -N -o ExitOnForwardFailure=yes -o ServerAliveInterval=30 -L 5300:127.0.0.1:5300 alex@192.168.220.147
```

Sustituye la IP si cambió. Mantén esa consola abierta y entra a `http://127.0.0.1:5300`. La API 5100 se consume desde Debian por el proxy; este túnel solo necesita 5300. Blazor sigue usando 5200 por separado. Si tienes un React local en 5300, detenlo con Ctrl+C antes del túnel.

`npm run preview` sirve archivos pero no configura el proxy de producción: para la defensa usa `serve-sdd.py` o Vite en desarrollo. No copies `.env.debian`, secretos JWT ni credenciales SMTP a React. Este servidor local con túnel sirve para la defensa; no constituye publicación pública con dominio/TLS.

## Recorrido funcional

1. Crear cuenta e iniciar sesión. Nombres y apellidos hasta 30 letras/espacios, correo hasta 30, contraseña sin máximo de 30.
2. Nueva venta: buscar cliente por campos y seleccionar uno sin deuda.
3. Completar entrega; buscar productos por nombre/código/precio/existencias. Los filtros se combinan mediante etiquetas y buscan automáticamente después de 400 ms.
4. Editar cantidades dentro del stock, cambiar producto conservando los demás datos y confirmar antes de quitar.
5. Revisar subtotal, descuento, base, IVA y total; guardar una sola vez. La API ejecuta la transacción e inventario.
6. Consultar historial propio y factura al frente. El detalle usa importes guardados. El PDF original se renderiza con PDF.js y permite descargar, abrir e imprimir.
7. Salir: se eliminan token, acceso a ventas/facturas y reloj.

## Evidencia SDD

El orden seguido fue constitución → especificación/clarificaciones → plan/contrato → tareas → pruebas → implementación → verificación. Los documentos se escribieron antes del código; las primeras pruebas fallaron por módulos todavía inexistentes. Se siguió el proceso manualmente; no se ejecutó una CLI de Spec Kit.

- [Constitución](docs/constitution.md)
- [Especificación con RF-01 a RF-22](specs/001-sales-frontend/spec.md)
- [Clarificaciones del usuario](specs/001-sales-frontend/clarifications.md)
- [Plan](specs/001-sales-frontend/plan.md)
- [Tareas](specs/001-sales-frontend/tasks.md)
- [Contrato real de la API](specs/001-sales-frontend/contracts/api.md)
- [Trazabilidad y resultados](specs/001-sales-frontend/validation.md)
- [Decisiones y correcciones](docs/decisions.md)
- [Comandos y guion de defensa](docs/GUION-DEFENSA-SDD.txt)

Ejemplo para explicar SDD: RF-10 exige confirmar antes de quitar → T07 lo implementa en `OrderPage.tsx` y ventana común en `context.tsx` → se verifica cancelar/confirmar/Escape. RF-12 exige redondeo decimal → `domain/rules.ts` → casos concretos en `domain/rules.test.ts` y factura real.

## Comprobaciones reproducibles

```bash
npm run check
npm test
npm run build
python3 -m unittest discover -s tools -p test_serve_sdd.py -v
```

El siguiente verificador escribe cuentas y seis ventas ficticias. Úsalo exclusivamente con una base aislada, catálogos de prueba y correo PickupDirectory. No lo ejecutes contra la base de la defensa ni producción:

```bash
node tools/verify-api.mjs http://127.0.0.1:5300/api --isolated-test-database api-tests.json
```

Los resultados distinguen pruebas automáticas, recorrido visual y pendientes. Se recuperó la red de Debian al iniciar VMware NAT en Windows. El tercer frontend está instalado en `/home/alex/NorthWind-React-SDD/northwind-sdd`: servidor activo en 5300, proxy hacia la API publicada .NET 10.0.13, seis pruebas del servidor correctas también en Debian 13 y rutas privadas con 401 sin token. Las ventas de integración siguen siendo exclusivamente las de la base aislada de Windows; no se crearon ventas de prueba en la base de defensa de Debian. La impresión física continúa sin comprobar.

Referencias metodológicas y técnicas: [GitHub Spec Kit](https://github.github.com/spec-kit/), [persistencia de especificaciones](https://github.github.com/spec-kit/concepts/spec-persistence.html), [React](https://react.dev/learn/build-a-react-app-from-scratch), [visor Mozilla PDF.js](https://mozilla.github.io/pdf.js/examples/).
