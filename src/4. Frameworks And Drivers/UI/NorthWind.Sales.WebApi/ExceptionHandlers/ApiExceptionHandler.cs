using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NorthWind.Exceptions.Entities;
namespace NorthWind.Sales.WebApi.ExceptionHandlers;

public sealed class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        ProblemDetails problem = exception switch
        {
            ValidationException validation => new()
            {
                Status = 400, Title = "Los datos ingresados no son válidos.",
                Extensions = { ["errors"] = validation.Errors.GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.Message).ToArray()) }
            },
            StockConflictException stock => new() { Status = 409, Title = "Las existencias o el precio cambiaron.", Detail = stock.Message },
            BadHttpRequestException => new() { Status = 400, Title = "Los datos enviados no tienen un formato válido." },
            UnauthorizedAccessException => new() { Status = 401, Title = "Debes iniciar sesión." },
            UpdateException => new() { Status = 500, Title = "No se pudieron guardar los cambios." },
            NotificationException notification => new() { Status = 500, Title = "No se pudo enviar la notificación.", Detail = notification.Message },
            _ => new() { Status = 500, Title = "Ocurrió un error inesperado. Inténtalo nuevamente." }
        };
        problem.Instance = context.Request.Path;
        problem.Extensions["traceId"] = context.TraceIdentifier;
        if (problem.Status >= 500) logger.LogError(exception, "Request failed {TraceId}", context.TraceIdentifier);
        context.Response.StatusCode = problem.Status ?? 500;
        await context.Response.WriteAsJsonAsync(problem, (JsonSerializerOptions?)null, "application/problem+json", cancellationToken);
        return true;
    }
}
