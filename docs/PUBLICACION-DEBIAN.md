# Backend publicado como servicio en Debian

La instalación del equipo usa `northwind-api.service`, ejecutado como `alex`, con
la publicación Release de `/home/alex/NorthWind/publish/backend`. El entorno es
Production. La API escucha en `127.0.0.1:5100`; se accede desde Windows mediante
el túnel SSH existente. Esta instalación no publica el servicio en Internet.

El servicio lee `/home/alex/NorthWind/.env.debian`, con permisos 600. Conserva
PostgreSQL, JWT y SMTP. Las migraciones automáticas están deshabilitadas en esta
ejecución publicada. Swagger se habilita expresamente con `Swagger:Enabled`
para la defensa local; su valor predeterminado fuera de Development es falso.

## Consultar y administrar

En Debian:

```bash
sudo systemctl status northwind-api --no-pager
systemctl is-enabled northwind-api
curl -sS http://localhost:5100/health | python3 -m json.tool
sudo journalctl -u northwind-api -n 50 --no-pager
sudo journalctl -u northwind-api -f
```

Ctrl+C termina el seguimiento de los registros; la API sigue activa.
Los registros técnicos están en journalctl. La auditoría del negocio continúa
en la tabla DomainLogs.

```bash
sudo systemctl restart northwind-api
sudo systemctl stop northwind-api
sudo systemctl start northwind-api
```

`systemctl stop` detiene la API hasta iniciarla otra vez o reiniciar Debian;
para desactivar también el inicio automático, usa:

```bash
sudo systemctl disable --now northwind-api
```

Para volver a activarlo:

```bash
sudo systemctl enable --now northwind-api
```

## Arranque de la base de datos

Docker está habilitado al arrancar Debian. El contenedor existente de PostgreSQL
tiene `restart: unless-stopped`, conserva su volumen y no se recrea al iniciar
la API. El servicio espera al puerto configurado de PostgreSQL antes de arrancar.
Si detienes el contenedor manualmente, vuelve a iniciarlo desde la carpeta del backend:

```bash
cd /home/alex/NorthWind
sudo docker compose up -d db
sudo systemctl restart northwind-api
```

## Actualizar una publicación

Guarda primero un respaldo de la publicación actual y prepara la nueva versión
en una carpeta separada. Para regenerar directamente la carpeta actual, detén
el servicio durante la publicación:

```bash
cd /home/alex/NorthWind
sudo systemctl stop northwind-api
dotnet publish "src/4. Frameworks And Drivers/UI/NorthWind.Sales.WebApi/NorthWind.Sales.WebApi.csproj" -c Release -o ./publish/backend
sudo systemctl start northwind-api
curl -sS http://localhost:5100/health | python3 -m json.tool
```

No inicies otra API con `dotnet run` en el mismo puerto. El script
`tools/start-backend.sh` detecta el servicio activo y muestra cómo administrarlo.

## Frontend individual

Esta instalación registra solamente el backend como servicio. Blazor conserva
su ejecución independiente y apunta a la misma API. Después de reiniciar Debian,
si el frontend no está iniciado:

```bash
python3 /home/alex/NorthWind-Full/NorthWind/tools/manage-debian-demo.py --start frontend
```

En Windows, mantén el túnel SSH cuando uses el navegador:

```powershell
ssh -o ExitOnForwardFailure=yes -L 5100:localhost:5100 -L 5200:localhost:5200 alex@192.168.220.147
```

No abras otro túnel si esos puertos ya están ocupados por el túnel existente.
Swagger: http://localhost:5100/swagger. Blazor: http://localhost:5200.

## Archivos de la instalación

- Unidad instalada: `/etc/systemd/system/northwind-api.service`.
- Plantilla: `tools/systemd/northwind-api.service`.
- Espera de PostgreSQL: `tools/systemd/wait-database.py`.
- Estado privado del runtime: `/var/lib/northwind-api`.
- Respaldos y verificación: `artifacts/PublishedService`.

Se verifican servicio activo y habilitado, reinicio del proceso, Swagger,
autenticación, CORS del frontend y conservación de la base de datos.
El reinicio completo de Debian no forma parte de la instalación automática.

Referencia: [Hospedar ASP.NET Core en Linux](https://learn.microsoft.com/en-gb/aspnet/core/host-and-deploy/linux-nginx?view=aspnetcore-10.0).
