using NorthWind.Sales.Backend.BusinessObjects.Aggregates;
using NorthWind.Sales.Backend.BusinessObjects.Interfaces;
using NorthWind.Sales.Backend.BusinessObjects.POCOEntities;
using NorthWind.Sales.Backend.Repositories.Interfaces;
namespace NorthWind.Sales.Backend.Repositories.Repositories;
public sealed class CommandsRepository(INorthWindSalesCommandsDataContext context) : ICommandsRepository
{
    public async Task CreateOrder(OrderAggregate order, CancellationToken cancellationToken = default)
    {
        await context.ReserveStockAsync(order.OrderDetails.Select(d => new Entities.OrderDetail {
            ProductId = d.ProductId, UnitPrice = d.UnitPrice, Quantity = d.Quantity }), cancellationToken);
        var stored = new Order { CustomerId = order.CustomerId, ShipAddress = order.ShipAddress,
            ShipCity = order.ShipCity, ShipCountry = order.ShipCountry, ShipPostalCode = order.ShipPostalCode,
            ShippingType = order.ShippingType, DiscountType = order.DiscountType, Discount = order.Discount,
            OrderDate = order.OrderDate, CreatedBy = order.CreatedBy, VatRate = order.VatRate };
        await context.AddOrderAsync(stored, cancellationToken);
        await context.AddOrderDetailsAsync(order.OrderDetails.Select(d => new Entities.OrderDetail {
            Order = stored, ProductId = d.ProductId, UnitPrice = d.UnitPrice, Quantity = d.Quantity }), cancellationToken);
        pending = (order, stored);
    }
    private (OrderAggregate Aggregate, Order Stored)? pending;
    public async Task SaveChanges(CancellationToken cancellationToken = default)
    {
        await context.SaveChangesAsync(cancellationToken);
        if (pending is { } pair) pair.Aggregate.Id = pair.Stored.Id;
        pending = null;
    }
}
