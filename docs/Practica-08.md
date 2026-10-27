# Práctica 08: Manejo de excepciones

Notificar errores mediante excepciones propias y respuestas ProblemDetails.

## Implementación

ApiExceptionHandler traduce validación a 400, acceso no autenticado a 401, persistencia a 500, notificación fallida a 500 y fallos inesperados a 500. Los errores de persistencia siguen el estado HTTP de la práctica 8. La lista de errores de validación se serializa explícitamente en la extensión errors de ProblemDetails para que llegue completa al cliente. ExceptionDelegatingHandler recupera los errores de la API y ExceptionDisplay los muestra en Blazor.

## Archivos principales

- [ApiExceptionHandler.cs](../src/4.%20Frameworks%20And%20Drivers/UI/NorthWind.Sales.WebApi/ExceptionHandlers/ApiExceptionHandler.cs)
- [HttpHandlers.cs](../src/1.%20Enterprise%20Business%20Rules/NorthWind.HttpDelegatingHandlers/HttpHandlers.cs)
- [ExceptionDisplay.razor](../src/3.%20Interface%20Adapters/Views/NorthWind.RazorComponents/Exceptions/ExceptionDisplay.razor)

## Comprobación

Envía una orden inválida desde Swagger y verifica application/problem+json y errors. No se exponen mensajes internos de SQL.

La solución es acumulativa: esta funcionalidad forma parte del estado final de las 16 guías.
