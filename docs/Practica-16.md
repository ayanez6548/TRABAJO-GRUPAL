# Práctica 16: Configuración de autenticación y autorización

Configurar JWT Bearer en API, Swagger y Blazor.

## Implementación

La API valida firma, emisor, audiencia y expiración. Swagger incluye Authorize. Blazor guarda TokensDto en localStorage, usa AuthenticationStateProvider, AuthorizeRouteView y un DelegatingHandler que adjunta el token. El cierre de sesión borra el almacenamiento.

## Archivos principales

- [Program.cs](../src/4.%20Frameworks%20And%20Drivers/UI/NorthWind.Sales.WebApi/Program.cs)
- [JWTAuthenticationStateProvider.cs](../src/3.%20Interface%20Adapters/Views/NorthWind.Membership.Frontend.RazorViews/AuthenticationStateProviders/JWTAuthenticationStateProvider.cs)
- [HttpHandlers.cs](../src/1.%20Enterprise%20Business%20Rules/NorthWind.HttpDelegatingHandlers/HttpHandlers.cs)

## Comprobación

Inicia sesión, recarga el navegador, crea una orden y cierra sesión. Sin sesión se redirige a login. Un token alterado o vencido debe recibir 401 de la API; el estado del navegador no sustituye esa validación.

La solución es acumulativa: esta funcionalidad forma parte del estado final de las 16 guías.
