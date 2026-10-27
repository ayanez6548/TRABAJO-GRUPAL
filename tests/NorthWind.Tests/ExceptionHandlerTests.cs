using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NorthWind.Exceptions.Entities;
using NorthWind.Sales.WebApi.ExceptionHandlers;
namespace NorthWind.Tests;

[TestClass]
public sealed class ExceptionHandlerTests
{
    [TestMethod]
    [DataRow("validation", 400)]
    [DataRow("unauthorized", 401)]
    [DataRow("update", 500)]
    [DataRow("notification", 500)]
    [DataRow("unexpected", 500)]
    public async Task Practice08_UsesProblemDetailsAndDocumentedStatusCodes(string failure, int expected)
    {
        Exception exception = failure switch
        {
            "validation" => new ValidationException([new("CustomerId", "El cliente es requerido.")]),
            "unauthorized" => new UnauthorizedAccessException(),
            "update" => new UpdateException("DETALLE_INTERNO_SQL"),
            "notification" => new NotificationException("La orden fue cancelada."),
            _ => new InvalidOperationException("DETALLE_INTERNO_SQL")
        };
        using var services = new ServiceCollection().AddLogging().BuildServiceProvider();
        var context = new DefaultHttpContext { RequestServices = services };
        context.Request.Path = "/CreateOrder";
        await using var body = new MemoryStream();
        context.Response.Body = body;
        bool handled = await new ApiExceptionHandler(NullLogger<ApiExceptionHandler>.Instance)
            .TryHandleAsync(context, exception, CancellationToken.None);
        Assert.IsTrue(handled);
        Assert.AreEqual(expected, context.Response.StatusCode);
        StringAssert.StartsWith(context.Response.ContentType!, "application/problem+json");
        body.Position = 0;
        using var document = await JsonDocument.ParseAsync(body);
        Assert.AreEqual(expected, document.RootElement.GetProperty("status").GetInt32());
        bool exposesInternalFailure = document.RootElement.ToString().Contains("DETALLE_INTERNO_SQL");
        Assert.IsFalse(exposesInternalFailure);
        if (failure == "validation") Assert.IsTrue(document.RootElement.GetProperty("errors").TryGetProperty("CustomerId", out _));
    }
}
