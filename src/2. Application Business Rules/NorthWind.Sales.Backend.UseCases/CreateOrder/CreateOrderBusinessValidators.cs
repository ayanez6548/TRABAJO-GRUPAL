using NorthWind.Validation.Entities;
using NorthWind.Sales.Backend.BusinessObjects.Interfaces;
using NorthWind.Sales.Entities.Dtos.CreateOrder;
namespace NorthWind.Sales.Backend.UseCases.CreateOrder;

public sealed class CreateOrderCustomerValidator(IQueriesRepository repository) : IModelValidator<CreateOrderDto>
{
    public ValidationConstraint Constraint => ValidationConstraint.ValidateIfThereAreNoPreviousErrors;
    public async Task<IReadOnlyList<ValidationError>> Validate(CreateOrderDto model, CancellationToken cancellationToken = default)
    {
        var balance = await repository.GetCustomerCurrentBalance(model.CustomerId, cancellationToken);
        return balance is null ? [new("CustomerId", "El cliente no existe.")]
            : balance > 0 ? [new("CustomerId", "El cliente tiene un saldo pendiente.")] : [];
    }
}
public sealed class CreateOrderProductValidator(IQueriesRepository repository) : IModelValidator<CreateOrderDto>
{
    public ValidationConstraint Constraint => ValidationConstraint.ValidateIfThereAreNoPreviousErrors;
    public async Task<IReadOnlyList<ValidationError>> Validate(CreateOrderDto model, CancellationToken cancellationToken = default)
    {
        var required = model.OrderDetails.GroupBy(d => d.ProductId)
            .Select(g => new { ProductId = g.Key, Quantity = g.Sum(d => (int)d.Quantity) }).ToArray();
        var stock = (await repository.GetProductsUnitsInStock(required.Select(d => d.ProductId), cancellationToken))
            .ToDictionary(p => p.ProductId, p => p.UnitsInStock);
        List<ValidationError> errors = [];
        foreach (var item in required)
        {
            int index = model.OrderDetails.Select((detail, position) => (detail, position))
                .First(pair => pair.detail.ProductId == item.ProductId).position;
            if (!stock.TryGetValue(item.ProductId, out var available))
                errors.Add(new($"OrderDetails[{index}].ProductId", $"El producto {item.ProductId} no existe."));
            else if (available < item.Quantity)
                errors.Add(new($"OrderDetails[{index}].Quantity", $"Existencias insuficientes para el producto {item.ProductId}: se solicitaron {item.Quantity} y hay {available} disponibles."));
        }
        return errors;
    }
}
