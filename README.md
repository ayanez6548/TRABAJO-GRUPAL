# NorthWind — .NET 10 and Blazor

**Para los compañeros:** sigue [la guía de entrega completa](docs/EQUIPO.md) para preparar una copia independiente con API, PostgreSQL, Blazor y React SDD, sin depender de la máquina ni las contraseñas de Alexander. Arranque en Windows o Debian: `node tools/start-team-demo.mjs`.

The third group frontend now lives in [frontends/northwind-sdd](frontends/northwind-sdd/README.md): React + TypeScript with specifications, plan, tasks and verification evidence. It uses port 5300 and the existing API; the individual Blazor frontend remains on 5200. Start it with `npm run dev` in that directory, or serve its compiled files on Debian as documented there.

The 31-project solution implements practices 01–16 and the previous-semester sales requirements in four Clean Architecture layers. All projects target .NET 10 / C# 14. Blazor WebAssembly uses stable 10.0.13. The backend presentation uses Debian 13 and PostgreSQL. Windows retains SQL Server LocalDB and its existing data.

## Debian backend

Follow [the Debian guide](docs/DEBIAN.md) for SDK installation, VS Code debugging and the individual Blazor presentation on the same distribution. Publication is optional.

```bash
bash tools/init-demo-env.sh
docker compose up -d --build api
```

Open http://localhost:5100/swagger and inspect http://localhost:5100/health for the actual OS and runtime. This command starts only the backend and database. Run Blazor separately with `bash tools/start-frontend.sh` after installing the SDK, or use the optional Compose `frontend` profile.

## Windows

Use a Visual Studio release supporting .NET 10, the .NET 10 SDK and SQL Server LocalDB. Open **NorthWind.sln** as a solution, select **NorthWind - App + Swagger + Mailtrap**, then press F5. It starts the API on port 5100 and Blazor on port 5200, and opens the app, Swagger and Mailtrap. Stop other instances using those ports before starting this profile.

The database remains `NorthWindPracticas8`; its historical name does not determine the target framework. Startup applies additive migrations. Mailtrap credentials remain in local user secrets and are excluded from source packages. The existing user-secrets identifier is preserved for compatibility.

## Interface language

Code identifiers, routes and API payload names remain English. The visual interface, validation messages, emails and invoice PDFs are Spanish. HTML declares Spanish and the Blazor client uses es-EC formatting; browser translation is unnecessary. See [the Spanish interface update guide](docs/IDIOMAS.md) to update the existing Debian copies. Existing product/customer names remain business data.

## Sales flow

Register and sign in, then create an order. Customer and product dialogs search by ID and name with server pagination. Prices, stock and totals are read-only. Only products with positive stock are offered. Draft lines can be changed or removed; duplicate products, invalid quantities and overselling are rejected. The backend checks prices and reserves stock atomically inside the order transaction.

Saved invoices contain the header, details, subtotal, discount, configured VAT and total. Open **Facturas** to retrieve an owned order and preview, download or print its PDF. Other users cannot access that invoice. Existing orders retain their original VAT treatment; new orders save the configured rate (15% for this academic demo). This is an academic invoice without electronic fiscal authorization.

Special orders with at least four distinct products trigger the practice notification. Windows can use Mailtrap SMTP; Debian defaults to local `.eml` files. Domain audit records are stored in `DomainLogs`; operational diagnostics appear in the API console. See the Debian guide for SMTP configuration.

## Build and verify

```bash
dotnet restore NorthWind.sln
dotnet build NorthWind.sln --no-restore
dotnet test tests/NorthWind.Tests/NorthWind.Tests.csproj --filter 'TestCategory!=SqlServer'
python3 tools/smoke-test.py --require-linux
```

The last command requires a running Linux backend and creates test sales. Integration tests require `NORTHWIND_TEST_CONNECTION` and `NORTHWIND_TEST_PROVIDER` (`PostgreSql` or `SqlServer`); use a test database. The historical `SqlServer` test category covers both relational providers. HTTP verification saves a PDF and JSON report under `artifacts/Net10`.

- [Requirement implementation](docs/SEMESTER-REQUIREMENTS.md)
- [Normalized entity relationship model](docs/DATA-MODEL.md)
- [Current verification](docs/NET10-VERIFICATION.md)
- [31-project distribution](ESTRUCTURA.md)

Older practice reviews describe the original .NET 8 baseline. The current requirements and verification documents supersede those historical reports.
