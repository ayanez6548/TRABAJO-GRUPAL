using System.Text.RegularExpressions;
using NorthWind.Sales.Entities.Dtos.CreateOrder;
using NorthWind.Sales.Entities.Dtos;
using NorthWind.Validation.Entities;

namespace NorthWind.Sales.Validators.Entities;

public sealed class CreateOrderDtoValidator : IModelValidator<CreateOrderDto>
{
    public ValidationConstraint Constraint => ValidationConstraint.AlwaysValidate;

    public Task<IReadOnlyList<ValidationError>> Validate(CreateOrderDto model, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        List<ValidationError> errors = [];
        if (string.IsNullOrWhiteSpace(model.CustomerId) || !Regex.IsMatch(model.CustomerId, "^[a-zA-Z0-9]{5}$"))
            errors.Add(new("CustomerId", "El código del cliente debe tener exactamente 5 letras o números."));
        CheckText(model.ShipAddress, "ShipAddress", 1, SalesFieldLimits.Address, errors);
        CheckText(model.ShipCity, "ShipCity", 3, SalesFieldLimits.City, errors, true);
        CheckText(model.ShipCountry, "ShipCountry", 3, SalesFieldLimits.Country, errors, true);
        if (!string.IsNullOrEmpty(model.ShipPostalCode) && (model.ShipPostalCode.Length > SalesFieldLimits.PostalCode
            || !Regex.IsMatch(model.ShipPostalCode, @"^[0-9]+$")))
            errors.Add(new("ShipPostalCode", "El código postal es opcional y admite hasta 10 dígitos, sin letras ni espacios."));
        if (model.OrderDetails is null || model.OrderDetails.Count == 0)
            errors.Add(new("OrderDetails", "Agrega al menos un producto."));
        else
        {
            if (model.OrderDetails.Where(d => d is not null).GroupBy(d => d.ProductId).Any(group => group.Count() > 1))
                errors.Add(new("OrderDetails", "Un producto solo puede aparecer una vez. Modifica la línea existente."));
            for (int index = 0; index < model.OrderDetails.Count; index++)
            {
                var detail = model.OrderDetails[index];
                if (detail is null) errors.Add(new($"OrderDetails[{index}]", "El detalle de la orden es obligatorio."));
                else errors.AddRange(CreateOrderDetailDtoValidator.ValidateDetail(detail, $"OrderDetails[{index}]"));
            }
            if (model.OrderDetails.Where(d => d is not null).GroupBy(d => d.ProductId)
                .Any(group => group.Sum(d => (long)d.Quantity) > short.MaxValue))
                errors.Add(new("OrderDetails", "La cantidad por producto no puede superar 32767."));
        }
        return Task.FromResult<IReadOnlyList<ValidationError>>(errors);
    }

    private static void CheckText(string? value, string property, int minimum, int maximum, List<ValidationError> errors, bool lettersOnly = false)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Trim().Length < minimum || value.Length > maximum
            || !Regex.IsMatch(value, lettersOnly ? @"^ *\p{L}[\p{L}\p{M} ]*$" : @"^[\p{L}\p{M}\p{N} #.,/\-]+$"))
            errors.Add(new(property, lettersOnly
                ? $"Ingresa entre {minimum} y {maximum} caracteres: solo letras y espacios, incluidas tildes y ñ."
                : $"La dirección debe tener entre {minimum} y {maximum} caracteres: letras, números, espacios o los signos # . , / - ."));
    }
}

public sealed class CreateOrderDetailDtoValidator : IModelValidator<CreateOrderDetailDto>
{
    public ValidationConstraint Constraint => ValidationConstraint.AlwaysValidate;
    public Task<IReadOnlyList<ValidationError>> Validate(CreateOrderDetailDto model, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<IReadOnlyList<ValidationError>>(ValidateDetail(model, ""));
    }
    internal static IReadOnlyList<ValidationError> ValidateDetail(CreateOrderDetailDto model, string prefix)
    {
        string Property(string name) => prefix.Length == 0 ? name : $"{prefix}.{name}";
        List<ValidationError> errors = [];
        if (model.ProductId <= 0) errors.Add(new(Property("ProductId"), "Selecciona un producto válido."));
        if (model.UnitPrice <= 0 || model.UnitPrice > 999999.99m || decimal.Round(model.UnitPrice, 2) != model.UnitPrice)
            errors.Add(new(Property("UnitPrice"), "El precio debe ser mayor que cero y tener como máximo 6 dígitos enteros y 2 decimales."));
        if (model.Quantity <= 0) errors.Add(new(Property("Quantity"), "La cantidad debe ser mayor que cero."));
        return errors;
    }
}
