# Práctica 06: Frameworks And Drivers

Conectar EF Core y SQL Server, la API con Swagger y el cliente Blazor WebAssembly.

## Implementación

NorthWindContext contiene el mapeo y la migración inicial. IoC registra implementaciones. Los proyectos de entrada son Sales.WebApi y Sales.BlazorClient.

## Archivos principales

- [NorthWindContext.cs](../src/4.%20Frameworks%20And%20Drivers/Frameworks/NorthWind.Sales.Backend.DataContexts.EFCore/DataContexts/NorthWindContext.cs)
- [Program.cs](../src/4.%20Frameworks%20And%20Drivers/UI/NorthWind.Sales.WebApi/Program.cs)
- [Program.cs](../src/4.%20Frameworks%20And%20Drivers/UIs/NorthWind.Sales.BlazorClient/Program.cs)

## Comprobación

Ejecuta las dos aplicaciones siguiendo README.md. En Development las migraciones se aplican al arrancar la API.

La solución es acumulativa: esta funcionalidad forma parte del estado final de las 16 guías.
