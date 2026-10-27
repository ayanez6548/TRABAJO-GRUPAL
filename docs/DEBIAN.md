# Debian 13 presentation guide

The current machine also supports a published Release backend managed by systemd.
See [PUBLICACION-DEBIAN.md](PUBLICACION-DEBIAN.md) for the installed service,
automatic startup, logs and update commands. Do not start a second backend on
port 5100 while `northwind-api.service` is active. The options below describe
running the source directly or starting the existing container demonstration.

Use **Debian 13 (trixie)** for the group backend and the individual Blazor assignment. The group coordinator should record Debian as the team's selected distribution. Another group's choice is outside this repository.

The backend targets **.NET 10**, not Windows .NET Framework. Blazor uses stable **10.0.13**, checked on 20 October 2026. The main solution still contains 31 projects. `NorthWind.Backend.slnf` selects only the 23 backend projects and their dependencies.

## Option 1: run from source in Debian and VS Code

1. Copy this project folder into Debian. Open a terminal in the folder containing `NorthWind.sln`.
2. Install the SDK and VS Code:

   ```bash
   bash tools/setup-debian.sh
   dotnet --info
   code .
   ```

3. Install the recommended C# Dev Kit extension. The SDK includes the ASP.NET Core runtime. Node.js and publication to a web server are not required.
4. Use an existing reachable PostgreSQL server, or install Docker Engine following the [official Debian instructions](https://docs.docker.com/engine/install/debian/).
5. For the included isolated development database:

   ```bash
   bash tools/init-demo-env.sh
   docker compose up -d db
   bash tools/configure-debian-debug.sh
   dotnet build NorthWind.Backend.slnf
   bash tools/start-backend.sh
   ```

   The scripts generate private `.env` and `.env.debian` files with random credentials. These files are excluded from source packages and version control. Existing credentials are preserved on subsequent runs.

6. Open `http://localhost:5100/swagger`. The `/health` endpoint displays the target, actual .NET runtime and OS. Use this as evidence during the presentation.
7. Alternatively, select **NorthWind Backend - Debian / .NET 10** in VS Code and press **F5**. The debugger uses `.env.debian`, builds only the backend and opens Swagger.

For a PostgreSQL server managed outside Docker, set `Database__Provider=PostgreSql` and `ConnectionStrings__NorthWind` in `.env.debian`. The database account must be able to create the development database or migrate a database created by the administrator.

## Option 2: reproducible backend container on Debian 13

```bash
bash tools/init-demo-env.sh
docker compose up -d --build api
docker compose exec api cat /etc/os-release
docker compose exec api dotnet --info
```

`Dockerfile.debian` starts from `debian:13-slim` and installs Microsoft's .NET 10 SDK. The default Compose services are PostgreSQL and the backend. The frontend is excluded from this presentation command. These commands run from source; publishing is optional.

After changing source code, rebuild the affected images to include the changes. `--no-build` reuses an existing image, which may still contain the previous interface. A fresh Debian machine needs network access for the initial build. See IDIOMAS.md for the 21 October Spanish-interface update.

Swagger: `http://localhost:5100/swagger`. Stop the demo with `docker compose stop`. The database and local email volumes are preserved.

## Individual Blazor assignment on the same Debian version

With the backend running, open another terminal in Debian:

```bash
bash tools/start-frontend.sh
```

Open `http://localhost:5200`. For the container variant, the frontend uses the same Debian 13 base:

```bash
docker compose --profile frontend up -d --build frontend
```

The browser calls `http://localhost:5100`. If the browser runs on another machine, update the client's `wwwroot/appsettings.json`, API `AllowedOrigins` and exposed network addresses consistently.

## Database and email

Windows supports the existing SQL Server LocalDB database. Debian uses PostgreSQL through Npgsql. The contexts share entities and rules, with separate migrations per provider. The Windows database is not copied into PostgreSQL automatically; the Debian database is initialized with test customers and products. Existing SQL Server orders receive VAT rate zero, preserving their original treatment. New orders snapshot the configured VAT rate.

The Debian demo defaults to `PickupDirectory`: special orders generate `.eml` files in the email volume. To use the previously configured Mailtrap sandbox, privately set `SMTP_MODE=Smtp`, `SMTP_USERNAME` and `SMTP_PASSWORD` in `.env`, then recreate the API service with `docker compose up -d api`. Windows user secrets are not included in a source archive.

## Demonstration sequence

1. Show Debian's `/etc/os-release` and `dotnet --info`.
2. Start only the backend; open Swagger and `/health`.
3. Register and log in. Paste `accessToken` into Swagger's **Authorize** control.
4. Search customers/products by ID and name. Check pagination and positive stock.
5. Create an order with distinct products and current catalog prices. Query the saved invoice and show decreased stock.
6. Download `/orders/{id}/invoice.pdf`.
7. Demonstrate rejected duplicates, overselling and changed prices.
8. Run the individual Blazor frontend separately when required.

For an automated HTTP check:

```bash
python3 tools/smoke-test.py --require-linux
```

This creates two test sales and triggers the configured special-order notification. It saves an invoice PDF and the result report under `artifacts/Net10`.

## Official references

- [Install .NET on Debian](https://learn.microsoft.com/en-us/dotnet/core/install/linux-debian)
- [Install VS Code on Linux](https://code.visualstudio.com/docs/setup/linux)
- [Stable Blazor WebAssembly package](https://www.nuget.org/packages/Microsoft.AspNetCore.Components.WebAssembly/10.0.13)
- [Npgsql EF Core 10](https://www.npgsql.org/efcore/release-notes/10.0.html)
- [Migrations for multiple EF providers](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/providers)
