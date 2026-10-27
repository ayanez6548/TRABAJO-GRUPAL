# Práctica 01: Introducción

Identificar la regla de dependencia y las cuatro capas de Clean Architecture.

## Implementación

La solución separa contratos y dominio de ASP.NET Core, EF Core y Blazor. Las capas internas no dependen de implementaciones de infraestructura.

## Archivos principales

- [NorthWind.sln](../NorthWind.sln)
- [Directory.Build.props](../Directory.Build.props)

## Comprobación

Revisa las referencias entre proyectos en Visual Studio. Entities no contiene referencias a EF Core, Blazor ni ASP.NET Core.

La solución es acumulativa: esta funcionalidad forma parte del estado final de las 16 guías.
