# Distribución de la solución

Actualizada el 20 de octubre de 2026. `NorthWind.sln` contiene 31 proyectos de producto. Todos usan .NET 10 y C# 14.

```text
NorthWind (31 proyectos)
├── 1. Enterprise Business Rules
│   ├── NorthWind.DomainLogs.Entities
│   ├── NorthWind.Entities
│   ├── NorthWind.Events.Entities
│   ├── NorthWind.Exceptions.Entities
│   ├── NorthWind.HttpDelegatingHandlers
│   ├── NorthWind.Membership.Entities
│   ├── NorthWind.Result.Entities
│   ├── NorthWind.Sales.Entities
│   ├── NorthWind.Sales.Validators.Entities
│   ├── NorthWind.Transactions.Entities
│   └── NorthWind.Validation.Entities
├── 2. Application Business Rules
│   ├── NorthWind.Membership.Backend.Core
│   ├── NorthWind.Sales.Backend.BusinessObjects
│   ├── NorthWind.Sales.Backend.UseCases
│   └── NorthWind.Sales.Frontend.BusinessObjects
├── 3. Interface Adapters
│   ├── Controllers
│   │   └── NorthWind.Sales.Backend.Controllers
│   ├── Gateways
│   │   ├── EmailGateways
│   │   │   └── NorthWind.Sales.Backend.SmtpGateways
│   │   ├── Repositories
│   │   │   └── NorthWind.Sales.Backend.Repositories
│   │   └── WebApiGateways
│   │       └── NorthWind.Sales.Frontend.WebApiGateways
│   ├── Presenters
│   │   └── NorthWind.Sales.Backend.Presenters
│   └── Views
│       ├── NorthWind.Membership.Frontend.RazorViews
│       ├── NorthWind.RazorComponents
│       └── NorthWind.Sales.Frontend.Views
└── 4. Frameworks And Drivers
    ├── Frameworks
    │   ├── NorthWind.Membership.Backend.AspNetIdentity
    │   └── NorthWind.Sales.Backend.DataContexts.EFCore
    ├── IoC
    │   ├── NorthWind.Sales.Backend.IoC
    │   └── NorthWind.Sales.Frontend.IoC
    ├── Libraries
    │   ├── NorthWind.UserServices
    │   └── NorthWind.ValidationService.FluentValidation
    ├── UI
    │   └── NorthWind.Sales.WebApi
    └── UIs
        └── NorthWind.Sales.BlazorClient
```

La distribución de carpetas físicas coincide con las carpetas de solución. No hay proyectos vacíos: cada biblioteca contiene el código que corresponde a su responsabilidad.

Las pruebas están fuera de la solución, en `tests/NorthWind.Tests`, para conservar los 31 proyectos y las cuatro capas de las capturas. Ejecución:

```powershell
dotnet test tests/NorthWind.Tests/NorthWind.Tests.csproj
```

Abre `NorthWind.sln` y usa el perfil **NorthWind - App + Swagger + Mailtrap**. Los puertos siguen siendo 5100 para API/Swagger y 5200 para Blazor. El identificador de secretos de usuario y la conexión a `NorthWindPracticas8` se conservan.

La API contiene `Startup.cs`, `GlobalUsings.cs`, `Program.cs` y `Extensions`. El cliente se llama `NorthWind.Sales.BlazorClient` y mantiene `Layout`, `NavMenu.razor`, `Pages`, `App.razor` y `wwwroot`.

Verificado: compilación sin errores ni advertencias, 60 pruebas aprobadas con SQL Server, prueba HTTP de órdenes y membresía y modelo EF sin cambios pendientes.
