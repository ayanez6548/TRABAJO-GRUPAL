using NorthWind.Events.Entities;
using NorthWind.Entities.Interfaces;
using NorthWind.Sales.Backend.BusinessObjects.Events;
namespace NorthWind.Sales.Backend.UseCases.CreateOrder;
public sealed class SendEMailWhenSpecialOrderCreatedEventHandler(IMailService mail) : IDomainEventHandler<SpecialOrderCreatedEvent>
{
    public Task Handle(SpecialOrderCreatedEvent domainEvent, CancellationToken cancellationToken = default) =>
        mail.SendMailToAdministrator("Orden especial creada",
            $"Se creó la orden {domainEvent.OrderId} con {domainEvent.ProductsCount} productos distintos.", cancellationToken);
}
