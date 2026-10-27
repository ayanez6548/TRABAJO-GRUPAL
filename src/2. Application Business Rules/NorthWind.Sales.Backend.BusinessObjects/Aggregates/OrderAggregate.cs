using NorthWind.Sales.Entities.Dtos.CreateOrder;
using NorthWind.Sales.Backend.BusinessObjects.POCOEntities;
using NorthWind.Sales.Backend.BusinessObjects.ValueObjects;
using NorthWind.Exceptions.Entities;
using NorthWind.Validation.Entities;
namespace NorthWind.Sales.Backend.BusinessObjects.Aggregates;

public sealed class OrderAggregate : Order
{
    private readonly List<OrderDetail> details = [];
    public IReadOnlyCollection<OrderDetail> OrderDetails => details.AsReadOnly();
    public void AddDetail(int productId, decimal unitPrice, short quantity)
    {
        var existing = details.FirstOrDefault(d => d.ProductId == productId);
        if (existing is not null)
            throw new ValidationException([new("OrderDetails", "Un producto solo puede aparecer una vez en la orden.")]);
        int total = quantity;
        if (total > short.MaxValue || total <= 0)
            throw new ValidationException([new("OrderDetails", "La cantidad debe estar entre 1 y 32767.")]);
        details.Add(new(productId, unitPrice, (short)total));
    }
    public static OrderAggregate From(CreateOrderDto dto)
    {
        var order = new OrderAggregate { CustomerId = dto.CustomerId, ShipAddress = dto.ShipAddress,
            ShipCity = dto.ShipCity, ShipCountry = dto.ShipCountry, ShipPostalCode = dto.ShipPostalCode };
        foreach (var detail in dto.OrderDetails) order.AddDetail(detail.ProductId, detail.UnitPrice, detail.Quantity);
        return order;
    }
}
