# .NET 10 migration verification — 20 October 2026

## Implementation

The solution retains its four layers and 31 product projects. The backend filter selects 23 projects. All target net10.0 / C# 14. Blazor packages and WebAssembly runtime are pinned to stable 10.0.13. The existing Windows SQL Server database receives an additive migration; Debian uses a separate PostgreSQL database with its own migration set. No existing Windows orders were deleted during this migration.

The complete previous-semester checklist is in SEMESTER-REQUIREMENTS.md. It covers unique order lines, inventory, totals, validation, paginated modal lookup, draft editing, protected invoices/PDF, transactions and concurrency.

## Executed checks

| Check | Result |
|---|---|
| Complete solution, Windows .NET 10 SDK | Build succeeded, zero warnings/errors |
| Backend, actual Debian 13 container | Release build succeeded, zero warnings/errors |
| Blazor, actual Debian 13 container | Release build succeeded, zero warnings/errors; SDK 10.0.402 |
| Debian runtime | Debian GNU/Linux 13 (trixie), SDK 10.0.402, runtime .NET 10.0.13 |
| Automated tests with PostgreSQL | 60 passed, zero failed/skipped |
| Automated tests with SQL Server LocalDB, dedicated test database | 60 passed, zero failed/skipped |
| Windows API HTTP integration | 38 checks passed |
| Debian API HTTP integration | 39 checks passed, including Linux OS assertion |
| Blazor browser flow | Sign in, customer/product modal lookup, save order, success message, owned invoice totals and browser PDF download verified |
| PDF output | Valid PDF bytes on both providers; first-page layout rendered and visually inspected |
| Notification | Debian special-order flow generated one local .eml message in PickupDirectory mode |
| Backend solution filter in Debian | 23 backend projects resolved |

Database tests include two concurrent attempts to reserve the last unit, rollback restoring inventory and rejection of tampered prices. Shared database initialization tests run sequentially; the concurrency test still runs its two sales concurrently.

HTTP checks cover registration/login, duplicate email, anonymous rejection, search by ID/name, distinct pages, invalid paging, duplicate lines, debtor rejection, insufficient stock, price conflicts, successful commit, inventory deduction, totals, owner-only invoice access, special-order notification and PDF download.

Local evidence is under `artifacts/Net10`: Net10-PostgreSql.trx, Net10-SqlServer.trx, WindowsHttp/http-results.json, DebianHttp/http-results.json, generated invoice PDFs and blazor-invoice.png. Artifacts and credentials are excluded from source packages.

## Scope and practical limits

Linux execution was verified inside a Debian 13 container on this Windows machine. A Debian desktop/VM and Linux VS Code were not installed on the host. The native installation and F5 configuration are supplied in DEBIAN.md and .vscode. The individual Blazor frontend was also built and started in a Debian 13 container. The completed sales browser flow initially used the Windows-hosted Blazor client calling the Debian backend. After switching the frontend to its Debian container, the authenticated invoice page and PDF download link were verified again.

The embedded browser exposed the PDF link but did not render its built-in PDF preview. The downloaded API document was rendered separately and checked, and the browser download produced a valid A4 PDF. Physical printing was not performed. SMTP delivery was not repeated during migration tests; Debian defaults to a local pickup directory, and Mailtrap can be configured privately as described in DEBIAN.md.

Docker's connection to NuGet failed in this environment. The actual Debian build was verified using a local feed of official cached NuGet packages, with the build network disabled. The regular Dockerfile restores from nuget.org and requires network access. The offline cache is temporary verification material, excluded from both source packages.

VAT is a configurable academic example. The PDF has no electronic fiscal authorization. No national-ID field is collected by this NorthWind model; CustomerId remains the original business code.

## Spanish interface update — 21 October 2026

The user's clarification supersedes the original English-interface requirement. Code identifiers and API field names remain English. User-facing content is now Spanish, including validation, email notifications, invoices and accessibility labels. Browser translation is unnecessary.

Verification: full solution build without warnings/errors; 60 SQL Server tests passed; 38 HTTP checks passed on an isolated Windows API; Spanish registration errors retain English JSON keys; the special-order .eml subject/body and invoice PDF were inspected. The translated PDF was rendered and its table header width adjusted for Spanish labels. Browser checks cover empty-field validation with Spanish labels, sign-in, modal catalog lookup, completed sales with Spanish confirmation, the translated invoice page and preservation of an edited shipping address. Evidence is in artifacts/SpanishInterface.

The interface was first verified locally. The native Debian copies were subsequently updated after the user authorized SSH authentication; see the native verification below. Previous container execution results above remain historical evidence for the .NET migration.

## Native Debian update completed — 21 October 2026

Both existing project copies on the user's Debian machine were updated in place: backend at /home/alex/NorthWind and full Blazor solution at /home/alex/NorthWind-Full/NorthWind. Existing .env files and appsettings JSON configurations were preserved byte for byte. Source backups are in each project's artifacts/Before-Spanish-20261021-122603 directory.

Native backend and Blazor builds completed with zero warnings and errors. The backend reports Debian GNU/Linux 13 (trixie), .NET 10.0.13 and SDK 10.0.402. Swagger now reports the Spanish API title. Browser verification through the existing SSH tunnel confirmed the Spanish registration form and required-field feedback. Invalid API registration returned Spanish messages with English JSON field names and did not create an account.

Database counts and the inventory fingerprint matched before and after the update: 5 products, 3 customers, 0 users, 0 orders/details/audit records at the time of verification. No sales or accounts were created for this check. No database schema changes were introduced by the interface update.

Both applications were restarted as background demo processes. Their logs and PID files are under artifacts/SpanishUpdate-20261021-122603 in their respective projects. tools/manage-debian-demo.py reports or stops only those managed processes after checking ownership, working directory, command and process group. See IDIOMAS.md for usage.

Evidence: artifacts/SpanishInterface/DebianNative/update-report.json, spanish-validation.json and interface-spanish-debian.png. SSH credentials were not saved in source files or archives.
