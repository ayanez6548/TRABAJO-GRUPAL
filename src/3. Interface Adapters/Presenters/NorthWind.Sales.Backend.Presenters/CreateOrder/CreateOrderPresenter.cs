using NorthWind.Sales.Backend.BusinessObjects.Aggregates;
using NorthWind.Sales.Backend.BusinessObjects.Interfaces;
namespace NorthWind.Sales.Backend.Presenters.CreateOrder;
public sealed class CreateOrderPresenter : ICreateOrderOutputPort
{
    public int OrderId { get; private set; }
    public Task Handle(OrderAggregate order, CancellationToken cancellationToken = default)
    {
        OrderId = order.Id;
        return Task.CompletedTask;
    }
}
