# Entrega completa para el equipo

Esta guía permite que un compañero prepare su propia máquina y presente el proyecto sin depender del equipo, la cuenta, la IP ni la máquina virtual de Alexander.

## Contenido del repositorio

- `src/`: backend .NET 10 y frontend Blazor, con las capas y las prácticas NorthWind.
- `frontends/northwind-sdd/`: tercer frontend React + TypeScript, especificaciones, plan, tareas, pruebas y trazabilidad SDD.
- `tests/`: pruebas del backend.
- `docs/`: modelo de datos, requisitos y guías de ejecución/defensa.
- `tools/`: preparación local, arranque y verificaciones.
- `compose.yaml` / `Dockerfile.debian`: PostgreSQL, API y Blazor en contenedores basados en Debian 13.

Se comparte código y datos iniciales definidos en migraciones. **Las cuentas, órdenes, facturas y logs existentes en la máquina de Alexander no se transfieren por Git.** Cada compañero registra una cuenta y crea sus ventas de demostración. No necesita la contraseña de Debian ni claves de Mailtrap de otra persona.

## Preparación antes de la defensa

Instala Git, Docker con Compose y Node compatible (probado con Node 24.14; mínimo de dependencias 22.13). En Windows inicia Docker Desktop con contenedores Linux. En Debian 13 puede usarse Docker Engine y Compose. La compilación .NET se hace dentro del contenedor; esta opción no exige instalar el SDK .NET en el host.

Clona la URL compartida en GitHub y entra a la carpeta que contiene `NorthWind.sln`. Para ejecutar una copia independiente, usa tu propia máquina o VM; los puertos 5100, 5200 y 5300 deben estar libres. La primera ejecución requiere Internet y debe prepararse con anticipación para descargar imágenes y paquetes.

```bash
git clone https://github.com/ayanez6548/TRABAJO-GRUPAL.git NorthWind
cd NorthWind
node tools/start-team-demo.mjs
```

Si GitHub solicita acceso, el propietario debe darte permiso al repositorio. Para actualizar una copia que no tiene cambios locales: `git pull --ff-only`. No compartas contraseñas entre compañeros ni subas `.env`; los inicializadores crean claves independientes.

El repositorio contiene actualmente dos implementaciones de frontend: Blazor individual y React SDD. Los otros frontends del trabajo grupal deben aportarlos sus responsables; no se afirma que ya estén en esta entrega.

## Arranque completo en Windows

En PowerShell, dentro de la carpeta clonada:

```powershell
node tools/start-team-demo.mjs
```

Este script genera un `.env` privado con claves aleatorias si falta, inicia PostgreSQL + API + Blazor mediante Compose, instala React con el lockfile y abre su servidor de desarrollo. Mantén la consola de React abierta.

También puedes realizar cada paso por separado:

```powershell
node tools/init-demo-env.mjs
docker compose --profile frontend up -d --build
cd frontends\northwind-sdd
npm.cmd ci
npm.cmd run dev
```

El arranque con Node funciona sin ejecutar archivos PowerShell: no requiere cambiar políticas de ejecución. `npm.cmd` evita depender del lanzador PowerShell de npm. Se conserva `Initialize-Demo.ps1` para quienes ya lo usan, con APIs compatibles con Windows PowerShell 5.1 y PowerShell 7.

## Arranque completo en Debian 13

Dentro de la carpeta clonada:

```bash
node tools/start-team-demo.mjs
```

O por pasos:

```bash
node tools/init-demo-env.mjs
docker compose --profile frontend up -d --build
cd frontends/northwind-sdd
npm ci
npm run dev
```

Los scripts conservan un `.env` ya existente; cada equipo usa sus propias claves. No copies archivos `.env` ni directorios de base de datos de otro equipo para esta instalación.

## Direcciones que hay que abrir

| Componente | Dirección |
|---|---|
| API y Swagger | `http://localhost:5100/swagger` |
| Salud y sistema de la API | `http://localhost:5100/health` |
| Blazor individual | `http://localhost:5200` |
| React SDD grupal | `http://localhost:5300` |

Abre estas páginas en el navegador de la misma máquina donde se ejecutan los servicios. Para usar el navegador Windows con servicios dentro de una VM Debian, abre túneles en una PowerShell y reemplaza usuario/IP por los tuyos:

```powershell
ssh -N -o ExitOnForwardFailure=yes -L 5100:127.0.0.1:5100 -L 5200:127.0.0.1:5200 -L 5300:127.0.0.1:5300 usuario@IP-DE-TU-DEBIAN
```

