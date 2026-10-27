namespace NorthWind.Sales.Entities.Dtos.CreateOrder;

public sealed record CreateOrderDetailDto(int ProductId, decimal UnitPrice, short Quantity);
public sealed record CreateOrderDto(string CustomerId, string ShipAddress, string ShipCity,
    string ShipCountry, string? ShipPostalCode, IReadOnlyList<CreateOrderDetailDto> OrderDetails);
