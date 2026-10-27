using Microsoft.AspNetCore.Builder;
using NorthWind.Entities.Interfaces;
using NorthWind.Sales.Entities.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using NorthWind.Result.Entities;
using NorthWind.Membership.Backend.Core.Interfaces;
using NorthWind.Membership.Entities.Dtos;
using NorthWind.Sales.Backend.BusinessObjects.Interfaces;
using NorthWind.Sales.Entities.Dtos.CreateOrder;
using SalesEndpoints = NorthWind.Sales.Entities.ValueObjects.Endpoints;
using MemberEndpoints = NorthWind.Membership.Entities.ValueObjects.Endpoints;
namespace NorthWind.Sales.Backend.Controllers;

public static class EndpointsContainer
{
    public static IEndpointRouteBuilder MapNorthWindSalesEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost(SalesEndpoints.CreateOrder, async (CreateOrderDto dto, ICreateOrderInputPort port,
            ICreateOrderOutputPort presenter, CancellationToken ct) =>
        {
            await port.Handle(dto, ct);
            return Results.Ok(presenter.OrderId);
        }).RequireAuthorization().WithTags("Órdenes").WithName("CreateOrder");
        app.MapGet(SalesEndpoints.Customers, async (IQueriesRepository repository, CancellationToken ct) =>
            Results.Ok(await repository.GetCustomers(ct))).RequireAuthorization().WithTags("Catálogos");
        app.MapGet(SalesEndpoints.Products, async (IQueriesRepository repository, CancellationToken ct) =>
            Results.Ok(await repository.GetProducts(ct))).RequireAuthorization().WithTags("Catálogos");
        app.MapPost(MemberEndpoints.Register, async (UserRegistrationDto dto, IUserRegistrationInputPort input,
            IUserRegistrationOutputPort presenter, CancellationToken ct) =>
        {
            await input.Handle(dto, ct);
            return ToHttp(presenter.Result ?? throw new InvalidOperationException("The presenter did not receive a result."));
        }).AllowAnonymous().WithTags("Usuarios");
        app.MapPost(MemberEndpoints.Login, async (UserCredentialsDto dto, IUserLoginInputPort input,
            IUserLoginOutputPort presenter, CancellationToken ct) =>
        {
            await input.Handle(dto, ct);
            var result = presenter.Result ?? throw new InvalidOperationException("The presenter did not receive a result.");
            if (result.Errors.Any(e => e.PropertyName == "Credentials")) return Results.Problem(statusCode: 401, title: "El correo electrónico o la contraseña son incorrectos.");
            return ToHttp(result);
        }).AllowAnonymous().WithTags("Usuarios");
        app.MapGet("/catalog/customers/search", SearchCustomers).RequireAuthorization().WithTags("Catálogos");
        app.MapGet("/catalog/products/search", SearchProducts).RequireAuthorization().WithTags("Catálogos");
        app.MapGet("/sales/settings", (ISalesSettings settings) => Results.Ok(new SalesSettingsDto(10m, settings.VatRate)))
            .RequireAuthorization().WithTags("Ventas");
        app.MapGet("/orders", async (int? page, int? pageSize, IQueriesRepository repository, IUserService user, CancellationToken ct) =>
        {
            int number = page ?? 1, size = pageSize ?? 5;
            if (InvalidPage(null, number, size)) return (IResult)Results.ValidationProblem(new Dictionary<string, string[]>
                { ["History"] = ["Usa una página entre 1 y 100000 y entre 1 y 50 facturas por página."] }, title: "Revisa los datos del historial.");
            return Results.Ok(await repository.GetInvoiceHistory(user.UserName, number, size, ct));
        }).RequireAuthorization().WithTags("Facturas");
        app.MapGet("/orders/{id:int}", async (int id, IQueriesRepository repository, IUserService user, CancellationToken ct) =>
        {
            var invoice = await repository.GetInvoice(id, user.UserName, ct);
            return invoice is null ? (IResult)Results.NotFound(new { message = "No se encontró la orden solicitada." }) : Results.Ok(invoice);
        }).RequireAuthorization().WithTags("Facturas");
        app.MapGet("/orders/{id:int}/invoice.pdf", async (int id, IQueriesRepository repository, IUserService user,
            IInvoicePdfExporter exporter, CancellationToken ct) =>
        {
            var invoice = await repository.GetInvoice(id, user.UserName, ct);
            return invoice is null ? (IResult)Results.NotFound(new { message = "No se encontró la orden solicitada." })
                : Results.File(exporter.Export(invoice), "application/pdf", $"NorthWind-Factura-{id}.pdf");
        }).RequireAuthorization().WithTags("Facturas");
        return app;
    }
    private static bool InvalidPage(string? q, int page, int pageSize) =>
        page < 1 || page > 100000 || pageSize < 1 || pageSize > 50 || q?.Length > 100;
    private static async Task<IResult> SearchCustomers(string? q, string? code, string? name, decimal? minBalance, decimal? maxBalance, int? page, int? pageSize, IQueriesRepository repository, CancellationToken ct)
    {
        int number = page ?? 1, size = pageSize ?? 10;
        if (InvalidPage(q, number, size)) return Results.ValidationProblem(new Dictionary<string,string[]> { ["Search"] = ["Usa una página mayor o igual a 1, entre 1 y 50 resultados por página y hasta 100 caracteres de búsqueda."] }, title: "Los datos de búsqueda no son válidos.");
        var errors = new Dictionary<string, string[]>();
        if (code?.Length > 5 || code?.Any(character => !char.IsAsciiLetterOrDigit(character)) == true)
            errors["Code"] = ["El código del cliente admite hasta 5 letras o números."];
        if (name?.Length > 100) errors["Name"] = ["El nombre admite hasta 100 caracteres."];
        ValidateRange(errors, "Balance", "saldo", minBalance, maxBalance, 999999.99m, true);
        if (errors.Count > 0) return Results.ValidationProblem(errors, title: "Revisa los filtros de búsqueda.");
        return Results.Ok(await repository.SearchCustomers(q, number, size, ct, new(code, name, minBalance, maxBalance)));
    }
    private static async Task<IResult> SearchProducts(string? q, int? code, string? name, decimal? minPrice, decimal? maxPrice, int? minStock, int? maxStock, int? page, int? pageSize, IQueriesRepository repository, CancellationToken ct)
    {
        int number = page ?? 1, size = pageSize ?? 10;
        if (InvalidPage(q, number, size)) return Results.ValidationProblem(new Dictionary<string,string[]> { ["Search"] = ["Usa una página mayor o igual a 1, entre 1 y 50 resultados por página y hasta 100 caracteres de búsqueda."] }, title: "Los datos de búsqueda no son válidos.");
        var errors = new Dictionary<string, string[]>();
        if (code <= 0) errors["Code"] = ["El código del producto debe ser un entero positivo."];
        if (name?.Length > 100) errors["Name"] = ["El nombre admite hasta 100 caracteres."];
        ValidateRange(errors, "Price", "precio", minPrice, maxPrice, 999999.99m, true);
        ValidateRange(errors, "Stock", "existencias", minStock, maxStock, short.MaxValue, false);
        if (minStock == 0 || maxStock == 0) errors["Stock"] = ["Las existencias deben estar entre 1 y 32767."];
        if (errors.Count > 0) return Results.ValidationProblem(errors, title: "Revisa los filtros de búsqueda.");
        return Results.Ok(await repository.SearchProducts(q, number, size, ct, new(code, name, minPrice, maxPrice, minStock, maxStock)));
    }
    private static void ValidateRange(Dictionary<string, string[]> errors, string field, string label, decimal? minimum, decimal? maximum, decimal limit, bool money)
    {
        if (minimum < 0 || maximum < 0 || minimum > limit || maximum > limit)
            errors[field] = [$"El filtro de {label} debe estar entre 0 y {limit}."];
        else if (minimum > maximum) errors[field] = [$"El mínimo de {label} no puede superar el máximo."];
        else if (money && (minimum is decimal min && decimal.Round(min, 2) != min || maximum is decimal max && decimal.Round(max, 2) != max))
            errors[field] = [$"El filtro de {label} admite hasta dos decimales."];
    }
    private static IResult ToHttp<T>(Result<T> result) => result.HasError
        ? Results.ValidationProblem(result.Errors.GroupBy(e => e.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(e => e.Message).ToArray()), title: "Revisa los datos ingresados.")
        : Results.Ok(result.Value);
}