Reutiliza los túneles que ya tengas abiertos; evita iniciar dos ejecuciones en el mismo puerto. La opción usa loopback y SSH, no una publicación pública con dominio.

## Comprobación de la instalación

Desde la raíz del repositorio:

```bash
docker compose ps
docker compose logs --tail 50 api
```

Abre `/health`: debe responder `status: ok`, `framework: net10.0` y un sistema Linux cuando la API está en el contenedor Debian. Comprueba las páginas de Blazor y React.

1. Registra tu propia cuenta con un correo válido de hasta 30 caracteres e inicia sesión.
2. Selecciona ALFKI o ANATR (sin deuda) y completa la dirección, ciudad y país.
3. Busca productos y revisa filtros por campos. La semilla incluye Chai, Aniseed Syrup y dos productos Chef Anton con stock positivo; Chang agotado no debe ofrecerse.
4. Comprueba el rechazo de duplicados y exceso de stock, el cambio de producto y la confirmación al quitar.
5. Guarda una venta. Abre su factura, visualiza el PDF y consulta el historial propio.
6. Guarda una orden con al menos cuatro productos distintos para demostrar la notificación de la práctica.
7. Cierra sesión y verifica que desaparezcan ventas, facturas y reloj.

No cambies el IVA para intentar replicar una tasa vigente: el 15 % configurado corresponde a esta demostración académica y se conserva por factura.

## Correos sin depender de Mailtrap

El modo inicial es `PickupDirectory`: la aplicación genera correos `.eml` y no necesita una cuenta externa. La notificación se activa con cuatro o más productos distintos, según las prácticas.

Para copiar los correos del contenedor a tu carpeta local, crea primero la carpeta `artifacts` si no existe y, desde la raíz, ejecuta:

```bash
docker compose cp api:/workspace/App_Data/mail ./artifacts/team-mail
```

Si deseas usar tu propio sandbox Mailtrap, configura privadamente `SMTP_MODE=Smtp`, `SMTP_USERNAME` y `SMTP_PASSWORD` en `.env`; recrea la API con `docker compose up -d api`. No subas esas credenciales al repositorio. Conservar el modo `.eml` permite presentar la funcionalidad sin depender de acceso a otra cuenta.

## Documentación que debe conocer quien exponga

- [Requisitos del semestre y su implementación](SEMESTER-REQUIREMENTS.md)
- [Modelo entidad-relación](DATA-MODEL.md)
- [Estructura por capas](../ESTRUCTURA.md)
- [Especificación SDD](../frontends/northwind-sdd/specs/001-sales-frontend/spec.md)
- [Plan SDD](../frontends/northwind-sdd/specs/001-sales-frontend/plan.md)
- [Tareas SDD](../frontends/northwind-sdd/specs/001-sales-frontend/tasks.md)
- [Trazabilidad y evidencia](../frontends/northwind-sdd/specs/001-sales-frontend/validation.md)
- [Guion detallado de defensa React](../frontends/northwind-sdd/docs/GUION-DEFENSA-SDD.txt)

El guion detallado contiene también rutas/IP de la instalación de Alexander; para otra máquina sigue las rutas relativas de esta guía. Explica SDD mostrando requisito → tarea → implementación → prueba, además del recorrido visual. No se utilizó una CLI de Spec Kit ni se inventaron aprobaciones del docente.

## Detener y volver a iniciar

- Ctrl+C en la consola React detiene React.
- `docker compose --profile frontend stop` detiene API, Blazor y PostgreSQL de esta copia y conserva sus volúmenes.
- Para volver a presentar: `docker compose --profile frontend up -d`, después `npm run dev` en la carpeta React.
- Apagar la VM impide usar servicios que estén ejecutándose allí.

## Alcance de la verificación

Se verificaron reglas/HTTP de React, integración real sobre una base aislada Windows y ejecución del servidor/proxy en Debian 13 con la API publicada. El inicializador Node tiene pruebas de claves aleatorias, conservación de configuración existente e independencia entre copias. La prueba de ejecución del script antiguo en PowerShell 5.1 fue bloqueada por AuthorizationManager; por eso se ofrece el arranque directo con Node, sin cambiar políticas. El arranque conjunto Compose + React se entrega para una copia nueva; no se ejecuta sobre la instalación activa de Alexander para evitar colisiones de puertos y cambios en sus datos. Cada compañero debe completar el recorrido anterior antes de la exposición.
