using NorthWind.Events.Entities;
namespace NorthWind.Sales.Backend.BusinessObjects.Events;
public sealed record SpecialOrderCreatedEvent(int OrderId, int ProductsCount) : IDomainEvent;
